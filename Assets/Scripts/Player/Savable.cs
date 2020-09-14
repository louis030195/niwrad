using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Object = UnityEngine.Object;

namespace Player
{
    /// <summary>
    /// This class is meant to be inherited by plain class saved / loaded at runtime
    /// </summary>
    [Serializable]
    public abstract class Savable
    {
        private const string SliderTemplatePath = "Prefabs/SliderTemplate";
        private const string CheckboxTemplatePath = "Prefabs/CheckboxTemplate";
        private static GameObject _defaultSlider = Resources.Load(SliderTemplatePath) as GameObject;
        private static GameObject _defaultCheckBox = Resources.Load(CheckboxTemplatePath) as GameObject;


        public void Save()
        {
            // JsonUtility.ToJson(this);
            
        }

        public void Load()
        {
            
        }
        
        /// <summary>
        /// Render UI in-game based on given templates for numbers and booleans as child to given parent
        /// (typically a ScrollView content with grid layout)
        /// </summary>
        /// <param name="sliderTemplate"></param>
        /// <param name="checkboxTemplate"></param>
        /// <param name="parent"></param>
        public void Render(Transform parent, GameObject sliderTemplate = null, GameObject checkboxTemplate = null)
        {
            sliderTemplate = sliderTemplate ? sliderTemplate : _defaultSlider;
            checkboxTemplate = checkboxTemplate ? checkboxTemplate : _defaultCheckBox;
            var fields = GetType().GetFields();
            foreach (var field in fields)
            {
                var val = field.GetValue(this);
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
                else
                {
                    throw new Exception($"Tried to render {val}, {val.GetType()} type not handled !");
                }
            }
        }
    }
}
