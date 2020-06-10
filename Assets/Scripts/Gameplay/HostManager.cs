
using System;
using System.Collections.Generic;
using UnityEngine;
using Evolution;
using Net.Match;
using Net.Realtime;
using Net.Session;
using Net.Utils;
using Utils;
using ProceduralTree;
using Quaternion = UnityEngine.Quaternion;
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
        private readonly Dictionary<ulong, Animal> m_Animals = new Dictionary<ulong, Animal>();

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
	        MatchCommunicationManager.instance.TreeSpawned -= OnTreeSpawned;
	        MatchCommunicationManager.instance.MemeUpdated -= OnMemeUpdated;
	        MatchCommunicationManager.instance.TransformUpdated -= OnTransformUpdated;
        }

        #endregion

        #region PUBLIC METHODS

        public Animal SpawnAnimal(Vector3 p, Quaternion r)
        {
	        var packet = new Packet()
		        .Basic()
		        .SpawnAnimal(++m_NextId, p, r);
	        // Notify others
	        MatchCommunicationManager.instance.Rpc(packet);
	        return SpawnAnimal(packet.Spawn.Animal.ObjectTransform);
        }

        /// <summary>
        /// Returns animal with given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Animal GetAnimal(ulong id)
        {
	        m_Animals.TryGetValue(id, out var animal);
	        return animal;
        }

        /// <summary>
        /// Removes given animal from dictionary
        /// </summary>
        /// <param name="animal"></param>
        public bool RemoveAnimal(Animal animal)
        {
            return m_Animals.Remove(animal.id);
        }


        public Vegetation SpawnTree(Vector3 p, Quaternion r)
        {
	        var packet = new Packet()
		        .Basic()
		        .SpawnTree(++m_NextId, p, r);
	        // Notify others
	        MatchCommunicationManager.instance.Rpc(packet);
	        return SpawnTree(packet.Spawn.Tree.ObjectTransform);
        }

        /// <summary>
        /// Returns tree with given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Vegetation GetTree(ulong id)
        {
	        m_Trees.TryGetValue(id, out var tree);
	        return tree;
        }

        /// <summary>
        /// Removes given tree from dictionary
        /// </summary>
        /// <param name="tree"></param>
        public bool RemoveTree(Vegetation tree)
        {
	        return m_Trees.Remove(tree.id);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// A client's game play has been initialized
        /// </summary>
        /// <param name="senderId"></param>
        private void Initialized(string senderId)
        {
	        var syncingGlobalState = SessionManager.instance.isServer &&
	                                 SessionManager.instance.session.UserId != senderId;
	        var m = syncingGlobalState ? " sending global state" : "";
	        Debug.Log($"Player {senderId} game play is initialized{m}");
	        // Slight different use case: client joined, i'm server, send him the current state and return
	        if (syncingGlobalState)
	        {
		        // Retrieve the presence
		        var p = MatchCommunicationManager.instance.players
			        .Find(u => u.UserId == senderId);
		        foreach (var kv in m_Animals)
		        {
			        var t = kv.Value.transform;
			        MatchCommunicationManager.instance.Rpc(new Packet()
				        .Basic()
				        .SpawnAnimal(kv.Key, t.position, t.rotation),
				        recipients: new []{p});
		        }
		        foreach (var kv in m_Trees)
		        {
			        var t = kv.Value.transform;
			        MatchCommunicationManager.instance.Rpc(new Packet()
					        .Basic()
					        .SpawnTree(kv.Key, t.position, t.rotation),
				        recipients: new []{p});
		        }
	        }
	        MatchCommunicationManager.instance.AnimalSpawned += OnAnimalSpawned;
	        MatchCommunicationManager.instance.TreeSpawned += OnTreeSpawned;
	        MatchCommunicationManager.instance.MemeUpdated += OnMemeUpdated;
	        MatchCommunicationManager.instance.TransformUpdated += OnTransformUpdated;
        }

        private Animal SpawnAnimal(Net.Realtime.Transform obj)
        {
	        var a = Pool.Spawn(animalPrefab, obj.Position.ToVector3(), obj.Rotation.ToQuaternion());
	        m_Animals[obj.Id] = a.GetComponent<Animal>();
	        m_Animals[obj.Id].BringToLife();
	        return m_Animals[obj.Id];
        }
        private void OnAnimalSpawned(Net.Realtime.Transform obj)
        {
	        SpawnAnimal(obj);
        }
        private Vegetation SpawnTree(Net.Realtime.Transform obj)
        {
	        var (go, _) = TreePool.instance.Spawn(obj.Position.ToVector3(), obj.Rotation.ToQuaternion());
	        m_Trees[obj.Id] = go.GetComponent<Vegetation>();
	        m_Trees[obj.Id].BringToLife();
	        return m_Trees[obj.Id];
        }
        private void OnTreeSpawned(Net.Realtime.Transform obj)
        {
	        SpawnTree(obj);
        }

        private void OnMemeUpdated(Packet.Types.UpdateMeme obj)
        {
	        Host h = GetAnimal(obj.Id);

	        // If this id doesn't belong to animals, maybe it's a tree
	        if (h == null) h = GetTree(obj.Id);
	        if (h != null)
	        {
		        h.memes.TryGetValue(obj.MemeName, out var m);
		        if (m == null)
		        {
			        Debug.LogError($"Tried to update in-existent meme {obj.MemeName} on host {obj.Id}");
			        return;
		        }

		        // Transition to the new received meme
		        h.controller.Transition(m);
	        }
	        else
	        {
		        Debug.LogError($"Tried to update meme {obj.MemeName} on in-existent host {obj.Id}");
	        }
        }

        private void OnTransformUpdated(Net.Realtime.Transform obj)
        {
	        var animal = GetAnimal(obj.Id);
	        if (animal == null)
	        {
		        Debug.LogError($"Tried to update in-existent animal"); // TODO: maybe should pass client id
		        return;
	        }

	        animal.transform.position = obj.Position.ToVector3();
	        animal.transform.rotation = obj.Rotation.ToQuaternion();
        }

        #endregion
    }

}
