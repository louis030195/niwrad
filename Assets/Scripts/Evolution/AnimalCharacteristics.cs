using System.Collections.Generic;
using UnityEngine;

namespace Evolution
{
    /// <summary>
    /// Stores animal-specific related parameters, e.g. vegetation don't usually move unlike animals
    /// </summary>
    [CreateAssetMenu(fileName = "AnimalCharacteristics", menuName = "ScriptableObjects/AnimalCharacteristics", order = 1)]
    public class AnimalCharacteristics : HostCharacteristics
    {
        [Header("Initial characteristics")]
        [Range(2, 20)]
        public float initialSpeed = 5f;
        [Range(1, 1000)]
        public float randomMovementRange = 20f;
        [Range(1, 1000)]
        public float sightRange = 20f;
        [Range(2f, 10.0f)]
        public float eatRange = 5f;
        [Range(1, 100.0f), Tooltip("How much life eating bring")]
        public float metabolism = 10f;

        [Header("Reproduction")]
        [Range(20, 80)]
        public float reproductionLifeLoss = 50f;
    }
}
