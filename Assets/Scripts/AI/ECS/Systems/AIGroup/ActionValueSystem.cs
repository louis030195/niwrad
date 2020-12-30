using AI.ECS.Components;
using AI.ECS.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
        // TODO: parametrize all these hard-coded thresholds
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity,
                int entityInQueryIndex,
                ref DynamicBuffer<ActionValue> actionValues,
                in DynamicBuffer<CharacteristicValue> characteristicValues,
                in LocalToWorld localToWorld,
                in Decision decision) =>
            {
                var satiation = characteristicValues[(int) CharacteristicType.Satiation].value;
                var energy = characteristicValues[(int) CharacteristicType.Energy].value;
                var youth = characteristicValues[(int) CharacteristicType.Youth].value;
                var distanceToTarget = HasComponent<Target>(entity)
                    ? math.distance(localToWorld.Position,
                        GetComponent<LocalToWorld>(GetComponent<Target>(entity).target).Position)
                    : float.MaxValue;
                // If it's close enough and not satiated, EAT !!!
                actionValues[(int) ActionType.Eat] = (decision.action == ActionType.Eat ||
                                                      decision.action == ActionType.Reach) &&
                                                     distanceToTarget <= 5f &&
                                                     satiation < 1.0f // Eat until full
                    ? 1
                    : 0;
                // TODO: make food more important than sex or reverse but no values should be at 1 in same time
                // i.e. more continuous values less discrete ones

                // If it's close enough and can mate, do it !!!
                actionValues[(int) ActionType.Mate] = (decision.action == ActionType.Mate ||
                                                       decision.action == ActionType.Reach) &&
                                                      distanceToTarget <= 5f &&
                                                      youth >= 0.2f && youth <= 0.8f && // Must be old enough but not too old
                                                      energy > 0.3f // Until has no more energy
                    ? 1
                    : 0;
                // Has a target ? Top 1 priority, reach it
                actionValues[(int) ActionType.Reach] =
                    distanceToTarget > 5f && distanceToTarget < float.MaxValue - 1 ? 1 : 0;
                // once it starts to sleep, it will not awake until it have enough energy
                actionValues[(int) ActionType.Sleep] = decision.action == ActionType.Sleep
                    ? energy < 1.0f ? 1 : 0
                    : Function.RaiseFastToSlow(math.clamp(1 - energy, 0f, 1f));
                // If low satiation, need to look for food, ignores if already eating or reaching target
                actionValues[(int) ActionType.LookForFood] = actionValues[(int) ActionType.Eat] > 0 ||
                                                             actionValues[(int) ActionType.Reach] > 0
                    ? 0
                    : Function.Exponential(math.clamp(1 - satiation, 0f, 1f));
                // If high energy, old enough and not too old, need to look for mate, ignores if already mating or reaching target
                actionValues[(int) ActionType.LookForMate] = actionValues[(int) ActionType.Mate] > 0 ||
                                                             actionValues[(int) ActionType.Reach] > 0 ||
                                                             youth < 0.2f && youth > 0.8f
                    ? 0
                    : Function.Exponential(math.clamp(energy, 0f, 1f));
                // Energetic, satiated and not interested in sex, can just wander
                // Sleep has more impact than food which has more impact than sex
                var concernBothersWandering = actionValues[(int) ActionType.Sleep] * 0.5f +
                                              actionValues[(int) ActionType.LookForFood] * 0.3f +
                                              actionValues[(int) ActionType.LookForMate] * 0.2f;
                // If energetic & satiated, let's wander around
                actionValues[(int) ActionType.Wander] = math.clamp(1f - concernBothersWandering, 0f, 1f);
            }).ScheduleParallel();

            CompleteDependency();
        }
    }
}
