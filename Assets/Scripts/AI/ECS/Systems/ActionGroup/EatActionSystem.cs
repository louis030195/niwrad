using AI.ECS.Components;
using Reese.Nav;
using Unity.Entities;
using Unity.Mathematics;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public class EatActionSystem : SystemBase
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
                    in EatAction _1,
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
                in EatAction _) =>
            {
                for (var i = 0; i < characteristicChanges[(int) ActionType.Eat].value.Length; i++)
                {
                    characteristicValues[i] =
                        math.clamp(characteristicValues[i] +
                                   characteristicChanges[(int) ActionType.Eat].value[i] * deltaTime, 0f, 1f);
                }

                // Ate enough, remove target
                if (characteristicChanges[(int) ActionType.Eat].value[(int) CharacteristicType.Satiation] > 0.9)
                {
                    ecb.RemoveComponent<Target>(entityInQueryIndex, entity);
                }
            }).ScheduleParallel();


            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
