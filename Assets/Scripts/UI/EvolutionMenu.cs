using System.Collections.Generic;
using Api.Realtime;
using Evolution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;

namespace UI
{
    /// <summary>
    /// Given a HostCharacteristics, will spawn an UI to tweak these characteristics
    /// </summary>
    public class EvolutionMenu : Menu
    {

        private const string ScriptableObjectsPath = "ScriptableObjects";
        [SerializeField] private TMP_Dropdown characteristicsDropDown;
        [SerializeField] private GameObject characteristicList;
        [SerializeField] private TMP_InputField saveAsInputField;
        [SerializeField] private Button saveAsButton;
        [SerializeField] private Button deleteButton;
        
        /// <summary>
        /// characteristics should be given as a copy ! We don't necessarily want to save the tweaked characteristics
        /// </summary>
        private List<Characteristics> _savedCharacteristics;
        // private HostCharacteristics _selectedCharacteristics;

        private void Start()
        {
            // Save();
            // deleteButton.onClick.AddListener(Delete);
            // saveAsButton.onClick.AddListener(Save);
            // Initial selection
            if (_savedCharacteristics.Count > 0) InstanciateScrollView(_savedCharacteristics[0]);
        }

        // TODO: quite inefficient to clean/alloc everytime but who care ?
        private void InstanciateScrollView(Characteristics c)
        {
            // Clear all child
            foreach (Transform o in characteristicList.transform)
            {
                Destroy(o.gameObject);
            }
            // c.Render(characteristicList.transform);
        }

        // public void Save()
        // {
        //     if (_savedCharacteristics.Count > 0)
        //     {
        //         // TODO: name validation, check override file etc. TODO: does it works outside editor ? (standalone)
        //         var copy = Instantiate(_savedCharacteristics[characteristicsDropDown.value]);
        //         AssetDatabase.CreateAsset(copy, $"{ScriptableObjectsPath}/{saveAsInputField.text}");
        //         AssetDatabase.SaveAssets();
        //         AssetDatabase.Refresh();
        //
        //         // Reset 
        //         characteristicsDropDown.onValueChanged.RemoveAllListeners();
        //         characteristicsDropDown.options.Clear();
        //     }
        //
        //     // And reload
        //     _savedCharacteristics = Resources.LoadAll(ScriptableObjectsPath, typeof(HostCharacteristics))
        //         .Cast<HostCharacteristics>().ToList();
        //     characteristicsDropDown.AddOptions(_savedCharacteristics.Select(c => c.name).ToList());
        //     characteristicsDropDown.onValueChanged.AddListener(i => InstanciateScrollView(_savedCharacteristics[i]));
        // }
        //
        // public void Delete()
        // {
        //     var asset = _savedCharacteristics[characteristicsDropDown.value].name;
        //     if (asset.Contains("BasicAnimalCharacteristics") || asset.Contains("BasicVegetationCharacteristics"))
        //     {
        //         Debug.LogWarning("It's forbidden to delete base assets"); // TODO: UI stuff output
        //         return;
        //     }
        //     AssetDatabase.DeleteAsset($"{ScriptableObjectsPath}/{asset}");
        // }
    }
}
