// using AI.ECS.Components;
// using Api.Realtime;
// using Reese.Nav;
// using Reese.Random;
// using Unity.Collections;
// using Unity.Collections.LowLevel.Unsafe;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Physics.Systems;
// using Unity.Rendering;
// using Unity.Transforms;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// namespace AI.ECS.Systems.ActionGroup
// {
//     // [DisableAutoCreation]
//     public class SpawnerSystem : SystemBase
//     {
//         private BuildPhysicsWorld BuildPhysicsWorld 
//             => World.GetExistingSystem<BuildPhysicsWorld>();
//         private EntityCommandBufferSystem Barrier 
//             => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
//
//         protected override void OnCreate()
//         {
//             var physicsWorld = BuildPhysicsWorld.PhysicsWorld;
//             Dependency = JobHandle.CombineDependencies(Dependency, BuildPhysicsWorld.GetOutputDependency());
//             
//             var outputEntities = new NativeArray<Entity>(100, Allocator.Temp);
//             var currentPrefab = Barrier.EntityManager
//                 .CreateEntityQuery(typeof(HostPrefab)).GetSingleton<HostPrefab>().Value;
//             Barrier.EntityManager.Instantiate(currentPrefab, outputEntities);
//             
//             for (var i = 0; i < outputEntities.Length; ++i)
//             {
//                 Barrier.EntityManager.AddComponentData(outputEntities[i], new NavAgent
//                 {
//                     TranslationSpeed = 20,
//                     RotationSpeed = 0.3f,
//                     TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
//                     Offset = new float3(0, 1, 0)
//                 });
//
//                 physicsWorld.GetPointOnSurfaceLayer(
//                     new LocalToWorld(),
//                     new float3(Random.Range(0, 100), 0, Random.Range(0, 100)), 
//                     out var pos
//                 );
//             
//                 Barrier.EntityManager.AddComponentData(outputEntities[i], new Translation
//                 {
//                     Value = pos
//                 });
//             
//                 Barrier.EntityManager.AddComponent<LocalToWorld>(outputEntities[i]);
//                 Barrier.EntityManager.AddComponent<Parent>(outputEntities[i]);
//                 Barrier.EntityManager.AddComponent<LocalToParent>(outputEntities[i]);
//                 Barrier.EntityManager.AddComponent<NavNeedsSurface>(outputEntities[i]);
//                 Barrier.EntityManager.AddComponent<NavTerrainCapable>(outputEntities[i]);
//                 
//                 Barrier.EntityManager.AddComponent(outputEntities[i], ComponentType.ReadOnly<Host>());
//                 var buffer = Barrier.EntityManager.AddBuffer<ActionValue>(outputEntities[i]);
//                 for (var j = 0.0f; j < 4; j++) { buffer.Add(j); } // TODO: static data for nb actions (4)
//                 Barrier.EntityManager.AddComponentData(outputEntities[i], new Hungriness { Value = 0 });
//                 Barrier.EntityManager.AddComponentData(outputEntities[i], new Tiredness { Value = 0 });
//                 Barrier.EntityManager.AddComponentData(outputEntities[i], new Decision { Action = ActionType.Null });
//             
//                 Barrier.EntityManager.AddComponent(outputEntities[i], ComponentType.ReadWrite<ActionValue>());
//             }
//             
//             outputEntities.Dispose();
//             Barrier.AddJobHandleForProducer(Dependency);
//         }
//
//         protected override void OnUpdate()
//         {
//             throw new System.NotImplementedException();
//         }
//     }
// }
