using System;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Lean.Gui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Object = UnityEngine.Object;

namespace UI
{
    public static class CodeToUi
    {
        private const string SliderTemplatePath = "Prefabs/GreySliderTemplate";
        private const string CheckboxTemplatePath = "Prefabs/GreyCheckboxTemplate"; // TODO: should throw if cant find asset
        private static readonly GameObject DefaultSlider = Resources.Load(SliderTemplatePath) as GameObject;
        private static readonly GameObject DefaultCheckBox = Resources.Load(CheckboxTemplatePath) as GameObject;

        /// <summary>
        /// Given an object with number fields, an object of same number of fields representing the minimum value and
        /// another object of same number of fields representing the maximum value, create sliders for each fields properly
        /// configured as a child to the given parent object
        /// </summary>
        /// <param name="min">Minimum slider value</param>
        /// <param name="max">Maximum slider value</param>
        /// <param name="value">Slider value</param>
        /// <param name="parent">Parent object</param>
        /// <param name="sliderTemplate">Object with 1: label text mp pro gui, 2 slider, 3 value text mp pro gui</param>
        /// <param name="tooltips"></param>
        public static List<(PropertyInfo p, Slider s)> FloatsToUi(object min, object max, object value, Transform parent, 
            GameObject sliderTemplate = null, string[] tooltips = null)
        {
            sliderTemplate = sliderTemplate ? sliderTemplate : DefaultSlider;
            var ret = new List<(PropertyInfo p, Slider s)>();
            var minProps = min.GetType().GetProperties();
            var maxProps = max.GetType().GetProperties();
            var valueProps = value.GetType().GetProperties();
            var nonNumberCount = 0;
            for (var i = 0; i < minProps.Length; i++)
            {
                var minVal = minProps[i].GetValue(min);
                if (minVal == null) continue;
                var maxVal = maxProps[i].GetValue(max);
                var valVal = valueProps[i].GetValue(value);
                if (minVal.IsNumber())
                {
                    var tooltip = tooltips != null && i - nonNumberCount < tooltips.LongLength ? 
                        tooltips[i - nonNumberCount] :
                        "";
                    ret.Add((p: minProps[i], s: NumberToUi(minVal, maxVal, valVal, parent, minProps[i].Name, 
                        sliderTemplate, tooltip)));
                } else if (minVal.GetType().GetFields().Length > 0) // Object
                {
                    // TODO: could put as sub child in a new GO with object name ...
                    ret.AddRange(FloatsToUi(minVal, maxVal, valVal, parent));
                }
                else
                {
                    nonNumberCount++; // Only used for tooltips
                }
            }

            return ret;
        }

        /// <summary>
        /// Given a number, a minimum and maximum, create a slider matching these values and the given name
        /// as a child of the given parent object
        /// TODO: possible to constraint to number with generic ? doubt so
        /// TODO: tooltip like stuff
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="sliderTemplate">Object with 1: label text mp pro gui, 2 slider, 3 value text mp pro gui</param>
        /// <param name="tooltip"></param>
        public static Slider NumberToUi(object min, object max, object value, Transform parent, string name, 
            GameObject sliderTemplate = null, string tooltip = "")
        {
            if (!min.IsNumber()) throw new Exception("This method only accept numbers");
            sliderTemplate = sliderTemplate ? sliderTemplate : DefaultSlider;
            var go = Object.Instantiate(sliderTemplate, parent);
            var s = go.GetComponentInChildren<Slider>();
            s.minValue = Convert.ToSingle(min);
            s.maxValue = Convert.ToSingle(max);
            s.value = Convert.ToSingle(value);
            go.GetComponent<LeanTooltipData>().Text = tooltip;
            var labelValue = go.transform.GetChild(0);
            labelValue.GetComponent<TextMeshProUGUI>().text = $"{name}";
            var sliderValue = go.transform.GetChild(2);
            var sliderValueText = sliderValue.GetComponent<TextMeshProUGUI>();
            sliderValueText.text = $"{s.value:0}";
            s.onValueChanged.AddListener(v => sliderValueText.text = $"{v:0}");
            return s;
        }
        
        public static Toggle BooleanToUI(this bool value, Transform parent, string name, GameObject checkBoxTemplate = null)
        {
            checkBoxTemplate = checkBoxTemplate ? checkBoxTemplate : DefaultCheckBox;
            var go = Object.Instantiate(checkBoxTemplate, parent);
            var t = go.GetComponentInChildren<Toggle>();
            t.isOn = value;
            var labelValue = go.transform.GetChild(0);
            labelValue.GetComponent<TextMeshProUGUI>().text = $"{name}";
            return t;
        }
        
        public static async UniTaskVoid FadeInAndOut(this Graphic target, bool fadeIn, float duration)
        {
            //Set Values depending on if fadeIn or fadeOut
            float a, b;
            if (fadeIn)
            {
                a = 0f;
                b = 1f;
            }
            else
            {
                a = 1f;
                b = 0f;
            }

            var currentColor = target.color;
            var counter = 0f;

            while (counter < duration)
            {
                counter += Time.deltaTime;
                var alpha = Mathf.Lerp(a, b, counter / duration);

                target.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                await UniTask.Yield();
            }
        }
    }
}
