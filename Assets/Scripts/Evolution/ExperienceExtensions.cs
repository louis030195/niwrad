using System;
using System.IO;
using System.Linq;
using Api.Realtime;
using Google.Protobuf;
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
        
        public static void Save(this Experience e)
        {
            if(!Directory.Exists(ExperiencesPath)) Directory.CreateDirectory(ExperiencesPath);
            var json = JsonFormatter.Default.Format(e);
            File.WriteAllText($"{ExperiencesPath}/{e.Name}.json", FormatJson(json));
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
