using System.IO;
using Api.Realtime;
using Evolution;
using NUnit.Framework;
using UnityEngine;

namespace Tests.TestsEdit
{
    public class EvolutionTestsEdit
    {
        /// <summary>
        /// Check that a characteristics == b characteristics
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool AreObjectFloatsEqual(object a, object b)
        {
            var aFields = a.GetType().GetProperties();
            var bFields = b.GetType().GetProperties();
            for (var i = 0; i < aFields.Length; i++)
            {
                var val = aFields[i].GetValue(a);
                if (val == null) continue;
                // If contains object field, call recursive equality check
                if (!(val is float) && val.GetType().GetFields().Length > 0)
                {
                    if (!AreObjectFloatsEqual(aFields[i].GetValue(a), bFields[i].GetValue(b))) return false;
                }

                // Skipping non-float values
                if (!(aFields[i].GetValue(a) is float)) continue;
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
            var e = ExperienceExtensions.Load($"Assets/Scripts/Tests/Data/BasicExperience.json", true);
            var child = new Characteristics
            {
                Computation = 50,
                Life = 50,
                Robustness = 50,
                Energy = 50,
                ReproductionCost = 50,
                AnimalCharacteristics = new Characteristics.Types.AnimalCharacteristics
                {
                    Speed = 50,
                    RandomMovementRange = 50,
                    SightRange = 50,
                    EatRange = 50,
                    Metabolism = 50,
                }
            };
            child.Mutate(e.AnimalCharacteristics, e.AnimalCharacteristics, e);
            // The offspring should have different characteristics
            Assert.False(AreObjectFloatsEqual(child, e.AnimalCharacteristics)); // TODO: fix this assert
            // TODO: could go further: assert that it stays within a distribution while differing from the initial value
            // TODO: why ? could have hard coded stuff but mutation rate and range might change
        }
        // TODO: test save evolution stuff to disk etc.

        [Test]
        public void SaveExperience()
        {
            var exp = new Experience
            {
                Name = "BasicExperience",
                AnimalCharacteristics = new Characteristics
                {
                    Computation = 50,
                    Life = 50,
                    Robustness = 50,
                    Energy = 50,
                    ReproductionCost = 50,
                    AnimalCharacteristics = new Characteristics.Types.AnimalCharacteristics
                    {
                        Speed = 50,
                        RandomMovementRange = 50,
                        SightRange = 50,
                        EatRange = 50,
                        Metabolism = 50,
                    }
                },
                AnimalCharacteristicsMinimumBound = new Characteristics
                {
                    Computation = 0,
                    Life = 0,
                    Robustness = 0,
                    Energy = 0,
                    ReproductionCost = 0,
                    AnimalCharacteristics = new Characteristics.Types.AnimalCharacteristics
                    {
                        Speed = 0,
                        RandomMovementRange = 0,
                        SightRange = 0,
                        EatRange = 0,
                        Metabolism = 0,
                    }
                },
                AnimalCharacteristicsMaximumBound = new Characteristics
                {
                    Computation = 100,
                    Life = 100,
                    Robustness = 100,
                    Energy = 100,
                    ReproductionCost = 100,
                    AnimalCharacteristics = new Characteristics.Types.AnimalCharacteristics
                    {
                        Speed = 100,
                        RandomMovementRange = 100,
                        SightRange = 100,
                        EatRange = 100,
                        Metabolism = 100,
                    }
                },
                AnimalDistribution = new Experience.Types.PopulationDistribution
                {
                  InitialAmount = 20,
                  Scattering = 10
                },
                VegetationCharacteristics = new Characteristics
                {
                    Computation = 50,
                    Life = 50,
                    Robustness = 50,
                    Energy = 50,
                    ReproductionCost = 50,
                    VegetationCharacteristics = new Characteristics.Types.VegetationCharacteristics()
                },
                VegetationCharacteristicsMinimumBound = new Characteristics
                {
                    Computation = 0,
                    Life = 0,
                    Robustness = 0,
                    Energy = 0,
                    ReproductionCost = 0,
                    VegetationCharacteristics = new Characteristics.Types.VegetationCharacteristics()
                },
                VegetationCharacteristicsMaximumBound = new Characteristics
                {
                    Computation = 100,
                    Life = 100,
                    Robustness = 100,
                    Energy = 100,
                    ReproductionCost = 100,
                    VegetationCharacteristics = new Characteristics.Types.VegetationCharacteristics()
                },
                VegetationDistribution = new Experience.Types.PopulationDistribution
                {
                    InitialAmount = 20,
                    Scattering = 10
                },
            };
            exp.Save();
            Debug.Log(Application.persistentDataPath);
            Assert.True(File.Exists($"{Application.persistentDataPath}/Experiences/BasicExperience.json"));
        }

        [Test]
        public void LoadExperience()
        {
            var e = ExperienceExtensions.Load($"Assets/Scripts/Tests/Data/BasicExperience.json", true);
            Debug.Log(e);
            Assert.NotNull(e);
        }
    }
}
