using AI.ECS.Components;
using Reese.Nav;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI.ECS.Utilities
{
    public class Spawner : MonoBehaviour
    {
        // TODO: temporary ugly as hell file for tests then will be transformed into system maybe
        public int maxEntities = 50;

        private static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        private Entity _currentPrefab;
        private int _entitiesCount;
        

        private void Start()
        {
            _currentPrefab = EntityManager
                .CreateEntityQuery(typeof(HostPrefab)).GetSingleton<HostPrefab>().Value;
            
        }

        private bool _hack;
        private void Update()
        {
            if (Time.frameCount % 100 != 0 || _hack) return;
            _hack = true;
            var physicsWorldSystem =
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
            
            var rayInputs = new RaycastInput[maxEntities];
            var rayOutputs = new Unity.Physics.RaycastHit[maxEntities];
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0,
            };
            for (var i = 0; i < maxEntities; i++)
            {
                var pos = new float3(Random.Range(0, 500), 0, Random.Range(0, 500));
                rayInputs[i] = new RaycastInput
                {
                    Start = pos - new float3(0, 1, 0) * 1000,
                    End = pos + new float3(0, 1, 0) * 1000,
                    Filter = filter,
                };
            }

            collisionWorld.RayJob(rayInputs, ref rayOutputs);
            for (var i = 0; i < rayOutputs.Length; i++)
            {
                SpawnHost(rayOutputs[i].Position);
            }
        }

        private void SpawnHost(float3 p)
        {
            var e  = EntityManager.Instantiate(_currentPrefab);
            EntityManager.AddComponentData(e, new NavAgent
            {
                TranslationSpeed = 20,
                RotationSpeed = 0.3f,
                TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                Offset = new float3(0, 1, 0)
            });
            
            EntityManager.AddComponentData(e, new Translation
            {
                Value = p
            });            
            EntityManager.AddComponent<LocalToWorld>(e);
            EntityManager.AddComponent<Parent>(e);
            EntityManager.AddComponent<LocalToParent>(e);
            EntityManager.AddComponent<NavNeedsSurface>(e);
            EntityManager.AddComponent<NavTerrainCapable>(e);
            
            EntityManager.AddComponent(e, ComponentType.ReadOnly<Host>());
            var buffer = EntityManager.AddBuffer<ActionValue>(e);
            for (var j = 0.0f; j < 4; j++)
            {
                buffer.Add(j);
            } // TODO: static data for nb actions (4)
            
            EntityManager.AddComponentData(e, new Hungriness {Value = 0});
            EntityManager.AddComponentData(e, new Tiredness {Value = 0});
            EntityManager.AddComponentData(e, new Decision {Action = ActionType.Null});
            
            EntityManager.AddComponent(e, ComponentType.ReadWrite<ActionValue>());
        }

        // private void Update()
        // {
        //     if (Time.frameCount < 100 || _entitiesCount > maxEntities) return;
        //     var e  = EntityManager.Instantiate(_currentPrefab);
        //     EntityManager.AddComponentData(e, new NavAgent
        //     {
        //         TranslationSpeed = 20,
        //         RotationSpeed = 0.3f,
        //         TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
        //         Offset = new float3(0, 1, 0)
        //     });
        //
        //     var p = Raycast();
        //     p.y += 0.5f;
        //     EntityManager.AddComponentData(e, new Translation
        //     {
        //         Value = p
        //     });            
        //     EntityManager.AddComponent<LocalToWorld>(e);
        //     EntityManager.AddComponent<Parent>(e);
        //     EntityManager.AddComponent<LocalToParent>(e);
        //     EntityManager.AddComponent<NavNeedsSurface>(e);
        //     EntityManager.AddComponent<NavTerrainCapable>(e);
        //
        //
        //     EntityManager.AddComponent(e, ComponentType.ReadOnly<Host>());
        //     var buffer = EntityManager.AddBuffer<ActionValue>(e);
        //     for (var j = 0.0f; j < 4; j++)
        //     {
        //         buffer.Add(j);
        //     } // TODO: static data for nb actions (4)
        //
        //     EntityManager.AddComponentData(e, new Hungriness {Value = 0});
        //     EntityManager.AddComponentData(e, new Tiredness {Value = 0});
        //     EntityManager.AddComponentData(e, new Decision {Action = ActionType.Null});
        //
        //     EntityManager.AddComponent(e, ComponentType.ReadWrite<ActionValue>());
        //     _entitiesCount++;
        // }

        private float3 Raycast()
        {
            BuildPhysicsWorld physicsWorldSystem =
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();

            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

            var pos = new float3(Random.Range(0, 500), 0, Random.Range(0, 500));
            var rayInput = new RaycastInput
            {
                Start = pos - new float3(0, 1, 0) * 1000,
                End = pos + new float3(0, 1, 0) * 1000,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0,
                }
            };

            var pointOnSurface = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            if (collisionWorld.CastRay(rayInput, out var hit))
            {
                pointOnSurface = hit.Position;
            }
            else
            {
                Debug.LogWarning($"Failed to find a point on surface layer with ray {rayInput.ToString()}");
            }

            return pointOnSurface;
        }
    }
}
