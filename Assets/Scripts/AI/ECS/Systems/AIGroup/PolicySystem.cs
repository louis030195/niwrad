using System.Linq;
using AI.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
                in DynamicBuffer<ActionValue> actionValues) =>
            {
                float? highestScore = null;
                var actionToDo = ActionType.Wander;
                // Greedy policy, pick highest value
                // TODO: e-greedy, curiosity ? etc.
                var index = -1;
                for (var i = 0; i < actionValues.Length; i++)
                {
                    var thisNum = actionValues[i].Value;
                    if (!highestScore.HasValue || thisNum > highestScore.Value)
                    {
                        highestScore = thisNum;
                        index = i;
                    }
                }

                actionToDo = (ActionType) index;
                if (decision.Action != actionToDo)
                {
                    decision.Action = actionToDo;

                    switch (actionToDo)
                    {
                        case ActionType.Eat:
                            ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                            ecb.RemoveComponent<WanderAction>(entityInQueryIndex, entity);
                            ecb.AddComponent<EatAction>(entityInQueryIndex, entity);
                            ecb.SetComponent(entityInQueryIndex, entity, new EatAction
                            {
                                HungerRecoverPerSecond = 5.0f,
                                TirednessCostPerSecond = 2.0f
                            });
                            break;
                        case ActionType.Sleep:
                            ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                            ecb.RemoveComponent<WanderAction>(entityInQueryIndex, entity);
                            ecb.AddComponent<SleepAction>(entityInQueryIndex, entity);
                            ecb.SetComponent(entityInQueryIndex, entity, new SleepAction
                            {
                                TirednessRecoverPerSecond = 3.0f,
                                HungerCostPerSecond = 0.5f
                            });
                            break;
                        case ActionType.Wander:
                            ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                            ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                            ecb.AddComponent<WanderAction>(entityInQueryIndex, entity);
                            ecb.SetComponent(entityInQueryIndex, entity, new WanderAction
                            {
                                HungerCostPerSecond = 2.0f,
                                TirednessCostPerSecond = 4.0f
                            });
                            break;
                    }
                }
            }).ScheduleParallel();

            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
