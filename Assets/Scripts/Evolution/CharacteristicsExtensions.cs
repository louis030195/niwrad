using System;
using System.Reflection;
using Api.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Utils;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Transform = UnityEngine.Transform;

namespace Evolution
{
    public static class CharacteristicsExtensions
    {
        private static float Mutate(float a, float b, float mutationDegree)
        {
            var md = Mathf.Abs(mutationDegree) > 1 ? 1 : Mathf.Abs(mutationDegree);
            return (a + b) / 2 * (1 + Random.Range(-md, md));
        }

        public static (
            Characteristics, 
            Characteristics,
            FieldInfo[],
            FieldInfo[]
            ) GetCharacteristicsMinMax(this Characteristics characteristics, Experience experiences)
        {
            Characteristics minChar;
            Characteristics maxChar;
            FieldInfo[] minBounds;
            FieldInfo[] maxBounds;
            switch (characteristics.TypeCase)
            {
                case Characteristics.TypeOneofCase.None:
                    throw new Exception("Alien child not implemented !");
                case Characteristics.TypeOneofCase.AnimalCharacteristics:
                    minChar = experiences.AnimalCharacteristicsMinimumBound;
                    maxChar = experiences.AnimalCharacteristicsMaximumBound;
                    minBounds = experiences.AnimalCharacteristicsMinimumBound.GetType().GetFields();
                    maxBounds = experiences.AnimalCharacteristicsMaximumBound.GetType().GetFields();
                    break;
                case Characteristics.TypeOneofCase.PlantCharacteristics:
                    minChar = experiences.PlantCharacteristicsMinimumBound;
                    maxChar = experiences.PlantCharacteristicsMaximumBound;
                    minBounds = experiences.PlantCharacteristicsMinimumBound.GetType().GetFields();
                    maxBounds = experiences.PlantCharacteristicsMaximumBound.GetType().GetFields();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return (minChar, maxChar, minBounds, maxBounds);
        }

        /// <summary>
        /// Given an object, 2 parents object and some bounds, all objects with same fields type,
        /// will mutate the number values clamped between the bounds recursively
        /// </summary>
        /// <param name="child"></param>
        /// <param name="firstParent"></param>
        /// <param name="secondParent"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void MutateObjects( // TODO: gotta benchmark this, will be called quite often
            this object child, 
            object firstParent, 
            object secondParent,
            object min,
            object max)
        {
            // TODO: seems that c# object is fields, protobuf is properties
            var childFields = child.GetType().GetProperties();
            var firstParentFields = firstParent.GetType().GetProperties();
            var secondParentFields = secondParent.GetType().GetProperties();
            var minFields = min.GetType().GetProperties();
            var maxFields = max.GetType().GetProperties();
            if ((childFields.Length +
                 firstParentFields.Length +
                 secondParentFields.Length +
                 minFields.Length +
                 maxFields.Length) / 5 != childFields.Length)
            {
                throw new Exception("You can only mutate objects of same fields type !");
            }
            // For each field, mutate it clamped between experience bounds, simple !
            for (var i = 0; i < childFields.Length; i++)
            {
                var val = childFields[i].GetValue(child);
                if (val == null) continue;
                // Detected an object field potentially containing some number to mutate
                if (!(val is float) && val.GetType().GetFields().Length > 0)
                {
                    childFields[i].GetValue(child).MutateObjects(firstParentFields[i].GetValue(firstParent),
                        secondParentFields[i].GetValue(secondParent), 
                        minFields[i].GetValue(min), 
                        maxFields[i].GetValue(max));
                }

                // Skipping non-number values
                if (!(firstParentFields[i].GetValue(firstParent) is float)) continue;
                var firstParentField = (float) firstParentFields[i].GetValue(firstParent);
                var secondParentField = (float) secondParentFields[i].GetValue(secondParent);
                
                childFields[i].SetValue(child, Mathf.Clamp(Mutate(
                    firstParentField, 
                    secondParentField, 
                    1f), (float) minFields[i].GetValue(min), (float) maxFields[i].GetValue(max)));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="firstParent"></param>
        /// <param name="secondParent"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void Mutate( // TODO: gotta benchmark this, will be called quite often
            this Characteristics child, 
            Characteristics firstParent, 
            Characteristics secondParent,
            Characteristics min,
            Characteristics max)
        {
            child.MutateObjects(firstParent, secondParent, min, max);
        }
        
    }
}
