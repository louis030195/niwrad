using UnityEngine;

namespace Utils
{
	/// <summary MyClassName="{}">
	/// Inherit from this base class to create a singleton.
	/// e.g. public class MyClassName : Singleton
	/// </summary>
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// Lock used to not allow simultaneous operations on this singleton by multiple sources.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Reference to the singleton instance of type <see cref="T"/>.
        /// </summary>
        private static T _instance;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the reference to the singleton instance of type <see cref="T"/>.
        /// </summary>
        public static T instance
        {
            get
            {
                // Lock preventing from simultaneous access by multiple sources.
                lock (_lock)
                {
                    // If it's the first time accessing this singleton Instance, _instance will always be null
                    // Searching for an active instance of type T in the scene.
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                    }

                    return _instance;
                }
            }
        }

        #endregion

        #region Monobehaviour

        /// <summary>
        /// Checking if an instance of <see cref="Singleton{T}"/> already exists in the scene.
        /// If it exists, destroy this object.
        /// </summary>
        protected virtual void Awake()
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Removes the reference to this object on destroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                _instance = null;
            }
        }

        #endregion
    }
}
