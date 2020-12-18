using System;
using AI.ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AI.ECS.Utilities
{
    public class Spawner : MonoBehaviour
    {
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity currentPrefab;
        private void Start()
        {
            currentPrefab = entityManager
                .CreateEntityQuery(typeof(HostPrefab)).GetSingleton<HostPrefab>().Value;
            var entity = entityManager.Instantiate(currentPrefab);
            entityManager.AddComponent(entity, ComponentType.ReadOnly<Host>());
            entityManager.AddComponentData(entity, new Hungriness { Value = 0 });
            entityManager.AddComponentData(entity, new Tiredness { Value = 0 });
            entityManager.AddComponentData(entity, new Decision { Action = ActionType.Null });

            entityManager.AddComponent(entity, ComponentType.ReadWrite<EatScorer>()); ;
            entityManager.AddComponent(entity, ComponentType.ReadWrite<SleepScorer>());
            entityManager.AddComponent(entity, ComponentType.ReadWrite<PlayScorer>());
            
            entityManager.AddComponentData(entity, new Translation
            {
                Value = new float3(0, 0, 1.337f)
            });
            entityManager.AddComponent<LocalToWorld>(entity);
            entityManager.AddComponent<Parent>(entity);
            entityManager.AddComponent<LocalToParent>(entity);
        }
    }
}
