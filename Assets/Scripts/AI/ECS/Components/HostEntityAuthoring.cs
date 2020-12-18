using Unity.Entities;
using UnityEngine;

namespace AI.ECS.Components
{
    /// <summary>For authoring a host prefab.</summary>
    [GenerateAuthoringComponent]
    internal struct HostPrefab : IComponentData
    {
        /// <summary>A reference to the host prefab as an Entity.</summary>
        public Entity Value;
    }
}
