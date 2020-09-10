using System.Collections.Generic;
using System.Linq;
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

        private const string ScriptableObjectsPath = "ScriptableObjects";
        [SerializeField] private TMP_Dropdown characteristicsDropDown;
        [SerializeField] private GameObject characteristicTemplate;
        [SerializeField] private GameObject characteristicList;
        
        /// <summary>
        /// characteristics should be given as a copy ! We don't necessarily want to save the tweaked characteristics
        /// </summary>
        private List<HostCharacteristics> _savedCharacteristics;
        // private HostCharacteristics _selectedCharacteristics;

        private void Start()
        {
            _savedCharacteristics = Resources.LoadAll(ScriptableObjectsPath, typeof(HostCharacteristics))
                .Select(o => (HostCharacteristics) o).ToList();
            characteristicsDropDown.AddOptions(_savedCharacteristics.Select(c => c.name).ToList());
            characteristicsDropDown.onValueChanged.AddListener(i => InstanciateScrollView(_savedCharacteristics[i]));
            
            // Initial selection
            if (_savedCharacteristics.Count > 0) InstanciateScrollView(_savedCharacteristics[0]);
        }

        // TODO: quite inefficient to clean/alloc everytime but who care ?
        private void InstanciateScrollView(HostCharacteristics c)
        {
            // Clear all child
            foreach (Transform o in characteristicList.transform)
            {
                Destroy(o.gameObject);
            }
            // TODO: how to handle non-continuous characteristics (boolean, discrete ...) ?
            // for each characteristics
            var fields = c.GetType().GetFields();
            foreach (var field in fields)
            {
                var characteristicGo = Instantiate(characteristicTemplate, characteristicList.transform);
                if (!c.RangeAttributes.ContainsKey(field.Name)) continue;
                var r = c.RangeAttributes[field.Name];
                var s = characteristicGo.GetComponentInChildren<Slider>();
                s.minValue = r.min;
                s.maxValue = r.max;
                var val = field.GetValue(c);
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
