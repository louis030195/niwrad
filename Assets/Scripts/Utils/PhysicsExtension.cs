using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Utils
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
		/// <returns></returns>
		public static GameObject Closest(this GameObject go, float radius, LayerMask mask, bool skipInactive = true)
		{
			var center = go.transform.position;
			if (Physics.OverlapSphereNonAlloc(center, radius, Results, mask) == 0) return default;
			var min = default(GameObject);
			Results.ToList().ForEach(c =>
			{
				// Skip inactive ? Skip self
				if (c == null || skipInactive && !c.gameObject.activeInHierarchy || c.gameObject.Equals(go)) return;
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
	}
}
