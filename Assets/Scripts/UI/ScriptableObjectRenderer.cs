using System.Runtime.Versioning;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public static class ScriptableObjectRenderer
    {
        private const string SliderTemplatePath = "Prefabs/SliderTemplate";
        private const string CheckboxTemplatePath = "Prefabs/CheckboxTemplate";
        private static GameObject defaultSlider = Resources.Load(SliderTemplatePath) as GameObject;
        private static GameObject defaultCheckBox = Resources.Load(CheckboxTemplatePath) as GameObject;

        public static bool IsNumber(this object value) // TODO: move somewhere else
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
        
        /// <summary>
        /// Render ScriptableObject UI in-game based on given templates for numbers and booleans as child to given parent
        /// (typically a ScrollView content with grid layout)
        /// </summary>
        /// <param name="so"></param>
        /// <param name="sliderTemplate"></param>
        /// <param name="checkboxTemplate"></param>
        /// <param name="parent"></param>
        public static void Render(this ScriptableObject so, Transform parent, GameObject sliderTemplate = null, GameObject checkboxTemplate = null)
        {
            sliderTemplate = sliderTemplate ? sliderTemplate : defaultSlider;
            checkboxTemplate = checkboxTemplate ? checkboxTemplate : defaultCheckBox;
            var fields = so.GetType().GetFields();
            foreach (var field in fields)
            {
                var val = field.GetValue(so);
                if (val.IsNumber())
                {
                    foreach (var attribute in field.GetCustomAttributes(true))
                    {
                        if (attribute is RangeAttribute r)
                        {
                            var go = Object.Instantiate(sliderTemplate, parent);
                            var s = go.GetComponentInChildren<Slider>();
                            s.minValue = r.min;
                            s.maxValue = r.max;

                            s.value = val is int i ? i : (float) val;
                            var labelValue = go.transform.GetChild(0);
                            labelValue.GetComponent<TextMeshProUGUI>().text = $"{field.Name}";
                            var sliderValue = go.transform.GetChild(2);
                            var sliderValueText = sliderValue.GetComponent<TextMeshProUGUI>();
                            sliderValueText.text = $"{s.value}";
                            s.onValueChanged.AddListener(value => sliderValueText.text = $"{value}");
                        }
                    }
                } else if (val is bool b)
                {
                    var go = Object.Instantiate(checkboxTemplate, parent);
                    var t = go.GetComponentInChildren<Toggle>();
                    t.isOn = b;
                    var labelValue = go.transform.GetChild(0);
                    labelValue.GetComponent<TextMeshProUGUI>().text = $"{field.Name}";
                }
            }
        }
    }
}
