using System.Collections.Generic;
using UnityEngine;

namespace Evolution
{
    /// <summary>
    /// Stores vegetation-specific related parameters
    /// </summary>
    [CreateAssetMenu(fileName = "VegetationCharacteristics", menuName = "ScriptableObjects/VegetationCharacteristics", order = 1)]
    public class VegetationCharacteristics : HostCharacteristics
    {
        [Header("Reproduction"), Range(5, 1000)]
        public float reproductionSprayRadius = 100f;
        [Range(2, 100)]
        public float reproductionDistanceBetween= 5f;
        [Range(1, 100)]
        public float reproductionProbability = 10f;
    }
}
