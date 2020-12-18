using AI.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public class PlayActionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            Entities.ForEach((ref Hungriness hunger,
                ref Tiredness tired,
                in PlayAction playAct) =>
            {
                hunger.Value = math.clamp(
                    hunger.Value + playAct.HungerCostPerSecond * deltaTime, 0f, 100f);
                tired.Value = math.clamp(
                    tired.Value + playAct.TirednessCostPerSecond * deltaTime, 0f, 100f);
            }).ScheduleParallel();
        }
    }
}
