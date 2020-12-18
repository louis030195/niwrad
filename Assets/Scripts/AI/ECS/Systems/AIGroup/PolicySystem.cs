using AI.ECS.Components;
using Unity.Entities;

namespace AI.ECS.Systems.AIGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(AISystemGroup))]
    [UpdateAfter(typeof(ActionValueSystem))]
    public class PolicySystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            // Choose action base on the highest score
            var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
            Entities.ForEach((Entity entity,
                int entityInQueryIndex,
                ref Decision decision,
                in EatScorer eatScore,
                in SleepScorer sleepScore,
                in PlayScorer playScore) =>
            {
                float highestScore = 0.0f;
                ActionType actionToDo = ActionType.Play;
                if (eatScore.Score > highestScore)
                {
                    highestScore = eatScore.Score;
                    actionToDo = ActionType.Eat;
                }
                if (sleepScore.Score > highestScore)
                {
                    highestScore = sleepScore.Score;
                    actionToDo = ActionType.Sleep;
                }
                if (playScore.Score > highestScore)
                {
                    highestScore = playScore.Score;
                    actionToDo = ActionType.Play;
                }

                if (decision.Action != actionToDo)
                {
                    decision.Action = actionToDo;

                    switch (actionToDo)
                    {
                        case ActionType.Eat:
                            ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                            ecb.RemoveComponent<PlayAction>(entityInQueryIndex, entity);
                            ecb.AddComponent<EatAction>(entityInQueryIndex, entity);
                            ecb.SetComponent(entityInQueryIndex, entity, new EatAction()
                            {
                                HungerRecoverPerSecond = 5.0f,
                                TirednessCostPerSecond = 2.0f
                            });
                            break;
                        case ActionType.Sleep:
                            ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                            ecb.RemoveComponent<PlayAction>(entityInQueryIndex, entity);
                            ecb.AddComponent<SleepAction>(entityInQueryIndex, entity);
                            ecb.SetComponent(entityInQueryIndex, entity, new SleepAction()
                            {
                                TirednessRecoverPerSecond = 3.0f,
                                HungerCostPerSecond = 0.5f
                            });
                            break;
                        case ActionType.Play:
                            ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                            ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                            ecb.AddComponent<PlayAction>(entityInQueryIndex, entity);
                            ecb.SetComponent(entityInQueryIndex, entity, new PlayAction()
                            {
                                HungerCostPerSecond = 2.0f,
                                TirednessCostPerSecond = 4.0f
                            });
                            break;
                    }
                }
            }).ScheduleParallel();

            _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}
