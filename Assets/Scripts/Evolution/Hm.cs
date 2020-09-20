using System;
using System.Collections.Generic;
using System.Linq;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Api.Utils;
using Gameplay;
using ProceduralTree;
using UnityEngine;
using Utils;
using Quaternion = UnityEngine.Quaternion;
using Transform = Api.Realtime.Transform;
using Vector3 = UnityEngine.Vector3;

namespace Evolution
{
    /// <summary>
    /// Defines the level of details of prefab used, e.g. low = cubes only, fast perf
    /// </summary>
    enum GraphicTier
    {
        Low,
        Medium,
        High
    }
    
	/// <summary>
	/// The role of this manager is to request updates and update everything related to hosts.
	/// Single responsibility.
	/// </summary>
	public class Hm : Singleton<Hm> // TODO: fix ugly as hell min max stuff
    {
        [SerializeField] private GraphicTier graphicTier = GraphicTier.Low;
        [SerializeField] private GameObject lowPolyAnimalPrefab; // E.g. red cube
        [SerializeField] private GameObject lowPolyVegetationPrefab; // E.g. green cube

		/// <summary>
        /// Dictionary containing animals
        /// </summary>
        private readonly Dictionary<ulong, SimpleAnimal> _animals = new Dictionary<ulong, SimpleAnimal>();

		/// <summary>
		/// Dictionary containing vegetations
		/// </summary>
		private readonly Dictionary<ulong, Vegetation> _vegetations = new Dictionary<ulong, Vegetation>();

        /// <summary>
        /// Next id to give for new host
        /// </summary>
        private ulong _nextId;

        #region MONO

        protected override void Awake()
        {
            base.Awake();
            switch (graphicTier)
            {
                case GraphicTier.Low:
                    Pool.Preload(lowPolyAnimalPrefab, 100);
                    Pool.Preload(lowPolyVegetationPrefab, 100);
                    break;
                case GraphicTier.Medium:
                    Pool.Preload(lowPolyAnimalPrefab, 100);
                    TreePool.instance.FillSlowly(100);
                    break;
                case GraphicTier.High:
                    Pool.Preload(lowPolyAnimalPrefab, 100);
                    TreePool.instance.FillSlowly(100);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Start()
        {
            if (!Gm.instance.online) return;
            Mcm.instance.Initialized += Initialized;
        }

        protected override void OnDestroy()
        {
            if (Gm.instance == null || !Gm.instance.online) return;
	        Mcm.instance.Initialized -= Initialized;
	        Mcm.instance.AnimalSpawned -= OnAnimalSpawned;
	        Mcm.instance.AnimalDestroyed -= OnAnimalDestroyed;
	        Mcm.instance.TreeSpawned -= OnTreeSpawned;
	        Mcm.instance.TreeDestroyed -= OnTreeDestroyed;
	        Mcm.instance.AnimalSpawnRequested -= OnAnimalSpawnRequested;
	        Mcm.instance.AnimalDestroyRequested -= OnAnimalDestroyRequested;
	        Mcm.instance.TreeSpawnRequested -= OnTreeSpawnRequested;
	        Mcm.instance.TreeDestroyRequested -= OnTreeDestroyRequested;
	        Mcm.instance.MemeUpdated -= OnMemeUpdated;
	        Mcm.instance.TransformUpdated -= OnTransformUpdated;
	        Mcm.instance.NavMeshUpdated -= OnNavMeshUpdated;
        }

        #endregion

        #region PUBLIC METHODS

        public void InitializeNetworkHandlers()
        {
            // Now that we are initialized, can handle gameplay
            Mcm.instance.AnimalSpawned += OnAnimalSpawned;
            Mcm.instance.AnimalDestroyed += OnAnimalDestroyed;
            Mcm.instance.TreeSpawned += OnTreeSpawned;
            Mcm.instance.TreeDestroyed += OnTreeDestroyed;
            Mcm.instance.AnimalSpawnRequested += OnAnimalSpawnRequested;
            Mcm.instance.AnimalDestroyRequested += OnAnimalDestroyRequested;
            Mcm.instance.TreeSpawnRequested += OnTreeSpawnRequested;
            Mcm.instance.TreeDestroyRequested += OnTreeDestroyRequested;
            Mcm.instance.MemeUpdated += OnMemeUpdated;
            Mcm.instance.TransformUpdated += OnTransformUpdated;
            Mcm.instance.NavMeshUpdated += OnNavMeshUpdated;
        }

        /// <summary>
        /// Ask executors to spawn an animal
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        public void RequestSpawnAnimal(Vector3 p, Quaternion r)
			=> Mcm.instance.RpcAsync(new Packet().Basic(p.Net()).ReqSpawnAnimal(p, r));

        /// <summary>
        /// Ask executors to spawn a tree
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        public void RequestSpawnTree(Vector3 p, Quaternion r)
	        => Mcm.instance.RpcAsync(new Packet().Basic(p.Net()).ReqSpawnTree(p, r));

        /// <summary>
        /// Spawn animal and sync if online
        /// </summary>
        /// <param name="p">Position</param>
        /// <param name="r">Rotation</param>
        /// <param name="c"></param>
        /// <returns></returns>
        public CommonAnimal SpawnAnimalSync(Vector3 p, Quaternion r, Characteristics c, Characteristics cMin, Characteristics cMax)
        {
            _nextId++;
            var animal = SpawnAnimal(p, r, c, cMin, cMax);
            if (!Gm.instance.online) return animal;
            var packet = new Packet().Basic(p.Net()).SpawnAnimal(_nextId, p, r);
            Mcm.instance.RpcAsync(packet);
            return animal;
        }

        /// <summary>
        /// Notify animal destroy and destroy locally
        /// </summary>
        /// <param name="id"></param>
        public void DestroyAnimalSync(ulong id)
        {
            var animal = DestroyAnimal(id);
            if (!Gm.instance.online) return;
            var p = animal.transform.position;
	        var packet = new Packet().Basic(p.Net()).DestroyAnimal(id);
	        Mcm.instance.RpcAsync(packet);
        }

        /// <summary>
        /// Notify tree spawn and spawn locally
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <param name="cMin"></param>
        /// <param name="cMax"></param>
        /// <returns></returns>
        public Vegetation SpawnTreeSync(Vector3 p, Quaternion r, Characteristics c, Characteristics cMin, Characteristics cMax)
        {
            _nextId++;
            var veg = SpawnTree(p, r, c, cMin, cMax);
            if (!Gm.instance.online) return veg;
	        var packet = new Packet().Basic(p.Net()).SpawnTree(_nextId, p, r);
	        Mcm.instance.RpcAsync(packet);
            return veg;
        }

        public void DestroyTreeSync(ulong id)
        {
            var tree = DestroyTree(id);
            if (!Gm.instance.online) return;
            var p = tree.transform.position;
            var packet = new Packet().Basic(p.Net()).DestroyTree(id);
	        Mcm.instance.RpcAsync(packet);
        }

        /// <summary>
        /// De-spawn every hosts
        /// </summary>
        public void Reset()
        {
            _animals.Keys.ToList().ForEach(DestroyAnimalSync);
            _vegetations.Keys.ToList().ForEach(DestroyTreeSync);
        }

        public void Pause()
        {
            _animals.Values.ToList().ForEach(a => a.controller.aiActive = false);
            _vegetations.Values.ToList().ForEach(v => v.controller.aiActive = false);
        }
        
        public void Play()
        {
            _animals.Values.ToList().ForEach(a => a.controller.aiActive = true);
            _vegetations.Values.ToList().ForEach(v => v.controller.aiActive = true);
        }

        public void StartExperience(Experience e)
        {
            Reset();
            var areaRadius = Mathf.Pow(2, (float) e.Map.Size*0.9f);
            var middleOfMap = new Vector3(areaRadius, 0, areaRadius);
            for (ulong i = 0; i < e.AnimalDistribution.InitialAmount; i++)
            {
                // TODO: debug breakpoint here
                // Random position within the map spaced according to a given scattering
                var pos = middleOfMap.RandomPositionAroundAboveGroundWithDistance(areaRadius,
                    LayerMask.GetMask("Animal"),
                    e.AnimalDistribution.Scattering,
                    (float) e.Map.Height);
                if (Vector3.positiveInfinity.Equals(pos)) continue; // TODO: fix this
                var a = SpawnAnimal(pos, Quaternion.identity, 
                    e.AnimalCharacteristics,
                    e.AnimalCharacteristicsMinimumBound,
                    e.AnimalCharacteristicsMaximumBound);
            }
            
            for (ulong i = 0; i < e.VegetationDistribution.InitialAmount; i++)
            {
                // Random position within the map spaced according to a given scattering
                var pos = middleOfMap.RandomPositionAroundAboveGroundWithDistance(areaRadius,
                    LayerMask.GetMask("Vegetation"),
                    e.VegetationDistribution.Scattering,
                    (float) e.Map.Height);
                if (Vector3.positiveInfinity.Equals(pos)) continue; // TODO: fix this
                var a = SpawnTree(pos, Quaternion.identity, 
                    e.VegetationCharacteristics,
                    e.VegetationCharacteristicsMinimumBound,
                    e.VegetationCharacteristicsMaximumBound);
            }


            Play();
        }
        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// A client's game play has been initialized
        /// </summary>
        /// <param name="senderId"></param>
        private void Initialized(string senderId)
        {
            var syncingGlobalState = Sm.instance.isServer;
	        var m = syncingGlobalState ? " sending global state" : "";
	        Debug.Log($"Player {senderId} game play is initialized{m}");
            // TODO: unit test ?
         //    if (!syncingGlobalState) return;
	        // // Slight different use case: client joined, i'm server, send him the current state and return
         //    foreach (var kv in m_Animals)
         //    {
         //        var t = kv.Value.transform;
         //        var position = t.position;
         //        var p = new Packet().Basic(position.Net())
         //            .SpawnAnimal(kv.Key, position, t.rotation);
         //        p.Recipients.Add(senderId);
         //        MatchCommunicationManager.instance.RpcAsync(p);
         //    }
         //    foreach (var kv in m_Trees)
         //    {
         //        var t = kv.Value.transform;
         //        var position = t.position;
         //        var p = new Packet().Basic(position.Net())
         //            .SpawnTree(kv.Key, position, t.rotation);
         //        p.Recipients.Add(senderId);
         //        MatchCommunicationManager.instance.RpcAsync(p);
         //    }
        }
        
        private void OnAnimalSpawned(Transform obj) =>
            SpawnAnimal(obj.Position.ToVector3(), obj.Rotation.ToQuaternion(), 
                Gm.instance.Experience.AnimalCharacteristics,
                Gm.instance.Experience.AnimalCharacteristicsMinimumBound,
                Gm.instance.Experience.AnimalCharacteristicsMaximumBound);
        private void OnAnimalDestroyed(Transform obj) => DestroyAnimal(obj.Id);
        private void OnAnimalDestroyRequested(Transform obj) => throw new NotImplementedException();
        private void OnAnimalSpawnRequested(Transform obj)
        {
            
            // Someone requested an animal spawn, adjust to put it on top of ground
            obj.Position.Y = 100; // TODO: see @Utils.Spatial.PositionAboveGround
            var aboveGround = obj.Position.ToVector3().PositionAboveGround();
            obj.Position = aboveGround.Net();
            if (double.IsInfinity(obj.Position.Y))
            {
                Debug.LogWarning($"Someone requested an animal spawn outside map");
                return;
            }
	        var packet = new Packet().Basic(obj.Position);
	        obj.Id = ++_nextId;
	        packet.Spawn = new Spawn
	        {
		        Animal = new Animal
		        {
			        Transform = obj
		        }
	        };
            Debug.Log($"S1 requested animal spawn, approving at {obj.Position}");
            Mcm.instance.RpcAsync(packet);
	        SpawnAnimal(aboveGround, obj.Rotation.ToQuaternion(), 
                Gm.instance.Experience.AnimalCharacteristics,
                Gm.instance.Experience.AnimalCharacteristicsMinimumBound,
                Gm.instance.Experience.AnimalCharacteristicsMaximumBound);
        }

        private CommonAnimal SpawnAnimal(Vector3 p, Quaternion r, Characteristics c, Characteristics cMin, Characteristics cMax)
        {
            // Debug.Log($"Spawning animal {obj}");
            var a = Pool.Spawn(lowPolyAnimalPrefab, p, r);
            _animals[_nextId] = a.GetComponent<SimpleAnimal>();
            _animals[_nextId].characteristics = c;
            _animals[_nextId].characteristicsMin = cMin;
            _animals[_nextId].characteristicsMax = cMax;
            _animals[_nextId].id = _nextId;
            // Only server handle animal behaviours
            if (!Gm.instance.online || Sm.instance.isServer)
            {
                _animals[_nextId].EnableBehaviour(true);
                // var tSync = m_Animals[obj.Id].gameObject.AddComponent<TransformSync>();
                // tSync.id = obj.Id;
            }
            return _animals[_nextId];
        }

        private CommonAnimal DestroyAnimal(ulong id)
        {
	        if (!_animals.ContainsKey(id))
	        {
		        // TODO: fix
		        // Debug.LogError($"Tried to destroy in-existent animal {obj.Id}");
		        return null;
	        }

            var animal = _animals[id];
	        Pool.Despawn(animal.gameObject);
	        if (!_animals.Remove(id)) Debug.LogError($"Failed to remove animal {id}");
            return animal;
        }

        private void OnTreeSpawned(Transform obj) => 
            SpawnTree(obj.Position.ToVector3(), obj.Rotation.ToQuaternion(), 
                Gm.instance.Experience.VegetationCharacteristics,
                Gm.instance.Experience.VegetationCharacteristicsMinimumBound,
                Gm.instance.Experience.VegetationCharacteristicsMaximumBound);
        private void OnTreeDestroyed(Transform obj) => DestroyTree(obj.Id);
        private void OnTreeDestroyRequested(Transform obj) => throw new NotImplementedException();
        private void OnTreeSpawnRequested(Transform obj)
        {
	        Debug.Log($"S1 requested tree spawn");
            // Someone requested a tree spawn, adjust to put it on top of ground
            obj.Position.Y = 1000; // TODO: see @Utils.Spatial.PositionAboveGround
            var aboveGround = obj.Position.ToVector3().PositionAboveGround();
            obj.Position = aboveGround.Net();
	        var packet = new Packet().Basic(obj.Position);
	        obj.Id = ++_nextId;
	        packet.Spawn = new Spawn
	        {
		        Tree = new Api.Realtime.Tree
		        {
			        Transform = obj
		        }
	        };
	        Mcm.instance.RpcAsync(packet);
	        SpawnTree(aboveGround, obj.Rotation.ToQuaternion(), 
                Gm.instance.Experience.VegetationCharacteristics,
                Gm.instance.Experience.VegetationCharacteristicsMinimumBound,
                Gm.instance.Experience.VegetationCharacteristicsMaximumBound);
        }

        private Vegetation SpawnTree(Vector3 p, Quaternion r, Characteristics c, Characteristics cMin, Characteristics cMax)
        {
            var veg = graphicTier == GraphicTier.Low ? // TODO: for now ugly if else, better = OOP stuff: spawn something i dont care what it is
                Pool.Spawn(lowPolyVegetationPrefab, p, r).GetComponent<Vegetation>() : 
                TreePool.instance.Spawn(p, r).go.GetComponent<Vegetation>();
	        _vegetations[_nextId] = veg;
            _vegetations[_nextId].characteristics = c;
            _vegetations[_nextId].characteristicsMin = cMin;
            _vegetations[_nextId].characteristicsMax = cMax;
            _vegetations[_nextId].id = _nextId;

	        if (!Gm.instance.online || Sm.instance.isServer) _vegetations[_nextId].EnableBehaviour(true);
	        return _vegetations[_nextId];
        }


        private Vegetation DestroyTree(ulong id)
        {
	        if (!_vegetations.ContainsKey(id))
	        {
		        // Debug.LogError($"Tried to destroy in-existent tree {obj.Id}");
		        return null;
	        }

            var tree = _vegetations[id];
            if (graphicTier == GraphicTier.Low) Pool.Despawn(tree.gameObject);
            else TreePool.instance.Despawn(tree.gameObject);
	        _vegetations.Remove(id);
            return tree;
        }



        private void OnMemeUpdated(Meme obj)
        {
	        // If this id doesn't belong to animals, maybe it's a tree
	        if (_animals.ContainsKey(obj.Id))
	        {
		        var h = _animals[obj.Id];
		        if (!h.Memes.ContainsKey(obj.MemeName))
		        {
			        Debug.LogError($"Tried to update in-existent meme {obj.MemeName} on host {obj.Id}");
			        return;
		        }

		        // Transition to the new received meme
		        h.controller.Transition(h.Memes[obj.MemeName]);
	        }
	        else if (_vegetations.ContainsKey(obj.Id))
	        {
		        var h = _vegetations[obj.Id];
		        if (!h.Memes.ContainsKey(obj.MemeName))
		        {
			        Debug.LogError($"Tried to update in-existent meme {obj.MemeName} on host {obj.Id}");
			        return;
		        }

		        // Transition to the new received meme
		        h.controller.Transition(h.Memes[obj.MemeName]);
	        }
	        else
	        {
		        Debug.LogError($"Tried to update meme {obj.MemeName} on in-existent host {obj.Id}");
	        }
        }

        private void OnTransformUpdated(Transform obj) // TODO: merge spawn and update ?
        {
	        if (!_animals.ContainsKey(obj.Id))
	        {
		        // Debug.LogError($"Tried to update in-existent animal {obj.Id}"); // TODO: maybe should pass client id
		        return;
	        }

	        _animals[obj.Id].transform.position = obj.Position.ToVector3();
	        _animals[obj.Id].transform.rotation = obj.Rotation.ToQuaternion();
        }

        private void OnNavMeshUpdated(NavMeshUpdate obj)
        {
	        if (!_animals.ContainsKey(obj.Id))
	        {
		        // Debug.LogError($"Tried to update in-existent animal {obj.Id}"); // TODO: maybe should pass client id
		        return;
	        }

	        _animals[obj.Id].movement.MoveTo(obj.Destination.ToVector3());
        }

        #endregion
    }

}
