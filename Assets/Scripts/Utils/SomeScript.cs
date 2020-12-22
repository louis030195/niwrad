using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Utils.Physics;
using Random = UnityEngine.Random;

namespace Utils
{
    public class SomeScript : MonoBehaviour
    {
        private void Start()
        {
            // Debug.Log($"{new Vector3(Random.Range(0, 100), 0, Random.Range(0, 100)).PositionAboveGround()}");
            // Debug.Log($"{Raycast()}");
        }

        private void Update()
        {
            if (Time.frameCount % 5 != 0) return;
            Debug.Log($"{Raycast()}");
        }

        private float3 Raycast()
        {
            BuildPhysicsWorld physicsWorldSystem =
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();

            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

            var pos = new float3(Random.Range(0, 100), 0, Random.Range(0, 100));
            // var pos = new float3(0, 0, 0);
            var rayInput = new RaycastInput()
            {
                Start = pos - new float3(0, 1, 0) * 1000,
                End = pos + new float3(0, 1, 0) * 1000,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0,
                }
            };

            var pointOnSurface = float3.zero;
            if (collisionWorld.CastRay(rayInput, out var hit))
            {
                pointOnSurface = hit.Position;
            }
            else
            {
                Debug.LogWarning($"Failed to find a point on surface layer with ray {rayInput.ToString()}");
            }

            return pointOnSurface;
        }
    }
}
