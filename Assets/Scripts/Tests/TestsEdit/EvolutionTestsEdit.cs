using Evolution;
using NUnit.Framework;
using UnityEngine;

namespace Tests.TestsEdit
{
    public class EvolutionTestsEdit
    {
        private const string TestCharacteristicsPath = "ScriptableObjects/BasicAnimalCharacteristics";

        /// <summary>
        /// Check that a characteristics == b characteristics
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool AreCharacteristicsEqual(HostCharacteristics a, HostCharacteristics b)
        {
            var aFields = a.GetType().GetFields();
            var bFields = b.GetType().GetFields();
            for (var i = 0; i < aFields.Length; i++)
            {
                var targetPropertyName = aFields[i].Name;
                // Skipping non-float / non-ranged properties
                if (!a.RangeAttributes.ContainsKey(targetPropertyName)) continue;
                var r = a.RangeAttributes[targetPropertyName];
                var aCharacteristic = (float) aFields[i].GetValue(a);
                var bCharacteristic = (float) bFields[i].GetValue(b);
                if (!aCharacteristic.Equals(bCharacteristic)) return false;
            }

            return true;
        }
        
        [Test]
        public void TestMutate()
        {
            Random.InitState(666);
            var a = new AnimalCharacteristics();
            var b = new AnimalCharacteristics();
            var c = new AnimalCharacteristics();
            Assert.NotNull(a, $"Make sure test characteristics scriptable object is found under Assets/Resources/{TestCharacteristicsPath}");
            Assert.NotNull(b, $"Make sure test characteristics scriptable object is found under Assets/Resources/{TestCharacteristicsPath}");
            Assert.NotNull(c, $"Make sure test characteristics scriptable object is found under Assets/Resources/{TestCharacteristicsPath}");
            c.Mutate(a, b);
            // Shouldn't have changed any initial characteristics on parents
            Assert.True(AreCharacteristicsEqual(a, b));
            // The offspring should have different characteristics
            Assert.False(AreCharacteristicsEqual(a, c));
            Assert.False(AreCharacteristicsEqual(b, c));
            // TODO: could go further: assert that it stays within a distribution while differing from the initial value
            // TODO: why ? could have hard coded stuff but mutation rate and range might change
        }
    }
}
