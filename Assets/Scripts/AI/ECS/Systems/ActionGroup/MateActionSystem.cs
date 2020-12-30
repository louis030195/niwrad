using AI.ECS.Components;
using Reese.Nav;
using Unity.Entities;
using Unity.Mathematics;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public class MateActionSystem : SystemBase
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
                    in MateAction _1,
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
                in MateAction _) =>
            {
                for (var i = 0; i < characteristicChanges[(int) ActionType.Mate].value.Length; i++)
                {
                    characteristicValues[i] =
                        math.clamp(characteristicValues[i] +
                                   characteristicChanges[(int) ActionType.Mate].value[i] * deltaTime, 0f, 1f);
                }

                // Mated enough, remove target
                if (characteristicChanges[(int) ActionType.Mate].value[(int) CharacteristicType.Energy] < 0.3)
                {
                    ecb.RemoveComponent<Target>(entityInQueryIndex, entity);
                    // TODO: spawn child
                }
            }).ScheduleParallel();


            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
