using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public static class Spatial
	{
		/// <summary>
		/// Non alloc field for raycasts
		/// </summary>
		private static readonly RaycastHit[] Hit = new RaycastHit[1];
		/// <summary>
		/// Non alloc field for overlap sphere casts
		/// </summary>
		private static Collider[] _results = new Collider[1];
		/// <summary>
		/// Return position above ground relatively from the prefab size
		/// Global position
		/// </summary>
		/// <param name="position"></param>
		/// <param name="prefabHeight">Prefab height needed in order to place well on top of ground</param>
		/// <param name="transform">Transform parent</param>
		/// <param name="layerMask">Layers to ignore</param>
		/// <returns></returns> // TODO: FIX
		public static Vector3 PositionAboveGround(this Vector3 position,
			float prefabHeight = 1f,
			Transform transform = null,
			LayerMask layerMask = default)
		{
			var p = position;
			if (transform != null) p += transform.position;


			// Current position is below ground
			if (Physics.RaycastNonAlloc(p, Vector3.up, Hit, Mathf.Infinity, ~layerMask) > 0)
			{
				// TODO: maybe should check if _hit[0].collider != null
				p.y += Hit[0].distance + prefabHeight * 0.5f;
				return p;
			}


			// Current position is above ground
			if (Physics.RaycastNonAlloc(p, Vector3.down, Hit, Mathf.Infinity, ~layerMask) > 0)
			{
				p.y -= Hit[0].distance - prefabHeight * 0.5f;
				return p;
			}

			// There is no ground above or below, outside map
			return Vector3.positiveInfinity;
		}

		/// <summary>
		/// This function will find a position to spawn above ground and far enough from other objects of the given layer
		/// Returns Vector3 zero in case it couldn't find a free position
		/// </summary>
		/// <returns></returns> // TODO: Big O
		public static Vector3 RandomPositionAroundAboveGroundWithDistance(this Vector3 center,
			float radius,
			LayerMask layerMask,
			float distance,
			float prefabHeight = 1f,
			Transform transform = null,
			int numberOfTries = 10)
		{
			if (distance > radius)
			{
				Debug.LogError("Distance must be inferior to radius");
				return Vector3.positiveInfinity;
			}
			var tries = 0;
			// While we didn't find a free position
			while (tries < numberOfTries)
			{
				// We pick a random position around above ground
				var newPos = center + Random.insideUnitSphere * radius;
				// center.y += 1000; // Security check, AboveGround check below first
				newPos = newPos.PositionAboveGround(prefabHeight, transform);
				if (newPos.Equals(Vector3.positiveInfinity)) // Outside map
				{
					tries++;
					continue;
				}
				// Then we check if this spot is free (from the given layer)
				var size = Physics.OverlapSphereNonAlloc(newPos, distance, _results, layerMask);

				// If no objects of the same layer is detected, this spot is free, return
				if (size == 0) return newPos;

				tries++;
			}

			return Vector3.positiveInfinity;
		}
	}
}
