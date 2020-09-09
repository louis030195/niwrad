﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Evolution
{
    /// <summary>
    /// Stores common host (animal, vegetation, parasite ...) parameters.
    /// This is used to set initial population characteristics or maybe custom "fun" individual-level characteristics
    /// created at runtime.
    /// When reproduction occurs, the mutated parameters should NOT be saved.
    /// See https://www.reddit.com/r/Unity3D/comments/5zbkz8/how_do_you_not_save_changes_to_a_scriptable/
    /// </summary>
    [CreateAssetMenu(fileName = "HostCharacteristics", menuName = "ScriptableObjects/HostCharacteristics", order = 1)]
    public class HostCharacteristics : ScriptableObject
    {
        [Header("Evolution parameters"), Range(0.1f, 50f)]
        public float decisionFrequency = 1f;
        [Header("Initial characteristics"), Range(20, 80)]
        public float initialLife = 40f;
        [Tooltip("How much life losing over time"), Range(0.1f, 2.0f)] // TODO: fix names
        public float robustness = 1f;
        [Tooltip("Starting hunger, changed at runtime"), Range(0f, 100f)]
        public float hunger = 50f;
        [Tooltip("Necessary for every actions"), Range(0f, 100f)]
        public float energy = 50f; // TODO: implement

        [Header("Reproduction"), Range(20, 80)]
        public float reproductionThreshold = 80f;
        [Range(1, 100)]
        public float reproductionDelay = 20f;

        [NonSerialized] public readonly Dictionary<string, RangeAttribute> RangeAttributes = new Dictionary<string, RangeAttribute>();

        private void OnEnable()
        {
            var properties = GetType().GetFields();
            // Each characteristics is "fenced" in a range for balance. It's stored once to be reused
            foreach (var property in properties)
            {
                try
                {
                    RangeAttributes[property.Name] = ReflectionExtension.GetRange(GetType(), property.Name);
                }
                catch (Exception e)
                {
                    Debug.Log($"Ignoring {property.Name}: {e.Message}");
                }
            }
        }

        private float Mutate(float a, float b, float mutationDegree)
        {
            var md = Mathf.Abs(mutationDegree) > 1 ? 1 : Mathf.Abs(mutationDegree);
            return (a + b) / 2 * (1 + Random.Range(-md, md));
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstParent"></param>
        /// <param name="secondParent"></param>
        public void Mutate(HostCharacteristics firstParent, HostCharacteristics secondParent)
        {
            var targetProperties = GetType().GetFields();
            var firstParentProperties = firstParent.GetType().GetFields();
            var secondParentProperties = secondParent.GetType().GetFields();
            for (var i = 0; i < targetProperties.Length; i++)
            {
                var targetPropertyName = targetProperties[i].Name;
                // Skipping non-float / non-ranged properties
                if (!RangeAttributes.ContainsKey(targetPropertyName)) continue;
                var r = RangeAttributes[targetPropertyName];
                var firstParentCharacteristic = (float) firstParentProperties[i].GetValue(firstParent);
                var secondParentCharacteristic = (float) secondParentProperties[i].GetValue(secondParent);
                targetProperties[i].SetValue(this, Mathf.Clamp(Mutate(
                    firstParentCharacteristic, 
                    secondParentCharacteristic, 
                    1f), r.min, r.max));
            }
        }
    }
}
