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
    public class ReachActionSystem : SystemBase
    {

        private EntityCommandBufferSystem Barrier
            => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        

        protected override void OnUpdate()
        {
            var ecb = Barrier.CreateCommandBuffer().AsParallelWriter();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);

            var deltaTime = Time.DeltaTime;

            Entities.ForEach((Entity entity,
                int entityInQueryIndex,
                ref DynamicBuffer<CharacteristicValue> characteristicValues,
                in DynamicBuffer<CharacteristicChanges> characteristicChanges,
                in ReachAction _) =>
            {
                for (var i = 0; i < characteristicChanges[(int) ActionType.Reach].value.Length; i++)
                {
                    characteristicValues[i] = 
                        math.clamp(characteristicValues[i] + 
                                   characteristicChanges[(int) ActionType.Reach].value[i] * deltaTime, 0f, 1f);
                }
            }).ScheduleParallel();
            
            Entities
                .WithNone<NavHasProblem, NavNeedsDestination, NavPlanning>()
                .WithReadOnly(jumpableBufferFromEntity)
                .ForEach((Entity entity,
                    int entityInQueryIndex,
                    int nativeThreadIndex,
                    ref NavAgent agent,
                    in Target target,
                    in Parent surface,
                    in LocalToWorld localToWorld,
                    in ReachAction act
                ) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.HasComponent(surface.Value)
                    ) return;

                    var jumpableSurfaces = jumpableBufferFromEntity[surface.Value];
                    var targetPosition = GetComponent<LocalToWorld>(target.target).Position;
                    if (math.distance(localToWorld.Position, targetPosition) > 5f)
                    {
                        // Debug.Log($"Distance to target {math.distance(localToWorld.Position, targetPosition)}, target pos {targetPosition}");
                        // Reset path finding
                        ecb.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavHasProblem>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavLerping>(entityInQueryIndex, entity);
                        ecb.AddComponent(entityInQueryIndex, entity, new NavNeedsDestination
                        {
                            Destination = targetPosition //+ new float3(0, -0.5f, 0) // TODO: tmp hack
                        });
                    }
                    else // Once reached set to null ? Maybe should do that once finished with the target ?
                    {
                        // Debug.Log($"Reached my target at {targetPosition}");
                        ecb.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavHasProblem>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavLerping>(entityInQueryIndex, entity);
                        // TODO: at some point remove target ?
                    }
                })
                .WithName("ReachActionJob")
                .ScheduleParallel();

            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
