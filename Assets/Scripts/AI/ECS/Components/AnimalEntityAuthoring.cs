using Unity.Entities;
using UnityEngine;

namespace AI.ECS.Components
{
    /// <summary>For authoring a host prefab.</summary>
    [GenerateAuthoringComponent]
    internal struct AnimalPrefab : IComponentData
    {
        /// <summary>A reference to the animal prefab as an Entity.</summary>
#pragma warning disable 649
        public Entity value;
#pragma warning restore 649
    }
}
