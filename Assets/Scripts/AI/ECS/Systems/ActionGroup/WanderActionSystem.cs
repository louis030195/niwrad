using AI.ECS.Components;
using Reese.Nav;
using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;

namespace AI.ECS.Systems.ActionGroup
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(ActionSystemGroup))]
    public class WanderActionSystem : SystemBase
    {
        private BuildPhysicsWorld BuildPhysicsWorld 
            => World.GetExistingSystem<BuildPhysicsWorld>();
        private EntityCommandBufferSystem Barrier 
            => World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var physicsWorld = BuildPhysicsWorld.PhysicsWorld;
            var commandBuffer = Barrier.CreateCommandBuffer().AsParallelWriter();
            var jumpableBufferFromEntity = GetBufferFromEntity<NavJumpableBufferElement>(true);
            var renderBoundsFromEntity = GetComponentDataFromEntity<RenderBounds>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            Dependency = JobHandle.CombineDependencies(Dependency, BuildPhysicsWorld.GetOutputDependency());

            Entities.ForEach((ref Hungriness hunger,
                ref Tiredness tired,
                in WanderAction playAct) =>
            {
                hunger.Value = math.clamp(
                    hunger.Value + playAct.HungerCostPerSecond * deltaTime, 0f, 100f);
                tired.Value = math.clamp(
                    tired.Value + playAct.TirednessCostPerSecond * deltaTime, 0f, 100f);
            }).ScheduleParallel();
            
            Entities
                .WithNone<NavHasProblem, NavNeedsDestination, NavPlanning>()
                .WithReadOnly(jumpableBufferFromEntity)
                .WithReadOnly(renderBoundsFromEntity)
                .WithReadOnly(physicsWorld)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface, in LocalToWorld localToWorld) =>
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
                                renderBoundsFromEntity[surface.Value].Value,
                                1000
                            ),
                            out var validDestination
                        )
                    )
                    {
                        commandBuffer.AddComponent(entityInQueryIndex, entity, new NavNeedsDestination
                        {
                            Destination = validDestination
                        });
                    }

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("NavTerrainDestinationJob")
                .ScheduleParallel();

            Barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
