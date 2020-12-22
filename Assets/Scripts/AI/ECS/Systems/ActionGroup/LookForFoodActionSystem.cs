using AI.ECS.Components;
using AI.ECS.Utilities;
using Reese.Nav;
using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public class LookForFoodActionSystem : SystemBase
    {
        private BuildPhysicsWorld BuildPhysicsWorld 
            => World.GetExistingSystem<BuildPhysicsWorld>();
        private EntityCommandBufferSystem Barrier 
            => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var physicsWorld = BuildPhysicsWorld.PhysicsWorld;
            var collisionWorld = physicsWorld.CollisionWorld;
            var commandBuffer = Barrier.CreateCommandBuffer().AsParallelWriter();
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            Dependency = JobHandle.CombineDependencies(Dependency, BuildPhysicsWorld.GetOutputDependency());
            var colliderInput = Phext.NewColliderCastInput(~(uint) LayerMask.NameToLayer("Plant"), float3.zero, 10); // TODO
            var colliderOutput = new ColliderCastHit();

            Entities
                .WithNone<NavHasProblem, NavNeedsDestination, NavPlanning>()
                .WithReadOnly(renderBoundsFromEntity)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, 
                    in Parent surface, in LocalToWorld localToWorld, in LookForFoodAction lffAction, in Hungriness hungriness) =>
                {
                    if (surface.Value.Equals(Entity.Null)) return;

                    var random = randomArray[nativeThreadIndex];
                    collisionWorld.ColliderJob(colliderInput, ref colliderOutput);
                    if (colliderOutput.Entity != null)
                    {
                        // colliderOutput.
                    }
                    if (
                        physicsWorld.GetPointOnSurfaceLayer(
                            localToWorld,
                            NavUtil.GetRandomPointInBounds(
                                ref random,
                                renderBoundsFromEntity[surface.Value].Value,
                                100 // TODO
                            ),
                            out var validDestination
                        )
                    )
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, entity, new NavNeedsDestination
                        {
                            Destination = validDestination
                        });
                    }

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("LookForFoodJob")
                .ScheduleParallel();

            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
