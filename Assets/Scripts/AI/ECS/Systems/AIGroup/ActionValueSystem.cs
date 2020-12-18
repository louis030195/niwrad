using AI.ECS.Components;
using AI.ECS.Utilities;
using Unity.Entities;
using Unity.Mathematics;

namespace AI.ECS.Systems.AIGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(AISystemGroup))]
    public class ActionValueSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            //>> Calculate scores
            Entities.ForEach((ref EatScorer eatScore,
                in Hungriness hunger,
                in Decision decision) =>
            {
                if (decision.Action == ActionType.Eat)
                {
                    // once it starts to eat, it will not stop until it's full
                    eatScore.Score = hunger.Value <= float.Epsilon ? 0f : 1f;
                }
                else
                {
                    var input = math.clamp(hunger.Value * 0.01f, 0f, 1f);
                    eatScore.Score = Function.Exponential(input, 2f);
                }
            }).ScheduleParallel();

            Entities.ForEach((ref SleepScorer sleepScore,
                in Tiredness tired,
                in Decision decision) =>
            {
                if (decision.Action == ActionType.Sleep)
                {
                    // once it starts to sleep, it will not awake until it have enough rest
                    sleepScore.Score = tired.Value <= float.Epsilon ? 0f : 1f;
                }
                else
                {
                    var input = math.clamp(tired.Value * 0.01f, 0f, 1f);
                    sleepScore.Score = Function.RaiseFastToSlow(input, 4);
                }
            }).ScheduleParallel();

            Entities.ForEach((ref PlayScorer playScore,
                in Tiredness tired,
                in Hungriness hunger) =>
            {
                // The play scorer has two considerations
                // The cat will play when it feels neigher hungry nor tired
                // Let's say it hate tired more(love to sleep), so the sleep consideration get more weight
                // sleep weight: 0.6, eat weight: 0.4

                var eatConcern = Function.Exponential(math.clamp(hunger.Value * 0.01f, 0f, 1f));
                var sleepConcern = Function.RaiseFastToSlow(math.clamp(tired.Value * 0.01f, 0f, 1f));
            
                var concernBothersPlaying = sleepConcern * 0.6f + eatConcern * 0.4f;

                playScore.Score = math.clamp(1f - concernBothersPlaying, 0f, 1f);
            }).ScheduleParallel();
            //<<

            this.CompleteDependency();
        }
    }
}
