using Unity.Entities;

namespace AI.ECS.Systems.ActionGroup
{
    class ActionSystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            AddSystemToUpdateList(World.CreateSystem<EatActionSystem>());
            AddSystemToUpdateList(World.CreateSystem<SleepActionSystem>());
            AddSystemToUpdateList(World.CreateSystem<PlayActionSystem>());
        }
    }
}
