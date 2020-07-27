using UnityEngine;
using UnityEngine.AI;

namespace Utils
{
    [RequireComponent(typeof(Terrain))]
    public class NavMeshBaker : MonoBehaviour
    {
        [SerializeField] private NavMeshSurface[] surfaces;

        public void Bake()
        {
            foreach (var t in surfaces)
            {
                t.BuildNavMesh ();
            }
        }
    }
}
