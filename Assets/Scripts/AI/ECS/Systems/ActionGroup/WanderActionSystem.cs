using AI.ECS.Components;
using Reese.Nav;
using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public class WanderActionSystem : SystemBase
    {
        private BuildPhysicsWorld BuildPhysicsWorld 
            => World.GetExistingSystem<BuildPhysicsWorld>();
        private NavGroundingSystem NavGrounding
            => World.GetExistingSystem<NavGroundingSystem>();

        private EntityCommandBufferSystem Barrier 
            => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var physicsWorld = BuildPhysicsWorld.PhysicsWorld;
            var commandBuffer = Barrier.CreateCommandBuffer().AsParallelWriter();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            Dependency = JobHandle.CombineDependencies(Dependency, 
                BuildPhysicsWorld.GetOutputDependency(),
                NavGrounding.GetOutputDependency());

            Entities.ForEach((Entity entity,
                int entityInQueryIndex,
                ref DynamicBuffer<CharacteristicValue> characteristicValues,
                in DynamicBuffer<CharacteristicChanges> characteristicChanges,
                in WanderAction _) =>
            {
                for (var i = 0; i < characteristicChanges[(int) ActionType.Wander].value.Length; i++)
                {
                    characteristicValues[i] = 
                        math.clamp(characteristicValues[i] + 
                                   characteristicChanges[(int) ActionType.Wander].value[i] * deltaTime, 0f, 1f);
                }
            }).ScheduleParallel();
            Entities
                .WithNone<NavHasProblem, NavNeedsDestination, NavPlanning>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, 
                    int entityInQueryIndex, 
                    int nativeThreadIndex, 
                    ref NavAgent agent, 
                    in Parent surface, 
                    in LocalToWorld localToWorld,
                    in WanderAction wanderAction) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.HasComponent(surface.Value)
                    ) return;

                    var jumpableSurfaces = jumpableBufferFromEntity[surface.Value];
                    var random = randomArray[nativeThreadIndex];
                    if (
                        physicsWorld.GetPointOnSurfaceLayer(
                            localToWorld,
                            NavUtil.GetRandomPointInBounds(
                                ref random,
                                renderBoundsFromEntity[surface.Value].Value, // TODO: smaller AABB
                                random.NextFloat(0.1f, 1f)
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
                .WithName("WanderActionJob")
                .ScheduleParallel();

            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
