using System;
using System.IO;
using System.Linq;
using Api.Realtime;
using Google.Protobuf;
using Protometry.Vector3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Object = UnityEngine.Object;
using Transform = UnityEngine.Transform;

namespace Evolution
{
    public static class ExperienceExtensions
    {
        private const string SliderTemplatePath = "Prefabs/SliderTemplate";
        private const string CheckboxTemplatePath = "Prefabs/CheckboxTemplate";
        private static readonly GameObject DefaultSlider = Resources.Load(SliderTemplatePath) as GameObject;
        private static readonly GameObject DefaultCheckBox = Resources.Load(CheckboxTemplatePath) as GameObject;
        private static readonly string ExperiencesPath = $"{Application.persistentDataPath}/Experiences";

        private const string IndentString = "    ";

        static string FormatJson(string json) {

            int indentation = 0;
            int quoteCount = 0;
            var result = 
                from ch in json
                let quotes = ch == '"' ? quoteCount++ : quoteCount
                let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine +  String.Concat(Enumerable.Repeat(IndentString, indentation)) : null
                let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + String.Concat(Enumerable.Repeat(IndentString, ++indentation)) : ch.ToString()
                let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + String.Concat(Enumerable.Repeat(IndentString, --indentation)) + ch : ch.ToString()
                select lineBreak ?? (openChar.Length > 1 
                    ? openChar 
                    : closeChar);

            return string.Concat(result);
        }
        
        public static bool Save(this Experience e)
        {
            if(!Directory.Exists(ExperiencesPath)) Directory.CreateDirectory(ExperiencesPath);
            var isNew = !File.Exists($"{ExperiencesPath}/{e.Name}.json");
            var json = JsonFormatter.Default.Format(e);
            File.WriteAllText($"{ExperiencesPath}/{e.Name}.json", FormatJson(json));
            return isNew;
        }

        public static bool NewIfNone()
        {
            if(!Directory.Exists(ExperiencesPath)) Directory.CreateDirectory(ExperiencesPath);
            if (Directory.GetFiles(ExperiencesPath).Length != 0) return false;
            New();
            return true;
        }

        /// <summary>
        /// Create a new experience without erasing previous ones (increment name), save to disk and return it
        /// </summary>
        /// <returns></returns>
        public static Experience New()
        {
            if(!Directory.Exists(ExperiencesPath)) Directory.CreateDirectory(ExperiencesPath);
            // var e = Load($"Assets/Scripts/Tests/Data/BasicExperience.json", true);
            // TODO: normalize value, either all are percentage then tweaked in logic either direct value
            var e = new Experience
            {
                Name = "BasicExperience",
                IncludeCarnivorous = true, // include carnivorous by default but not much
                CarnivorousPercent = 10, // 10% chance born carnivorous (first spawn)
                Timescale = 1,
                AnimalCharacteristics = new Characteristics
                {
                    Computation = 50,
                    Life = 50,
                    Robustness = 50,
                    Energy = 50,
                    ReproductionCost = 90,
                    EnergyLoss = 100,
                    EatEnergyGain = 25,
                    DrinkEnergyGain = 50,
                    ReproductionDelay = 25,
                    ReproductionProbability = 25,
                    AnimalCharacteristics = new Characteristics.Types.AnimalCharacteristics
                    {
                        Speed = 5,
                        RandomMovementRange = 50,
                        SightRange = 50,
                        EatRange = 1,
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
                    EnergyLoss = 0,
                    EatEnergyGain = 10,
                    DrinkEnergyGain = 10,
                    ReproductionDelay = 0,
                    ReproductionProbability = 0,
                    AnimalCharacteristics = new Characteristics.Types.AnimalCharacteristics
                    {
                        Speed = 1,
                        RandomMovementRange = 1,
                        SightRange = 1,
                        EatRange = 1,
                        Metabolism = 1,
                    }
                },
                AnimalCharacteristicsMaximumBound = new Characteristics
                {
                    Computation = 100,
                    Life = 100,
                    Robustness = 100,
                    Energy = 100,
                    ReproductionCost = 100,
                    EnergyLoss = 100,
                    EatEnergyGain = 100,
                    DrinkEnergyGain = 100,
                    ReproductionDelay = 100,
                    ReproductionProbability = 100,
                    AnimalCharacteristics = new Characteristics.Types.AnimalCharacteristics
                    {
                        Speed = 100,
                        RandomMovementRange = 100,
                        SightRange = 100,
                        EatRange = 10,
                        Metabolism = 100,
                    }
                },
                AnimalDistribution = new Experience.Types.PopulationDistribution
                {
                  InitialAmount = 100,
                  Scattering = 10,
                  Radius = 150
                },
                PlantCharacteristics = new Characteristics
                {
                    Computation = 5,
                    Life = 50,
                    Robustness = 50,
                    Energy = 50,
                    ReproductionCost = 0,
                    EnergyLoss = 25,
                    EatEnergyGain = 10,
                    DrinkEnergyGain = 50,
                    ReproductionDelay = 5,
                    ReproductionProbability = 25,
                    PlantCharacteristics = new Characteristics.Types.PlantCharacteristics()
                },
                PlantCharacteristicsMinimumBound = new Characteristics
                {
                    Computation = 0,
                    Life = 0,
                    Robustness = 0,
                    Energy = 0,
                    ReproductionCost = 0,
                    EnergyLoss = 0,
                    EatEnergyGain = 10,
                    DrinkEnergyGain = 10,
                    ReproductionDelay = 0,
                    ReproductionProbability = 0,
                    PlantCharacteristics = new Characteristics.Types.PlantCharacteristics()
                },
                PlantCharacteristicsMaximumBound = new Characteristics
                {
                    Computation = 100,
                    Life = 100,
                    Robustness = 100,
                    Energy = 100,
                    ReproductionCost = 100,
                    EnergyLoss = 100,
                    EatEnergyGain = 100,
                    DrinkEnergyGain = 100,
                    ReproductionDelay = 100,
                    ReproductionProbability = 100,
                    PlantCharacteristics = new Characteristics.Types.PlantCharacteristics()
                },
                PlantDistribution = new Experience.Types.PopulationDistribution
                {
                    InitialAmount = 200,
                    Scattering = 10,
                    Radius = 150
                },
            }; // TODO: move somewhere else
            var i = 0;
            while (File.Exists($"{ExperiencesPath}/BasicExperience-{i}.json"))
            {
                i++;
            } 
            e.Name = $"BasicExperience-{i}";
            e.Save();
            return e;
        }
        
        public static void Delete(this Experience e)
        {
            File.Delete($"{ExperiencesPath}/{e.Name}.json");
        }

        /// <summary>
        /// Load an experience from json file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="absolute">Whether the given path is absolute, default false</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Experience Load(string path, bool absolute = false)
        {
            if(!Directory.Exists(ExperiencesPath)) throw new Exception("Experiences folder doesn't exist !");
            string realPath;
            if (File.Exists($"{ExperiencesPath}/{path}"))
               realPath = $"{ExperiencesPath}/{path}";
            else if (File.Exists($"{path}.json"))
                realPath = $"{path}.json";
            else if (absolute && File.Exists(path))
                realPath = path;
            else
                // If the file (absolute path or not) with or without the json ext doesn't exists, throw
                throw new Exception("Experience file doesn't exist !");
            
            var json = File.ReadAllText(realPath);
            return Experience.Parser.ParseJson(json);
        }
    }
}
