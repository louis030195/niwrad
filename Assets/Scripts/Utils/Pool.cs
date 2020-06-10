using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
    public static class Pool {

        // You can avoid resizing of the Stack's internal data by
        // setting this to a number equal to or greater to what you
        // expect most of your pool sizes to be.
        // Note, you can also use Preload() to set the initial size
        // of a pool -- this can be handy if only some of your pools
        // are going to be exceptionally large (for example, your bullets.)
        private const int DefaultPoolSize = 3;

        /// <summary>
        /// The Pool class represents the pool for a particular _prefab.
        /// </summary>
        private class PrefabPool {
            // We append an id to the name of anything we instantiate.
            // This is purely cosmetic.
            private int m_NextId = 1;

            // The structure containing our _inactive objects.
            // Why a Stack and not a List? Because we'll never need to
            // pluck an object from the start or middle of the array.
            // We'll always just grab the last one, which eliminates
            // any need to shuffle the objects around in memory.
            private readonly Stack<GameObject> m_Inactive;

            // The _prefab that we are pooling
            private readonly GameObject m_Prefab;

            public GameObject prefab => m_Prefab;

            // Constructor
            public PrefabPool(GameObject prefab, int initialQty) {
                m_Prefab = prefab;

                // If Stack uses a linked list internally, then this
                // whole initialQty thing is a placebo that we could
                // strip out for more minimal code. But it can't *hurt*.
                m_Inactive = new Stack<GameObject>(initialQty);
            }

            // Spawn an object from our pool
            public GameObject Spawn(Vector3 pos, Quaternion rot) {
                GameObject obj;
                if(m_Inactive.Count==0) {
                    // We don't have an object in our pool, so we
                    // instantiate a whole new object.
                    obj = Object.Instantiate(m_Prefab, pos, rot);
                    obj.name = m_Prefab.name + " ("+(m_NextId++)+")";

                    // Add a PoolMember component so we know what pool
                    // we belong to.
                    obj.AddComponent<PoolMember>().myPool = this;
                }
                else {
                    // Grab the last object in the _inactive array
                    obj = m_Inactive.Pop();

                    if(obj == null) {
                        // The _inactive object we expected to find no longer exists.
                        // The most likely causes are:
                        //   - Someone calling Despawn() on our object
                        //   - A scene change (which will destroy all our objects).
                        //     NOTE: This could be prevented with a DontDestroyOnLoad
                        //	   if you really don't want this.
                        // No worries -- we'll just try the next one in our sequence.

                        return Spawn(pos, rot);
                    }
                }

                obj.transform.position = pos;
                obj.transform.rotation = rot;
                obj.SetActive(true);
                return obj;

            }

            // Return an object to the _inactive pool.
            public void Despawn(GameObject obj) {
                obj.SetActive(false);

                // Since Stack doesn't have a Capacity member, we can't control
                // the growth factor if it does have to expand an internal array.
                // On the other hand, it might simply be using a linked list
                // internally.  But then, why does it allow us to specify a size
                // in the constructor? Maybe it's a placebo? Stack is weird.
                m_Inactive.Push(obj);
            }

        }


        /// <summary>
        /// Added to freshly instantiated objects, so we can link back
        /// to the correct pool on despawn.
        /// </summary>
        private class PoolMember : MonoBehaviour {
            public PrefabPool myPool;
        }

        // All of our pools
        private static Dictionary< GameObject, PrefabPool > _pools;

        /// <summary>
        /// Event invoked when this prefab is spawned
        /// </summary>
        public static Dictionary<GameObject, Action> Spawned;

        /// <summary>
        /// Event invoked when this prefab is despawned
        /// </summary>
        public static Dictionary<GameObject, Action> Despawned;

        /// <summary>
        /// Initialize our dictionary.
        /// </summary>
        private static void Init (GameObject prefab=null, int qty = DefaultPoolSize) {
            if(_pools == null) {
	            _pools = new Dictionary<GameObject, PrefabPool>();
	            Spawned = new Dictionary<GameObject, Action>();
	            Despawned = new Dictionary<GameObject, Action>();
            }
            if (prefab!=null && _pools.ContainsKey(prefab) == false) {
                _pools[prefab] = new PrefabPool(prefab, qty);
                Spawned[prefab] = delegate {  };
                Despawned[prefab] = delegate {  };
            }
        }

        /// <summary>
        /// If you want to preload a few copies of an object at the start
        /// of a scene, you can use this. Really not needed unless you're
        /// going to go from zero instances to 100+ very quickly.
        /// Could technically be optimized more, but in practice the
        /// Spawn/Despawn sequence is going to be pretty darn quick and
        /// this avoids code duplication.
        /// </summary>
        public static void Preload(GameObject prefab, int qty = 1) {
            Init(prefab, qty);

            // Make an array to grab the objects we're about to pre-spawn.
            var obs = new GameObject[qty];
            for (var i = 0; i < qty; i++) {
                obs[i] = Spawn (prefab, Vector3.zero, Quaternion.identity);
            }

            // Now despawn them all.
            for (var i = 0; i < qty; i++) {
                Despawn( obs[i] );
            }
        }

        /// <summary>
        /// Spawns a copy of the specified _prefab (instantiating one if required).
        /// NOTE: Remember that Awake() or Start() will only run on the very first
        /// spawn and that member variables won't get reset.  OnEnable will run
        /// after spawning -- but remember that toggling IsActive will also
        /// call that function.
        /// </summary>
        public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot) {
            Init(prefab);
	        Spawned[prefab]?.Invoke();
            return _pools[prefab].Spawn(pos, rot);
        }

        public static GameObject Spawn(GameObject prefab) {
            Init(prefab);
            Spawned[prefab]?.Invoke();
            return _pools[prefab].Spawn(Vector3.zero, Quaternion.identity);
        }


        /// <summary>
        /// Despawn the specified gameobject back into its pool.
        /// </summary>
        public static void Despawn(GameObject obj) {
            var pm = obj.GetComponent<PoolMember>();
            Despawned[pm.myPool.prefab]?.Invoke();

            if(pm == null) {
                Debug.Log ("Object '"+obj.name+"' wasn't spawned from a pool. Destroying it instead.");
                Object.Destroy(obj);
                return;
            }

            if (pm.myPool == null)
            {
	            Debug.Log ($"Object {obj.name} wasn't a prefab. Destroying it.");
	            Object.Destroy(obj);
	            return;
            }
	        pm.myPool.Despawn(obj);
        }

        /// <summary>
        /// Despawn the specified gameobject back into its pool after a delay
        /// </summary>
        public static void Despawn(GameObject obj, int after)
        {
	        Despawned[obj]?.Invoke();
	        Task.Delay(after).ContinueWith(t=> Despawn(obj));
        }

    }

}
