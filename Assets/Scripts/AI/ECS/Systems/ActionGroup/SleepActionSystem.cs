﻿using AI.ECS.Components;
using Reese.Nav;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public class SleepActionSystem : SystemBase
    {
        private EntityCommandBufferSystem Barrier 
            => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var ecb = Barrier.CreateCommandBuffer().AsParallelWriter();
            // Reset navigation
            Entities
                .ForEach((Entity entity,
                    int entityInQueryIndex,
                    in SleepAction _1,
                    in NavNeedsDestination _2) =>
                {
                    ecb.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity);
                    ecb.RemoveComponent<NavHasProblem>(entityInQueryIndex, entity);
                    ecb.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);
                    ecb.RemoveComponent<NavLerping>(entityInQueryIndex, entity);
                }).ScheduleParallel();
            Barrier.AddJobHandleForProducer(Dependency);

            Entities.ForEach((Entity entity,
                int entityInQueryIndex,
                ref DynamicBuffer<CharacteristicValue> characteristicValues,
                in DynamicBuffer<CharacteristicChanges> characteristicChanges,
                in SleepAction _) =>
            {
                for (var i = 0; i < characteristicChanges[(int) ActionType.Sleep].value.Length; i++)
                {
                    characteristicValues[i] = 
                        math.clamp(characteristicValues[i] + 
                                   characteristicChanges[(int) ActionType.Sleep].value[i] * deltaTime, 0f, 1f);
                }
            }).ScheduleParallel();
        }
    }
}
