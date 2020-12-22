using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Collider = Unity.Physics.Collider;
using RaycastHit = Unity.Physics.RaycastHit;

namespace AI.ECS.Utilities
{
    [BurstCompile]
    public struct RayCastJob : IJobParallelFor
    {
        [ReadOnly] public CollisionWorld world;
        [ReadOnly] public NativeArray<RaycastInput> inputs;
        public NativeArray<RaycastHit> outputs;

        public void Execute(int index)
        {
            world.CastRay(inputs[index], out var hit);
            outputs[index] = hit;
        }
    }

    [BurstCompile]
    public struct ColliderCastJob : IJobParallelFor
    {
        [ReadOnly] public CollisionWorld world;
        [ReadOnly] public NativeArray<ColliderCastInput> inputs;
        public NativeArray<ColliderCastHit> outputs;

        public void Execute(int index)
        {
            world.CastCollider(inputs[index], out var hit);
            outputs[index] = hit;
        }
    }

    public static class Phext
    {
        /// <summary>
        /// Batch cast ray
        /// </summary>
        /// <param name="world"></param>
        /// <param name="inputs"></param>
        /// <param name="outputs"></param>
        public static void RayJob(this CollisionWorld world, RaycastInput[] inputs, ref RaycastHit[] outputs)
        {
            var rayCommands = new NativeArray<RaycastInput>(inputs.Length, Allocator.TempJob);
            var rayResults = new NativeArray<RaycastHit>(outputs.Length, Allocator.TempJob);
            for (var i = 0; i < inputs.Length; i++)
            {
                rayCommands[i] = inputs[i];
            }

            var handle = world.ScheduleBatchRayCast(rayCommands, rayResults);
            handle.Complete();
            for (var i = outputs.Length - 1; i >= 0; i--)
            {
                outputs[i] = rayResults[i];
            }

            rayCommands.Dispose();
            rayResults.Dispose();
        }

        /// <summary>
        /// Single ray
        /// </summary>
        /// <param name="world"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void RayJob(this CollisionWorld world, RaycastInput input, ref RaycastHit output)
        {
            var rayCommands = new NativeArray<RaycastInput>(1, Allocator.TempJob);
            var rayResults = new NativeArray<RaycastHit>(1, Allocator.TempJob);
            rayCommands[0] = input;
            var handle = world.ScheduleBatchRayCast(rayCommands, rayResults);
            handle.Complete();
            output = rayResults[0];
            rayCommands.Dispose();
            rayResults.Dispose();
        }

        public static unsafe ColliderCastInput NewColliderCastInput(uint mask, float3 center, float radius)
        {
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = mask,
                GroupIndex = 0,
            };
            var sphereCollider = Unity.Physics.SphereCollider.Create(new SphereGeometry
            {
                Center = center,
                Radius = radius
            }, filter);
            return new ColliderCastInput
            {
                Start = float3.zero,
                Orientation = quaternion.identity,
                End = new float3(10, 10, 10), // TODO:
                Collider = (Collider*) sphereCollider.GetUnsafePtr()
            };
        }
        public static void ColliderJob(this CollisionWorld world, ColliderCastInput input, ref ColliderCastHit output)
        {
            var rayCommands = new NativeArray<ColliderCastInput>(1, Allocator.TempJob);
            var rayResults = new NativeArray<ColliderCastHit>(1, Allocator.TempJob);
            rayCommands[0] = input;
            var handle = world.ScheduleBatchColliderCast(rayCommands, rayResults);
            handle.Complete();
            output = rayResults[0];

            rayCommands.Dispose();
            rayResults.Dispose();
        }
        public static void ColliderJob(this CollisionWorld world, ColliderCastInput[] inputs,
            ref ColliderCastHit[] outputs)
        {
            var rayCommands = new NativeArray<ColliderCastInput>(inputs.Length, Allocator.TempJob);
            var rayResults = new NativeArray<ColliderCastHit>(outputs.Length, Allocator.TempJob);
            for (var i = 0; i < inputs.Length; i++)
            {
                rayCommands[i] = inputs[i];
            }
            var handle = world.ScheduleBatchColliderCast(rayCommands, rayResults);
            handle.Complete();
            for (var i = outputs.Length - 1; i >= 0; i--)
            {
                outputs[i] = rayResults[i];
            }

            rayCommands.Dispose();
            rayResults.Dispose();
        }

        private static JobHandle ScheduleBatchRayCast(this CollisionWorld world,
            NativeArray<RaycastInput> inputs, NativeArray<RaycastHit> results)
        {
            return new RayCastJob
            {
                inputs = inputs,
                outputs = results,
                world = world
            }.Schedule(inputs.Length, inputs.Length);
        }

        private static JobHandle ScheduleBatchColliderCast(this CollisionWorld world,
            NativeArray<ColliderCastInput> inputs, NativeArray<ColliderCastHit> results)
        {
            return new ColliderCastJob
            {
                inputs = inputs,
                outputs = results,
                world = world
            }.Schedule(inputs.Length, inputs.Length);
        }

        // public unsafe static Entity SphereCast(this CollisionWorld c, float3 RayFrom, float3 RayTo, float radius)
        // {
        //     var filter = new CollisionFilter
        //     {
        //         BelongsTo = ~0u, // all 1s, so all layers, collide with everything 
        //         CollidesWith = ~0u,
        //         GroupIndex = 0
        //     };
        //
        //     var sphereCollider = 
        //         SphereCollider.Create(new SphereGeometry{Center = RayFrom, Radius = radius}, filter);
        //
        //     var input = new ColliderCastInput
        //     {
        //         Start  = RayFrom,
        //         Orientation = quaternion.identity,
        //         End = RayTo - RayFrom,
        //         Collider = (Collider*)sphereCollider.GetUnsafePtr()
        //     };
        //
        //     ColliderCastHit hit = new ColliderCastHit();
        //     bool haveHit = collisionWorld.CastCollider(input, out hit);
        //     if (haveHit)
        //     {
        //         // see hit.Position 
        //         // see hit.SurfaceNormal
        //         Entity e = physicsWorldSystem.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
        //         return e;
        //     }
        //     return Entity.Null;
        // }
    }
}
