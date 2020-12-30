using System;
using AI.ECS.Components;
using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace AI.ECS.Systems.AIGroup
{
    /// <summary>
    /// PolicySystem should decide which action to take according to numerical values.
    /// NOTHING ELSE.
    /// </summary>
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
                ref NavAgent agent,
                in LocalToWorld localToWorld,
                in DynamicBuffer<ActionValue> actionValues) =>
            {
                float? highestScore = null;
                var actionToDo = ActionType.Wander;
                // Greedy policy, pick highest value
                // TODO: e-greedy, curiosity ? etc.
                var index = -1;
                for (var i = 0; i < actionValues.Length; i++)
                {
                    var thisNum = actionValues[i].value;
                    if (!highestScore.HasValue || thisNum > highestScore.Value)
                    {
                        highestScore = thisNum;
                        index = i;
                    }
                }

                actionToDo = (ActionType) index;
                if (decision.action == actionToDo) return;
                decision.action = actionToDo;
                switch (actionToDo)
                { // TODO: fix this verbose garbage somehow
                    case ActionType.Null:
                        break;
                    case ActionType.Eat:
                        ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<WanderAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForFoodAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<ReachAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForMateAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<MateAction>(entityInQueryIndex, entity);
                        ecb.AddComponent<EatAction>(entityInQueryIndex, entity);
                        break;
                    case ActionType.Sleep:
                        ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<WanderAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForFoodAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<ReachAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForMateAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<MateAction>(entityInQueryIndex, entity);
                        ecb.AddComponent<SleepAction>(entityInQueryIndex, entity);
                        break;
                    case ActionType.Wander:
                        ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForFoodAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<ReachAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForMateAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<MateAction>(entityInQueryIndex, entity);
                        ecb.AddComponent<WanderAction>(entityInQueryIndex, entity);
                        break;
                    case ActionType.LookForFood:
                        ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<WanderAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<ReachAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForMateAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<MateAction>(entityInQueryIndex, entity);
                        ecb.AddComponent<LookForFoodAction>(entityInQueryIndex, entity);
                        break;
                    case ActionType.Reach:
                        ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<WanderAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForFoodAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForMateAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<MateAction>(entityInQueryIndex, entity);
                        ecb.AddComponent<ReachAction>(entityInQueryIndex, entity);
                        break;
                    case ActionType.LookForMate:
                        ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<WanderAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<ReachAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForFoodAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<MateAction>(entityInQueryIndex, entity);
                        ecb.AddComponent<LookForMateAction>(entityInQueryIndex, entity);
                        break;
                    case ActionType.Mate:
                        ecb.RemoveComponent<SleepAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<WanderAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForFoodAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<ReachAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<EatAction>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<LookForMateAction>(entityInQueryIndex, entity);
                        ecb.AddComponent<MateAction>(entityInQueryIndex, entity);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }).ScheduleParallel();

            _endSimulationEcbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
