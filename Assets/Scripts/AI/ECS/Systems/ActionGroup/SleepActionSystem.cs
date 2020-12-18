using AI.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public class SleepActionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            Entities.ForEach((ref Tiredness tired,
                ref Hungriness hunger,
                in SleepAction sleepAct) =>
            {
                // recover tiredness
                tired.Value = math.clamp(
                    tired.Value - sleepAct.TirednessRecoverPerSecond * deltaTime, 0f, 100f);

                // sleep still get hungry slowly
                hunger.Value = math.clamp(
                    hunger.Value + sleepAct.HungerCostPerSecond * deltaTime, 0f, 100f);
            }).ScheduleParallel();
        }
    }
}
