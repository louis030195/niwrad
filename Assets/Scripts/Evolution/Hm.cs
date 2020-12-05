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
using Random = UnityEngine.Random;
using Transform = Api.Realtime.Transform;
using Vector3 = UnityEngine.Vector3;

namespace Evolution
{
    /// <summary>
    /// Defines the level of details of prefab used, e.g. low = cubes only, fast perf
    /// </summary>
    internal enum GraphicTier
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
        [SerializeField] private GameObject lowPolyHerbivorousAnimalPrefab; // E.g. blue cube
        [SerializeField] private GameObject lowPolyCarnivorousAnimalPrefab; // E.g. red cube
        [SerializeField] private GameObject lowPolyHerbivorousPlantPrefab; // E.g. green cube
        [SerializeField] private GameObject lowPolyCarnivorousPlantPrefab; // E.g. green cube
		/// <summary>
        /// Dictionary containing animals
        /// </summary>
        private readonly Dictionary<ulong, SimpleAnimal> _animals = new Dictionary<ulong, SimpleAnimal>();

		/// <summary>
		/// Dictionary containing Plants
		/// </summary>
		private readonly Dictionary<ulong, Plant> _plants = new Dictionary<ulong, Plant>();

        /// <summary>
        /// Next id to give for new host
        /// </summary>
        private ulong _nextId;

        public Statistics Statistics = new Statistics();

        #region MONO

        protected override void Awake()
        {
            base.Awake();
            switch (graphicTier)
            {
                case GraphicTier.Low:
                    Pool.Preload(lowPolyHerbivorousAnimalPrefab, 100);
                    Pool.Preload(lowPolyHerbivorousPlantPrefab, 100);
                    break;
                case GraphicTier.Medium:
                    Pool.Preload(lowPolyHerbivorousAnimalPrefab, 100);
                    TreePool.instance.FillSlowly(100);
                    break;
                case GraphicTier.High:
                    Pool.Preload(lowPolyHerbivorousAnimalPrefab, 100);
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

        private void Update()
        {
            if (Time.frameCount % 5 != 0) return;
            Statistics.Push(new TimeSeriePoint // TODO: should keep latest computed variables
            {
                Animals = _animals.Count,
                Plants = _plants.Count,
            });
        }

        protected override void OnDestroy()
        {
            if (Gm.instance == null || !Gm.instance.online) return;
	        Mcm.instance.Initialized -= Initialized;
	        Mcm.instance.AnimalSpawned -= OnAnimalSpawned;
	        Mcm.instance.AnimalDestroyed -= OnAnimalDestroyed;
	        Mcm.instance.PlantSpawned -= OnPlantSpawned;
	        Mcm.instance.PlantDestroyed -= OnPlantDestroyed;
	        Mcm.instance.AnimalSpawnRequested -= OnAnimalSpawnRequested;
	        Mcm.instance.AnimalDestroyRequested -= OnAnimalDestroyRequested;
	        Mcm.instance.PlantSpawnRequested -= OnPlantSpawnRequested;
	        Mcm.instance.PlantDestroyRequested -= OnPlantDestroyRequested;
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
            Mcm.instance.PlantSpawned += OnPlantSpawned;
            Mcm.instance.PlantDestroyed += OnPlantDestroyed;
            Mcm.instance.AnimalSpawnRequested += OnAnimalSpawnRequested;
            Mcm.instance.AnimalDestroyRequested += OnAnimalDestroyRequested;
            Mcm.instance.PlantSpawnRequested += OnPlantSpawnRequested;
            Mcm.instance.PlantDestroyRequested += OnPlantDestroyRequested;
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
        /// Ask executors to spawn a Plant
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        public void RequestSpawnPlant(Vector3 p, Quaternion r)
	        => Mcm.instance.RpcAsync(new Packet().Basic(p.Net()).ReqSpawnPlant(p, r));

        /// <summary>
        /// Spawn animal and sync if online
        /// </summary>
        /// <param name="p">Position</param>
        /// <param name="r">Rotation</param>
        /// <param name="c"></param>
        /// <param name="cMin"></param>
        /// <param name="cMax"></param>
        /// <returns></returns>
        public CommonAnimal SpawnAnimalSync(Vector3 p, Quaternion r, Characteristics c, Characteristics cMin, Characteristics cMax)
        {
            var animal = SpawnAnimal(p, r, c, cMin, cMax);
            if (!Gm.instance.online) return animal;
            var packet = new Packet().Basic(p.Net()).SpawnAnimal(_nextId+1, p, r);
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
        public Plant SpawnPlantSync(Vector3 p, Quaternion r, Characteristics c, Characteristics cMin, Characteristics cMax)
        {
            var veg = SpawnPlant(p, r, c, cMin, cMax);
            if (!Gm.instance.online) return veg;
	        var packet = new Packet().Basic(p.Net()).SpawnPlant(_nextId+1, p, r);
	        Mcm.instance.RpcAsync(packet);
            return veg;
        }

        public void DestroyPlantSync(ulong id)
        {
            var tree = DestroyPlant(id);
            if (!Gm.instance.online) return;
            var p = tree.transform.position;
            var packet = new Packet().Basic(p.Net()).DestroyPlant(id);
	        Mcm.instance.RpcAsync(packet);
        }

        /// <summary>
        /// De-spawn every hosts
        /// </summary>
        public void Reset()
        {
            _animals.Keys.ToList().ForEach(id => DestroyAnimal(id));
            _plants.Keys.ToList().ForEach(id => DestroyPlant(id));
        }

        public void Pause()
        {
            _animals.Values.ToList().ForEach(a => a.controller.aiActive = false);
            _plants.Values.ToList().ForEach(v => v.controller.aiActive = false);
        }
        
        public void Play()
        {
            _animals.Values.ToList().ForEach(a => a.controller.aiActive = true);
            _plants.Values.ToList().ForEach(v => v.controller.aiActive = true);
        }

        public void StartExperience(Experience e)
        {
            Reset();
            var areaRadius = Mathf.Pow(2, (float) e.Map.Size*0.9f);
            var middleOfMap = new Vector3(areaRadius, 0, areaRadius);
            var currentSeed = Random.state;
            for (ulong i = 0; i < e.AnimalDistribution.InitialAmount; i++)
            {
                Random.InitState(Random.Range(0, 10_000));
                var mask = LayerMask.GetMask("HerbivorousAnimal");
                // TODO: maybe should reset seed at some point
                var isCarnivorous = e.IncludeCarnivorous && Random.Range(0, 100) > 100 - e.CarnivorousPercent;
                // Debug.Log($"is carn {isCarnivorous} - {Random.value}");
                if (isCarnivorous) {
                    // If carnivorous ON = 1/2 chance of animal being carnivorous (TODO: better parameter for carni population)
                    mask = LayerMask.GetMask("CarnivorousAnimal");
                }
                // Random position within the map spaced according to a given scattering
                var pos = middleOfMap.RandomPositionAroundAboveGroundWithDistance(areaRadius,
                    mask,
                    e.AnimalDistribution.Scattering,
                    (float) e.Map.Height);
                if (Vector3.positiveInfinity.Equals(pos)) continue; // TODO: fix this
                e.AnimalCharacteristics.Carnivorous = isCarnivorous;
                var a = SpawnAnimal(pos, Quaternion.identity, 
                    e.AnimalCharacteristics,
                    e.AnimalCharacteristicsMinimumBound,
                    e.AnimalCharacteristicsMaximumBound);

            }
            
            for (ulong i = 0; i < e.PlantDistribution.InitialAmount; i++)
            {
                Random.InitState(Random.Range(0, 10_000));
                // Random position within the map spaced according to a given scattering
                var pos = middleOfMap.RandomPositionAroundAboveGroundWithDistance(areaRadius,
                    LayerMask.GetMask("Plant"),
                    e.PlantDistribution.Scattering,
                    (float) e.Map.Height);
                if (Vector3.positiveInfinity.Equals(pos)) continue; // TODO: fix this
                var isCarnivorous = e.IncludeCarnivorous && Random.Range(0, 100) > 100 - e.CarnivorousPercent;
                e.PlantCharacteristics.Carnivorous = isCarnivorous;
                var a = SpawnPlant(pos, Quaternion.identity, 
                    e.PlantCharacteristics,
                    e.PlantCharacteristicsMinimumBound,
                    e.PlantCharacteristicsMaximumBound);
            }

            // Reset to game seed
            Random.state = currentSeed;
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
	        obj.Id = _nextId+1;
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
            // Just a little hack to avoid crashing the PC ! :)
            if (_animals.Count > 500)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else    
                // Yeah let's crash other ppl PCs
                // Application.Quit();
#endif
            }
            // Debug.Log($"Spawning animal {obj}");
            _nextId++;
            var a = Pool.Spawn(c.Carnivorous ? lowPolyCarnivorousAnimalPrefab : lowPolyHerbivorousAnimalPrefab, p, r);
            _animals[_nextId] = a.GetComponent<SimpleAnimal>();
            _animals[_nextId].characteristics = c.Clone(); // TODO: prob quite expensive gotta pool later or something maybe
            _animals[_nextId].characteristicsMin = cMin;
            _animals[_nextId].characteristicsMax = cMax;
            _animals[_nextId].id = _nextId;
            _animals[_nextId].isCarnivorous = c.Carnivorous;
            // TODO: should prob have 2 prefab
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

        private void OnPlantSpawned(Transform obj) => 
            SpawnPlant(obj.Position.ToVector3(), obj.Rotation.ToQuaternion(), 
                Gm.instance.Experience.PlantCharacteristics,
                Gm.instance.Experience.PlantCharacteristicsMinimumBound,
                Gm.instance.Experience.PlantCharacteristicsMaximumBound);
        private void OnPlantDestroyed(Transform obj) => DestroyPlant(obj.Id);
        private void OnPlantDestroyRequested(Transform obj) => throw new NotImplementedException();
        private void OnPlantSpawnRequested(Transform obj)
        {
	        Debug.Log($"S1 requested tree spawn");
            // Someone requested a tree spawn, adjust to put it on top of ground
            obj.Position.Y = 1000; // TODO: see @Utils.Spatial.PositionAboveGround
            var aboveGround = obj.Position.ToVector3().PositionAboveGround();
            obj.Position = aboveGround.Net();
	        var packet = new Packet().Basic(obj.Position);
	        obj.Id = _nextId+1;
	        packet.Spawn = new Spawn
	        {
                Plant = new Api.Realtime.Plant
		        {
			        Transform = obj
		        }
	        };
	        Mcm.instance.RpcAsync(packet);
	        SpawnPlant(aboveGround, obj.Rotation.ToQuaternion(), 
                Gm.instance.Experience.PlantCharacteristics,
                Gm.instance.Experience.PlantCharacteristicsMinimumBound,
                Gm.instance.Experience.PlantCharacteristicsMaximumBound);
        }

        private Plant SpawnPlant(Vector3 p, Quaternion r, Characteristics c, Characteristics cMin, Characteristics cMax)
        {
            if (_plants.Count > 500)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else    
                // Yeah let's crash other ppl PCs
                // Application.Quit();
#endif
            }
            var veg = graphicTier == GraphicTier.Low ? // TODO: for now ugly if else, better = OOP stuff: spawn something i dont care what it is
                Pool.Spawn(c.Carnivorous ? lowPolyCarnivorousPlantPrefab : lowPolyHerbivorousPlantPrefab, p, r).GetComponent<Plant>() : 
                TreePool.instance.Spawn(p, r).go.GetComponent<Plant>();
            _nextId++;
            _plants[_nextId] = veg;
            _plants[_nextId].characteristics = c.Clone();
            _plants[_nextId].characteristicsMin = cMin;
            _plants[_nextId].characteristicsMax = cMax;
            _plants[_nextId].id = _nextId;
            // _plants[_nextId].isCarnivorous = c.Carnivorous; // TODO:

            if (!Gm.instance.online || Sm.instance.isServer) _plants[_nextId].EnableBehaviour(true);
	        return _plants[_nextId];
        }


        private Plant DestroyPlant(ulong id)
        {
	        if (!_plants.ContainsKey(id))
	        {
		        // Debug.LogError($"Tried to destroy in-existent tree {obj.Id}");
		        return null;
	        }

            var plant = _plants[id];
            if (graphicTier == GraphicTier.Low) Pool.Despawn(plant.gameObject);
            else TreePool.instance.Despawn(plant.gameObject);
	        _plants.Remove(id);
            return plant;
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
	        else if (_plants.ContainsKey(obj.Id))
	        {
		        var h = _plants[obj.Id];
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
