using System;
using System.Reflection;
using Evolution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Given a HostCharacteristics, will spawn an UI to tweak these characteristics
    /// </summary>
    public class EvolutionMenu : Menu
    {
        /// <summary>
        /// characteristics should be given as a copy ! We don't necessarily want to save the tweaked characteristics
        /// </summary>
        /*[HideInInspector] */public HostCharacteristics characteristics;
        [SerializeField] private GameObject characteristicTemplate;
        [SerializeField] private GameObject characteristicList;
        private void Start()
        {
            // TODO: how to handle non-continuous characteristics (boolean, discrete ...) ?
            // for each characteristics
            var fields = characteristics.GetType().GetFields();
            foreach (var field in fields)
            {
                var characteristicGo = Instantiate(characteristicTemplate, characteristicList.transform);
                if (!characteristics.RangeAttributes.ContainsKey(field.Name)) continue;
                var r = characteristics.RangeAttributes[field.Name];
                var s = characteristicGo.GetComponentInChildren<Slider>();
                s.minValue = r.min;
                s.maxValue = r.max;
                var val = field.GetValue(characteristics);
                if (val is float f) s.value = f;
                else Debug.LogError("Non-float characteristics being assigned to slider");
                var labelValue = characteristicGo.transform.GetChild(0);
                labelValue.GetComponent<TextMeshProUGUI>().text = $"{field.Name}";
                var sliderValue = characteristicGo.transform.GetChild(2);
                var sliderValueText = sliderValue.GetComponent<TextMeshProUGUI>();
                sliderValueText.text = $"{s.value}";
                s.onValueChanged.AddListener(value => sliderValueText.text = $"{value}");
            }
        }
    }
}
