using System;
using AI.ECS.Components;
using AI.ECS.Systems.ActionGroup;
using Api.Realtime;
using Reese.Nav;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Animal = AI.ECS.Components.Animal;
using Plant = AI.ECS.Components.Plant;
using Random = UnityEngine.Random;

namespace AI.ECS.Utilities
{
    public class Spawner : MonoBehaviour
    {
        // TODO: temporary ugly as hell file for tests then will be transformed into system maybe
        public int maxAnimals = 50;
        public int maxPlants = 50;
        public int mapSize = 1000;
        private static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        private Entity _animalPrefab;
        private Entity _plantPrefab;
        // private SpawnerSystem _spawner;

        private void Start()
        {
            _animalPrefab = EntityManager
                .CreateEntityQuery(typeof(AnimalPrefab)).GetSingleton<AnimalPrefab>().value;
            _plantPrefab = EntityManager
                .CreateEntityQuery(typeof(PlantPrefab)).GetSingleton<PlantPrefab>().value;
        }

        private bool _hack;
        
        private void Update()
        {
            if (Time.frameCount % 100 != 0 || _hack) return;
            // _spawner = World.DefaultGameObjectInjectionWorld.CreateSystem<SpawnerSystem>();

            _hack = true; // TODO: ugly hack yet cuz need to wait physics world to start
            var physicsWorldSystem =
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        
            var rayInputs = new RaycastInput[maxAnimals + maxPlants];
            var rayOutputs = new Unity.Physics.RaycastHit[maxAnimals + maxPlants];
            // Compute positions for both animals and plants
            var filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0,
            };
            for (var i = 0; i < maxAnimals + maxPlants; i++)
            {
                var pos = new float3(mapSize, 0, mapSize) + new float3(Random.Range(-mapSize, mapSize), 0, Random.Range(-mapSize, mapSize));
                rayInputs[i] = new RaycastInput
                {
                    Start = pos - new float3(0, 1, 0) * 1000,
                    End = pos + new float3(0, 1, 0) * 1000,
                    Filter = filter,
                };
            }
        
            collisionWorld.RayJob(rayInputs, ref rayOutputs);
            var terrain = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Terrain>())
                .GetSingletonEntity();
            // Then first half for animals
            for (var i = 0; i < maxAnimals; i++)
            {
                SpawnAnimal(rayOutputs[i].Position);
                // _spawner.EnqueueAnimal(rayOutputs[i].Position);
            }
        
            // Second half for plants
            for (var i = maxAnimals; i < rayOutputs.Length; i++)
            {
                SpawnPlant(rayOutputs[i].Position, terrain);
                // _spawner.EnqueuePlant(rayOutputs[i].Position);
            }
        }
        
        private void SpawnAnimal(float3 p)
        {
            if (p.Equals(default)) return;
            var e = EntityManager.Instantiate(_animalPrefab);
            EntityManager.AddComponentData(e, new NavAgent
            {
                TranslationSpeed = 100,
                RotationSpeed = 0.3f,
                TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                Offset = new float3(0, 0.5f, 0)
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
        
            EntityManager.AddComponent(e, ComponentType.ReadOnly<Animal>());
            EntityManager.AddComponent(e, ComponentType.ReadOnly<Herbivorous>());
            EntityManager.AddComponentData(e, new Decision {action = ActionType.Null});
            
            // All the arrays ! //
            
            var actionValues = EntityManager.AddBuffer<ActionValue>(e);
            for (var j = 0; j < Enum.GetNames(typeof(ActionType)).Length; j++)
            {
                // All action values to 0 (@ActionValueSystem takes charge of that according to current entity state)
                actionValues.Add(0);
            }
            EntityManager.AddComponent(e, ComponentType.ReadWrite<ActionValue>());
        
            // All characteristics to max at start (energy, satiation ...)
            var characteristicValues = EntityManager.AddBuffer<CharacteristicValue>(e);
            for (var j = 0; j < Enum.GetNames(typeof(CharacteristicType)).Length; j++)
            {
                characteristicValues.Add(1);
            }
            EntityManager.AddComponent(e, ComponentType.ReadWrite<CharacteristicValue>());
        
            // For each actions, the impact it has on every characteristics (hard coded)
            var characteristicChanges = EntityManager.AddBuffer<CharacteristicChanges>(e);
            // Action null only harms us ... (yet)                                              // satiation, hydration, age, energy
            characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.01f, -.01f }});
            // Action eat restores satiation, lower hydration, youth and energy
            characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { +.05f, -.01f, -.01f, -.01f }});
            // Action sleep restores energy, lower hydration, youth, energy
            characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.001f, +.05f }});
            // Action wander only harms us ... (yet)
            characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.1f, -.01f, -.01f, -.01f }});
            // Action look for food only harms us ... (yet)
            characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.01f, -.01f }});
            // Action reach only harms us ... (yet)
            characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.01f, -.01f }});
            // Action look for mate only harms us ... (yet)
            characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.01f, -.01f }});
            // Action mate lower energy much faster
            characteristicChanges.Add(new CharacteristicChanges {value = new FixedListFloat32 { -.01f, -.01f, -.01f, -.05f }});
            EntityManager.AddComponent(e, ComponentType.ReadWrite<CharacteristicValue>());
        }
        
        private void SpawnPlant(float3 p, Entity parent)
        {
            var e = EntityManager.Instantiate(_plantPrefab);
        
            EntityManager.AddComponentData(e, new Translation
            {
                Value = p + new float3(0, 0.5f, 0)
            });
            EntityManager.AddComponent<LocalToWorld>(e);
            EntityManager.AddComponent<Parent>(e);
            EntityManager.AddComponent<LocalToParent>(e);
            EntityManager.SetComponentData(e, new Parent {Value = parent});
            EntityManager.AddComponent(e, ComponentType.ReadOnly<Plant>());
            // Plants are brain-dead blocs yet, later should introduce (asexual usually) reproduction etc.
        }
    }
}
