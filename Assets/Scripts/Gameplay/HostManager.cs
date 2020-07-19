using System;
using System.Collections.Generic;
using UnityEngine;
using Evolution;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Api.Utils;
using Utils;
using ProceduralTree;
using Quaternion = UnityEngine.Quaternion;
using Transform = Api.Realtime.Transform;
using Vector3 = UnityEngine.Vector3;

namespace Gameplay
{
	/// <summary>
	/// The role of this manager is to request updates and update everything related to hosts.
	/// Single responsibility.
	/// </summary>
	public class HostManager : Singleton<HostManager> // TODO: prob need to move to Net.Gameplay
	{
		[SerializeField] private GameObject animalPrefab;

		/// <summary>
        /// Dictionary containing animals
        /// </summary>
        private readonly Dictionary<ulong, CommonAnimal> m_Animals = new Dictionary<ulong, CommonAnimal>();

		/// <summary>
		/// Dictionary containing trees
		/// </summary>
		private readonly Dictionary<ulong, Vegetation> m_Trees = new Dictionary<ulong, Vegetation>();

        /// <summary>
        /// Next id to give for new host
        /// </summary>
        private ulong m_NextId;

        #region MONO

        private void Start()
        {
	        MatchCommunicationManager.instance.Initialized += Initialized;
        }

        protected override void OnDestroy()
        {
	        MatchCommunicationManager.instance.Initialized -= Initialized;
	        MatchCommunicationManager.instance.AnimalSpawned -= OnAnimalSpawned;
	        MatchCommunicationManager.instance.AnimalDestroyed -= OnAnimalDestroyed;
	        MatchCommunicationManager.instance.TreeSpawned -= OnTreeSpawned;
	        MatchCommunicationManager.instance.TreeDestroyed -= OnTreeDestroyed;
	        MatchCommunicationManager.instance.AnimalSpawnRequested -= OnAnimalSpawnRequested;
	        MatchCommunicationManager.instance.AnimalDestroyRequested -= OnAnimalDestroyRequested;
	        MatchCommunicationManager.instance.TreeSpawnRequested -= OnTreeSpawnRequested;
	        MatchCommunicationManager.instance.TreeDestroyRequested -= OnTreeDestroyRequested;
	        MatchCommunicationManager.instance.MemeUpdated -= OnMemeUpdated;
	        MatchCommunicationManager.instance.TransformUpdated -= OnTransformUpdated;
	        MatchCommunicationManager.instance.NavMeshUpdated -= OnNavMeshUpdated;
        }

        #endregion

        #region PUBLIC METHODS

        public void RequestSpawnAnimal(Vector3 p, Quaternion r)
			=> MatchCommunicationManager.instance.RpcAsync(new Packet().ReqSpawnAnimal(p, r));

        public void RequestSpawnTree(Vector3 p, Quaternion r)
	        => MatchCommunicationManager.instance.RpcAsync(new Packet().ReqSpawnTree(p, r));

        public CommonAnimal SpawnAnimal(Vector3 p, Quaternion r)
        {
	        var packet = new Packet().SpawnAnimal(++m_NextId, p, r);
	        MatchCommunicationManager.instance.RpcAsync(packet);
	        return SpawnAnimal(packet.Spawn.Animal.Transform);
        }

        public void DestroyAnimal(ulong id)
        {
	        var packet = new Packet().DestroyAnimal(id);
	        MatchCommunicationManager.instance.RpcAsync(packet);
			DestroyAnimal(packet.Destroy.Animal.Transform);
        }

        public Vegetation SpawnTree(Vector3 p, Quaternion r)
        {
	        var packet = new Packet().SpawnTree(++m_NextId, p, r);
	        MatchCommunicationManager.instance.RpcAsync(packet);
	        return SpawnTree(packet.Spawn.Tree.Transform);
        }

        public void DestroyTree(ulong id)
        {
	        var packet = new Packet().DestroyTree(id);
	        MatchCommunicationManager.instance.RpcAsync(packet);
	        DestroyTree(packet.Destroy.Tree.Transform);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// A client's game play has been initialized
        /// </summary>
        /// <param name="senderId"></param>
        private void Initialized(string senderId)
        {
	        Pool.Preload(animalPrefab, 1000);

	        var syncingGlobalState = MatchCommunicationManager.instance.isHost &&
	                                 MatchCommunicationManager.instance.self.UserId != senderId;
	        var m = syncingGlobalState ? " sending global state" : "";
	        Debug.Log($"Player {senderId} game play is initialized{m}");
	        // Slight different use case: client joined, i'm server, send him the current state and return
	        if (syncingGlobalState)
	        {
				SyncGlobalState(senderId);
				return;
	        }
	        MatchCommunicationManager.instance.AnimalSpawned += OnAnimalSpawned;
	        MatchCommunicationManager.instance.AnimalDestroyed += OnAnimalDestroyed;
	        MatchCommunicationManager.instance.TreeSpawned += OnTreeSpawned;
	        MatchCommunicationManager.instance.TreeDestroyed += OnTreeDestroyed;
	        MatchCommunicationManager.instance.AnimalSpawnRequested += OnAnimalSpawnRequested;
	        MatchCommunicationManager.instance.AnimalDestroyRequested += OnAnimalDestroyRequested;
	        MatchCommunicationManager.instance.TreeSpawnRequested += OnTreeSpawnRequested;
	        MatchCommunicationManager.instance.TreeDestroyRequested += OnTreeDestroyRequested;
	        MatchCommunicationManager.instance.MemeUpdated += OnMemeUpdated;
	        MatchCommunicationManager.instance.TransformUpdated += OnTransformUpdated;
	        MatchCommunicationManager.instance.NavMeshUpdated += OnNavMeshUpdated;
        }

        private void SyncGlobalState(string senderId)
        {
	        foreach (var kv in m_Animals)
	        {
		        var t = kv.Value.transform;
		        var p = new Packet()
			        .SpawnAnimal(kv.Key, t.position, t.rotation);
		        p.Recipients.Add(senderId);
		        MatchCommunicationManager.instance.RpcAsync(p);
	        }
	        foreach (var kv in m_Trees)
	        {
		        var t = kv.Value.transform;
		        var p = new Packet()
			        .SpawnTree(kv.Key, t.position, t.rotation);
		        p.Recipients.Add(senderId);
		        MatchCommunicationManager.instance.RpcAsync(p);
	        }
        }
        private void OnAnimalSpawned(Transform obj) => SpawnAnimal(obj);
        private void OnAnimalDestroyed(Transform obj) => DestroyAnimal(obj);
        private void OnAnimalDestroyRequested(Transform obj) => throw new NotImplementedException();
        private void OnAnimalSpawnRequested(Transform obj)
        {
	        Debug.Log($"S1 requested animal spawn");
	        var packet = new Packet();
	        obj.Id = ++m_NextId;
	        packet.Spawn = new Spawn
	        {
		        Animal = new Animal
		        {
			        Transform = obj
		        }
	        };
	        MatchCommunicationManager.instance.RpcAsync(packet);
	        SpawnAnimal(obj);
        }

        private CommonAnimal SpawnAnimal(Transform obj)
        {
	        // Debug.Log($"Spawning animal {obj}");
	        m_NextId = obj.Id;
	        var a = Pool.Spawn(animalPrefab, obj.Position.ToVector3(), obj.Rotation.ToQuaternion());
	        m_Animals[obj.Id] = a.GetComponent<CommonAnimal>();
	        m_Animals[obj.Id].id = obj.Id;
	        // Only server handle animal behaviours
	        if (SessionManager.instance.isServer)
	        {
		        m_Animals[obj.Id].BringToLife();
		        // var tSync = m_Animals[obj.Id].gameObject.AddComponent<TransformSync>();
		        // tSync.id = obj.Id;
	        }
	        return m_Animals[obj.Id];
        }



        private void DestroyAnimal(Transform obj)
        {
	        if (!m_Animals.ContainsKey(obj.Id))
	        {
		        // TODO: fix
		        // Debug.LogError($"Tried to destroy in-existent animal {obj.Id}");
		        return;
	        }
	        Pool.Despawn(m_Animals[obj.Id].gameObject);
	        if (!m_Animals.Remove(obj.Id)) Debug.LogError($"Failed to remove animal {obj.Id}");
        }

        private void OnTreeSpawned(Transform obj) => SpawnTree(obj);
        private void OnTreeDestroyed(Transform obj) => DestroyTree(obj);
        private void OnTreeDestroyRequested(Transform obj) => throw new NotImplementedException();
        private void OnTreeSpawnRequested(Transform obj)
        {
	        Debug.Log($"S1 requested tree spawn");
	        var packet = new Packet();
	        obj.Id = ++m_NextId;
	        packet.Spawn = new Spawn
	        {
		        Tree = new Api.Realtime.Tree
		        {
			        Transform = obj
		        }
	        };
	        MatchCommunicationManager.instance.RpcAsync(packet);
	        SpawnTree(obj);
        }

        private Vegetation SpawnTree(Transform obj)
        {
	        m_NextId = obj.Id;
	        var (go, _) = TreePool.instance.Spawn(obj.Position.ToVector3(), obj.Rotation.ToQuaternion());
	        m_Trees[obj.Id] = go.GetComponent<Vegetation>();
	        m_Trees[obj.Id].id = obj.Id;

	        if (SessionManager.instance.isServer) m_Trees[obj.Id].BringToLife();
	        return m_Trees[obj.Id];
        }


        private void DestroyTree(Transform obj)
        {
	        if (!m_Trees.ContainsKey(obj.Id))
	        {
		        // Debug.LogError($"Tried to destroy in-existent tree {obj.Id}");
		        return;
	        }

	        TreePool.instance.Despawn(m_Trees[obj.Id].gameObject);
	        m_Trees.Remove(obj.Id);
        }



        private void OnMemeUpdated(Meme obj)
        {
	        // If this id doesn't belong to animals, maybe it's a tree
	        if (m_Animals.ContainsKey(obj.Id))
	        {
		        Host h = m_Animals[obj.Id];
		        if (!h.memes.ContainsKey(obj.MemeName))
		        {
			        Debug.LogError($"Tried to update in-existent meme {obj.MemeName} on host {obj.Id}");
			        return;
		        }

		        // Transition to the new received meme
		        h.controller.Transition(h.memes[obj.MemeName]);
	        }
	        else if (m_Trees.ContainsKey(obj.Id))
	        {
		        Host h = m_Trees[obj.Id];
		        if (!h.memes.ContainsKey(obj.MemeName))
		        {
			        Debug.LogError($"Tried to update in-existent meme {obj.MemeName} on host {obj.Id}");
			        return;
		        }

		        // Transition to the new received meme
		        h.controller.Transition(h.memes[obj.MemeName]);
	        }
	        else
	        {
		        Debug.LogError($"Tried to update meme {obj.MemeName} on in-existent host {obj.Id}");
	        }
        }

        private void OnTransformUpdated(Transform obj) // TODO: merge spawn and update ?
        {
	        if (!m_Animals.ContainsKey(obj.Id))
	        {
		        // Debug.LogError($"Tried to update in-existent animal {obj.Id}"); // TODO: maybe should pass client id
		        return;
	        }

	        m_Animals[obj.Id].transform.position = obj.Position.ToVector3();
	        m_Animals[obj.Id].transform.rotation = obj.Rotation.ToQuaternion();
        }

        private void OnNavMeshUpdated(NavMeshUpdate obj)
        {
	        if (!m_Animals.ContainsKey(obj.Id))
	        {
		        // Debug.LogError($"Tried to update in-existent animal {obj.Id}"); // TODO: maybe should pass client id
		        return;
	        }

	        m_Animals[obj.Id].movement.MoveTo(obj.Destination.ToVector3());
        }

        #endregion
    }

}
