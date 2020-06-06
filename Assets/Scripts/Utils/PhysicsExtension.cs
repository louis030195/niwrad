using System.Linq;
using UnityEngine;

namespace Utils
{
	public static class PhysicsExtension
	{
		/// <summary>
		/// Returns the closest GameObject around to this position given mask
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="mask"></param>
		/// <param name="skipInactive"></param>
		/// <returns></returns>
		public static GameObject Closest(this Vector3 center, float radius, LayerMask mask, bool skipInactive = true)
		{
			var hit = Physics.OverlapSphere(center, radius, mask);
			var last = hit.LastOrDefault();
			if (last == null) return null;
			var min = last.gameObject;
			foreach (var c in hit)
			{
				if (skipInactive && !c.gameObject.activeInHierarchy) continue;
				if (Vector3.Distance(c.transform.position, center) <
				    Vector3.Distance(min.transform.position, center))
				{
					min = c.gameObject;
				}
			}

			return min;
		}
		// TODO: similar for other raycasts
	}
}
