using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public static class Spatial
	{
		/// <summary>
		/// Return position above ground relatively from the prefab size
		/// Global position
		/// </summary>
		/// <param name="position"></param>
		/// <param name="prefabHeight">Prefab height needed in order to place well on top of ground</param>
		/// <param name="transform">Transform parent</param>
		/// <param name="layerMask">Layers to ignore</param>
		/// <returns></returns>
		public static Vector3 AboveGround(this Vector3 position,
			float prefabHeight = 1f,
			Transform transform = null,
			LayerMask layerMask = default)
		{
			var p = position;
			if (transform != null) p += transform.position;

			var below = false;

			// Below ground
			if (Physics.Raycast(p, Vector3.up, out var hit, Mathf.Infinity, ~layerMask))
			{
				p.y += hit.distance + prefabHeight * 0.5f;
				below = true;
			}

			if (!below) // No need to raycast again
			{
				// Above ground
				if (Physics.Raycast(p, Vector3.down, out hit, Mathf.Infinity, ~layerMask))
				{
					p.y -= hit.distance - prefabHeight * 0.5f;
				}
			}

			return p;
		}
	}
}
