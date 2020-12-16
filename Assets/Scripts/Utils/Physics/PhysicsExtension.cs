using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Physics
{
	public static class PhysicsExtension
	{
		private static readonly Collider[] Results = new Collider[1000];

        /// <summary>
        /// Returns the closest GameObject around to this position given mask.
        /// Doesn't guarantee closest when there is more than 1000 colliders around with the given mask
        /// </summary>
        /// <param name="go"></param>
        /// <param name="radius"></param>
        /// <param name="mask"></param>
        /// <param name="skipInactive"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static GameObject Closest(this GameObject go, float radius, LayerMask mask, bool skipInactive = true, 
            Func<GameObject, bool> filter = null)
		{
			var center = go.transform.position;
            // WARNING: NonAlloc means objects stay in the DS after, have to post-filter then usually
			if (UnityEngine.Physics.OverlapSphereNonAlloc(center, radius, Results, 1 << mask) == 0) return default;
			var min = default(GameObject);
			Results.ToList().ForEach(c =>
            {
				// Skip inactive ? Skip self, Skip non-target layers leftover in the data structure
				if (c == null || mask != c.gameObject.layer || skipInactive && !c.gameObject.activeInHierarchy || c.gameObject.Equals(go)) return;

                // var f = filter != null && filter.Invoke(c.gameObject); // TODO
				if (min == default ||
					Vector3.Distance(c.transform.position, center) <
				    Vector3.Distance(min.transform.position, center))
				{
					// Debug.Log($"Found animal closer: {c.transform.position} {c.gameObject.activeInHierarchy}");

					min = c.gameObject;
				}
			});
			return min;
		}
		// TODO: similar for other raycasts


		/// <summary>
		/// Find closest object in the list
		/// </summary>
		/// <param name="objects"></param>
		/// <param name="to"></param>
		/// <param name="mask">optional mask to filter-out unwanted</param>
		/// <returns></returns>
		public static GameObject Closest(this IEnumerable<(float f, GameObject go)> objects, Vector3 to, LayerMask mask = default)
		{
			var closest = default(GameObject);
			foreach (var (_, go) in objects
				.Where(tp => closest == default ||
				               tp.go.layer == mask &&
				               Vector3.Distance(to, tp.go.transform.position) <
				               Vector3.Distance(to, closest.transform.position)))
			{
				closest = go;
			}

			return closest;
		}
        
        public static bool AlmostEquals(this Vector3 a, Vector3 b, float epsilon=1e-005f)
        {
            return Vector3.SqrMagnitude(a - b) < epsilon;
        }
    }
}
