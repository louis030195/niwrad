// using System;
// using System.Collections.Concurrent;
// using AI.ECS.Components;
// using AI.ECS.Systems.AIGroup;
// using Reese.Nav;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Physics;
// using Unity.Physics.Systems;
// using Unity.Transforms;
// using UnityEngine;
// using Animal = AI.ECS.Components.Animal;
// using Plant = Evolution.Plant;
//
// namespace AI.ECS.Systems.ActionGroup
// {
//     internal struct BatchSpawn : IJob
//     {
//         public NativeArray<float3> animals;
//         public NativeArray<float3> plants;
//         public void Execute()
//         {
//             var rayInputs = new RaycastInput[animals.Length + plants.Length];
//             var rayOutputs = new Unity.Physics.RaycastHit[animals.Length + plants.Length];
//             // Compute positions for both animals and plants
//             var filter = new CollisionFilter
//             {
//                 BelongsTo = ~0u,
//                 CollidesWith = ~0u,
//                 GroupIndex = 0,
//             };
//             for (var i = 0; i < animals.Length + plants.Length; i++)
//             {
//                 var pos = new float3(500, 0, 500) + new float3(Random.Range(-500, 500), 0, Random.Range(-500, 500));
//                 rayInputs[i] = new RaycastInput
//                 {
//                     Start = pos - new float3(0, 1, 0) * 1000,
//                     End = pos + new float3(0, 1, 0) * 1000,
//                     Filter = filter,
//                 };
//             }
//         
//             collisionWorld.RayJob(rayInputs, ref rayOutputs);
//             var terrain = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Terrain>())
//                 .GetSingletonEntity();
//             // Then first half for animals
//             for (var i = 0; i < maxAnimals; i++)
//             {
//                 // SpawnAnimal(rayOutputs[i].Position);
//                 _spawner.EnqueueAnimal(rayOutputs[i].Position);
//             }
//         
//             // Second half for plants
//             for (var i = maxAnimals; i < rayOutputs.Length; i++)
//             {
//                 // SpawnPlant(rayOutputs[i].Position, terrain);
//                 _spawner.EnqueuePlant(rayOutputs[i].Position);
//             }
//         }
//     }
//
//     // [UpdateAfter(typeof(AISystemGroup))]
//     [DisableAutoCreation]
//     public class SpawnerSystem : SystemBase
//     {
//         private readonly ConcurrentQueue<float3> _queueAnimals = new ConcurrentQueue<float3>();
//         private readonly ConcurrentQueue<float3> _queuePlants = new ConcurrentQueue<float3>();
//         private BuildPhysicsWorld BuildPhysicsWorld 
//             => World.GetExistingSystem<BuildPhysicsWorld>();
//         private EntityCommandBufferSystem Barrier 
//             => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
//
//         private Entity _animalPrefab;
//         private Entity _plantPrefab;
//         private Entity _terrain;
//         // private EntityArchetype _animalArchetype;
//         // private EntityArchetype _plantArchetype;
//         protected override void OnCreate()
//         {
//             _animalPrefab = EntityManager
//                 .CreateEntityQuery(typeof(AnimalPrefab)).GetSingleton<AnimalPrefab>().value;
//             _plantPrefab = EntityManager
//                 .CreateEntityQuery(typeof(PlantPrefab)).GetSingleton<PlantPrefab>().value;
//             _terrain = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Terrain>())
//                 .GetSingletonEntity();
//             // TODO: archetype https://stackoverflow.com/questions/57482803/how-do-i-create-entities-from-an-archetype-that-are-renderable
//             //  should be more efficient
//             // _animalArchetype = EntityManager.CreateArchetype(
//             //     typeof(LocalToWorld),
//             //     typeof(Parent),
//             //     typeof(LocalToParent),
//             //     typeof(NavNeedsSurface),
//             //     typeof(NavTerrainCapable),
//             //     typeof(Translation),
//             //     typeof(Rotation),
//             //     ComponentType.ReadOnly<Animal>(),
//             //     typeof(Decision),
//             //     typeof(RenderMesh),
//             //     typeof(RenderBounds),
//             //     typeof(LocalToWorld)
//             // );
//         }
//
//         protected override void OnUpdate()
//         {
//             while (_queueAnimals.TryDequeue(out var p))
//             {
//                 SpawnAnimal(p);
//             }
//             while (_queuePlants.TryDequeue(out var p))
//             {
//                 SpawnPlant(p, _terrain);
//             }
//         }
//         
//         private void SpawnAnimal(float3 p)
//         {
//             if (p.Equals(default)) return;
//             var e = EntityManager.Instantiate(_animalPrefab);
//             EntityManager.AddComponentData(e, new NavAgent
//             {
//                 TranslationSpeed = 100,
//                 RotationSpeed = 0.3f,
//                 TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
//                 Offset = new float3(0, 0.5f, 0)
//             });
//
//             EntityManager.AddComponentData(e, new Translation
//             {
//                 Value = p
//             });
//             EntityManager.AddComponent<LocalToWorld>(e);
//             EntityManager.AddComponent<Parent>(e);
//             EntityManager.AddComponent<LocalToParent>(e);
//             EntityManager.AddComponent<NavNeedsSurface>(e);
//             EntityManager.AddComponent<NavTerrainCapable>(e);
//
//             EntityManager.AddComponent(e, ComponentType.ReadOnly<Animal>());
//             EntityManager.AddComponentData(e, new Decision {action = ActionType.Null});
//             
//             // All the arrays ! //
//             
//             var actionValues = EntityManager.AddBuffer<ActionValue>(e);
//             for (var j = 0; j < Enum.GetNames(typeof(ActionType)).Length; j++)
//             {
//                 // All action values to 0 (@ActionValueSystem takes charge of that according to current entity state)
//                 actionValues.Add(0);
//             }
//             EntityManager.AddComponent(e, ComponentType.ReadWrite<ActionValue>());
//
//             // All characteristics to max at start (energy, satiation ...)
//             var characteristicValues = EntityManager.AddBuffer<CharacteristicValue>(e);
//             for (var j = 0; j < Enum.GetNames(typeof(CharacteristicType)).Length; j++)
//             {
//                 characteristicValues.Add(1);
//             }
//             EntityManager.AddComponent(e, ComponentType.ReadWrite<CharacteristicValue>());
//
//             // For each actions, the impact it has on every characteristics (hard coded)
//             var characteristicChanges = EntityManager.AddBuffer<CharacteristicChanges>(e);
//             // Action null only harms us ... (yet)                                              // satiation, hydration, age, energy
//             characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.01f, -.01f }});
//             // Action eat restores satiation, lower hydration, youth and energy
//             characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { +.05f, -.01f, -.01f, -.01f }});
//             // Action sleep restores energy, lower hydration, youth, energy
//             characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.001f, +.05f }});
//             // Action wander only harms us ... (yet)
//             characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.1f, -.01f, -.01f, -.01f }});
//             // Action look for food only harms us ... (yet)
//             characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.01f, -.01f }});
//             // Action reach only harms us ... (yet)
//             characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.01f, -.01f }});
//             EntityManager.AddComponent(e, ComponentType.ReadWrite<CharacteristicValue>());
//         }
//
//         private void SpawnPlant(float3 p, Entity parent)
//         {
//             var e = EntityManager.Instantiate(_plantPrefab);
//
//             EntityManager.AddComponentData(e, new Translation
//             {
//                 Value = p + new float3(0, 0.5f, 0)
//             });
//             EntityManager.AddComponent<LocalToWorld>(e);
//             EntityManager.AddComponent<Parent>(e);
//             EntityManager.AddComponent<LocalToParent>(e);
//             EntityManager.SetComponentData(e, new Parent {Value = parent});
//             EntityManager.AddComponent(e, ComponentType.ReadOnly<Plant>());
//             // Plants are brain-dead blocs yet, later should introduce (asexual usually) reproduction etc.
//         }
//
//         public void EnqueueAnimal(float3 p) => _queueAnimals.Enqueue(p);
//         public void EnqueuePlant(float3 p) => _queuePlants.Enqueue(p);
//     }
// }
