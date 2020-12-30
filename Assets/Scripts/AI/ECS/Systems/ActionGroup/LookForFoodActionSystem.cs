﻿using AI.ECS.Components;
using Reese.Nav;
using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Plane = UnityEngine.Plane;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public unsafe class LookForFoodActionSystem : SystemBase
    {
        private BuildPhysicsWorld BuildPhysicsWorld
            => World.GetExistingSystem<BuildPhysicsWorld>();

        private NavGroundingSystem NavGrounding
            => World.GetExistingSystem<NavGroundingSystem>();

        private EntityCommandBufferSystem Barrier
            => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var ecb = Barrier.CreateCommandBuffer().AsParallelWriter();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            var plantMask = ~(uint) LayerMask.NameToLayer("Plant");
            var herbivorousAnimalMask = ~(uint) LayerMask.NameToLayer("HerbivorousAnimal");
            var physicsWorld = BuildPhysicsWorld.PhysicsWorld;
            var collisionWorld = physicsWorld.CollisionWorld;
            Dependency = JobHandle.CombineDependencies(Dependency,
                BuildPhysicsWorld.GetOutputDependency(),
                NavGrounding.GetOutputDependency());

            var deltaTime = Time.DeltaTime;

            Entities.ForEach((Entity entity,
                int entityInQueryIndex,
                ref DynamicBuffer<CharacteristicValue> characteristicValues,
                in DynamicBuffer<CharacteristicChanges> characteristicChanges,
                in LookForFoodAction _) =>
            {
                for (var i = 0; i < characteristicChanges[(int) ActionType.LookForFood].value.Length; i++)
                {
                    characteristicValues[i] = 
                        math.clamp(characteristicValues[i] + 
                                   characteristicChanges[(int) ActionType.LookForFood].value[i] * deltaTime, 0f, 1f);
                }
            }).ScheduleParallel();

            Entities
                .WithNone<NavHasProblem, NavNeedsDestination, NavPlanning>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, 
                    int entityInQueryIndex, 
                    int nativeThreadIndex, 
                    ref NavAgent agent, 
                    in Parent surface, 
                    in LocalToWorld localToWorld,
                    in LookForFoodAction _) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        !jumpableBufferFromEntity.HasComponent(surface.Value)
                    ) return;
            
                    var jumpableSurfaces = jumpableBufferFromEntity[surface.Value];
                    var random = randomArray[nativeThreadIndex];
                    if (
                        physicsWorld.GetPointOnSurfaceLayer(
                            localToWorld,
                            NavUtil.GetRandomPointInBounds(
                                ref random,
                                renderBoundsFromEntity[surface.Value].Value, // TODO: smaller AABB
                                random.NextFloat(0.1f, 1f)
                            ),
                            out var validDestination
                        )
                    )
                    {
                        ecb.AddComponent(entityInQueryIndex, entity, new NavNeedsDestination
                        {
                            Destination = validDestination
                        });
                    }
            
                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("LookForFoodRandomPositionJob")
                .ScheduleParallel();
            
            Barrier.AddJobHandleForProducer(Dependency);
            
            Entities
                .WithNone<Target>()
                .WithReadOnly(collisionWorld)
                .ForEach((Entity entity,
                    int entityInQueryIndex,
                    in LookForFoodAction act,
                    in LocalToWorld localToWorld) =>
                {
                    var r = 100;
                    // Debug.Log($"pos {localToWorld.Position}");
                    var sphereCollider = (Unity.Physics.Collider*) Unity.Physics.SphereCollider.Create(
                        new SphereGeometry
                        {
                            Center = localToWorld.Position,
                            Radius = r
                        }, new CollisionFilter
                        {
                            BelongsTo = ~0u,
                            CollidesWith = plantMask
                        }).GetUnsafePtr();
                    var colliderCastInput = new ColliderCastInput
                    {
                        Orientation = quaternion.identity,
                        Collider = sphereCollider
                    };  

                    if (collisionWorld.CastCollider(colliderCastInput, out var output) &&
                        HasComponent<Plant>(output.Entity))
                    {
                        // Gives target
                        ecb.AddComponent(entityInQueryIndex, entity, new Target
                        {
                            target = output.Entity
                        });
                        // Reset path finding
                        ecb.RemoveComponent<NavNeedsDestination>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavHasProblem>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<NavLerping>(entityInQueryIndex, entity);
                    }
                }).ScheduleParallel();
            // TODO: carnivorous
            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
