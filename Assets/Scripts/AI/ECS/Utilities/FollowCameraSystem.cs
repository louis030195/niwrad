using AI.ECS.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace AI.ECS.Utilities
{
    // Off now, this thing  doesn't work in ECS actually
    [DisableAutoCreation]
    public class FollowCameraSystem : SystemBase
    {
        protected override void OnUpdate()
        {

            var pos = EntityManager
                .CreateEntityQuery(typeof(Animal), typeof(Translation))
                .ToComponentDataArray<Translation>(Allocator.TempJob);
            Entities
                .WithoutBurst()
                .ForEach((Entity entity,
                int entityInQueryIndex,
                ref Translation translation,
                in Camera _1) =>
            {
                if (pos.Length > 0) translation.Value = pos[0].Value;
                else Debug.Log($"No animal found");
            }).Run();
            pos.Dispose();
        }
    }
}
