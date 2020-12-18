using Unity.Entities;

namespace AI.ECS.Systems.AIGroup
{
    public class AISystemGroup : ComponentSystemGroup
    {
        private const float AIUpdateFrequency = 1f;// times per seconds
        private const float AIUpdateInterval = 1f / AIUpdateFrequency;
        private float _aiUpdateCooldown;

        protected override void OnCreate()
        {
            base.OnCreate();

            AddSystemToUpdateList(World.CreateSystem<ActionValueSystem>());
            AddSystemToUpdateList(World.CreateSystem<PolicySystem>());
        }

        protected override void OnUpdate()
        {
            // AI system should update in a lower frequency
            if ( _aiUpdateCooldown <= 0.0f )
            {
                _aiUpdateCooldown += AIUpdateInterval;

                base.OnUpdate();
            }

            _aiUpdateCooldown -= Time.DeltaTime;
        }
    }
}
