﻿using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Utils.Physics
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
        /// <returns></returns>
        public static Vector3 PositionAboveGround(this Vector3 position, float prefabHeight = 1f)
		{
			var p = position;
            // LayerMask mask = LayerMask.NameToLayer("Ground") & LayerMask.NameToLayer("Water");
            LayerMask mask = 1 << LayerMask.NameToLayer("Ground") | (1 << LayerMask.NameToLayer("Water"));
            // TODO: do we even care about "below ground" ?
			// Current position is below ground
			// if (Physics.RaycastNonAlloc(p, Vector3.up, Hit, Mathf.Infinity, ~layerMask) > 0)
			// {
			// 	// TODO: maybe should check if _hit[0].collider != null
			// 	p.y += Hit[0].distance + prefabHeight * 0.5f;
   //              return p;
			// }
			
            Debug.DrawRay(p, Vector3.down*int.MaxValue, Color.magenta);
			// Current position is above ground
            if (UnityEngine.Physics.RaycastNonAlloc(p, Vector3.down, Hit, Mathf.Infinity, mask) <= 0)
                return Vector3.positiveInfinity;
            if (Hit[0].transform.gameObject.layer.Equals(LayerMask.NameToLayer("Water"))) return Vector3.positiveInfinity;
            p.y -= Hit[0].distance - prefabHeight * 0.5f;
            return p;

            // There is no ground above or below, outside map
        }

		/// <summary>
		/// This function will find a position to spawn above ground and far enough from other objects of the given layer
		/// Returns Vector3 zero in case it couldn't find a free position
		/// </summary>
		/// <returns></returns>
		public static Vector3 Spray(this Vector3 center,
			float areaRadius,
			LayerMask layerMask,
			float radiusBetweenObjects,
            float areaHeight = 1000f,
			float prefabHeight = 1f,
			int numberOfTries = 10)
		{
			if (radiusBetweenObjects > areaRadius)
			{
				Debug.LogError("Distance must be inferior to radius");
				return Vector3.positiveInfinity;
			}
			var tries = 0;
			// While we didn't find a free position
			while (tries < numberOfTries)
			{
				// We pick a random position around above ground
				var newPos = center + Random.insideUnitSphere * areaRadius;
                newPos.y = areaHeight;
				newPos = newPos.PositionAboveGround(prefabHeight);
				if (newPos.Equals(Vector3.positiveInfinity)) // Outside map
				{
					tries++;
					continue;
				}
				// Then we check if this spot is free (from the given layer)
				var size = UnityEngine.Physics.OverlapSphereNonAlloc(newPos, radiusBetweenObjects, _results, layerMask);
				// If no objects of the same layer is detected, this spot is free, return
				if (size == 0) return newPos;

				tries++;
			}

			return Vector3.positiveInfinity;
		}
    }
}
