using Unity.Entities;
using UnityEngine;

namespace AI.ECS.Components
{
    /// <summary>For authoring a plant prefab.</summary>
    [GenerateAuthoringComponent]
    internal struct PlantPrefab : IComponentData
    {
        /// <summary>A reference to the plant prefab as an Entity.</summary>
#pragma warning disable 649
        public Entity value;
#pragma warning restore 649
    }
}
