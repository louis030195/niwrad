using AI.ECS.Components;
using AI.ECS.Utilities;
using Unity.Entities;
using Unity.Mathematics;

namespace AI.ECS.Systems.AIGroup
{
    /// <summary>
    /// ActionValueSystem is a System that compute values of every actions given hard-coded heuristics
    /// unlike in reinforcement learning where this mapping is learned
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class ActionValueSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Calculate action values
            Entities.ForEach((ref DynamicBuffer<ActionValue> actionValues,
                in Hungriness hunger,
                in Tiredness tired,
                in Decision decision) =>
            {
                // TODO: can probably extract from the if "execution graph"-like
                if (decision.Action == ActionType.Eat)
                {
                    // once it starts to eat, it will not stop until it's full
                    actionValues[(int) ActionType.Eat] = hunger.Value <= float.Epsilon ? 0f : 1f;
                }
                else
                {
                    var input = math.clamp(hunger.Value * 0.01f, 0f, 1f);
                    actionValues[(int) ActionType.Eat] = Function.Exponential(input);
                }
                if (decision.Action == ActionType.Sleep)
                {
                    // once it starts to sleep, it will not awake until it have enough rest
                    actionValues[(int) ActionType.Sleep] = tired.Value <= float.Epsilon ? 0f : 1f;
                }
                else
                {
                    var input = math.clamp(tired.Value * 0.01f, 0f, 1f);
                    actionValues[(int) ActionType.Sleep] = Function.RaiseFastToSlow(input);
                }
                // The play scorer has two considerations
                // The host will play when it feels neither hungry nor tired
                // Let's say it loves to sleep, so the sleep consideration get more weight
                // sleep weight: 0.6, eat weight: 0.4

                var eatConcern = Function.Exponential(math.clamp(hunger.Value * 0.01f, 0f, 1f));
                var sleepConcern = Function.RaiseFastToSlow(math.clamp(tired.Value * 0.01f, 0f, 1f));
            
                var concernBothersPlaying = sleepConcern * 0.6f + eatConcern * 0.4f;

                actionValues[(int) ActionType.Wander] = math.clamp(1f - concernBothersPlaying, 0f, 1f);
            }).ScheduleParallel();

            CompleteDependency();
        }
    }
}
