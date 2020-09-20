using System;
using Api.Realtime;
using Evolution;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;

namespace UI
{
    public class ExperienceMenu : Menu
    {
        [SerializeField] private Transform animalCharacteristicMenu;
        [SerializeField] private Transform vegetationCharacteristicMenu;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button playButton;
        [SerializeField] private TMP_InputField experienceNameInputField;
        private Experience _experience;


        /// <summary>
        /// Triggered when saved as a new experience
        /// </summary>
        public event Action Added;

        /// <summary>
        /// Triggered when deleting an experience
        /// </summary>
        public event Action Deleted;
        
        protected override void Start()
        {
            base.Start();
            saveButton.onClick.AddListener(Save);
            deleteButton.onClick.AddListener(Delete);
            playButton.onClick.AddListener(Play);
            experienceNameInputField.onEndEdit.AddListener(value => _experience.Name = value);
            // TODO: name validation ? will fuck up if user name it something like /root/destroy_the_universe.exe
        }

        public void Load(Experience e)
        {
            _experience = e;
            experienceNameInputField.text = e.Name;
            CodeToUi.NumberToUi(0, 
                200, 
                e.AnimalDistribution?.InitialAmount ?? 0, 
                animalCharacteristicMenu,
                "Initial Amount")
                .onValueChanged.AddListener(value =>  _experience.AnimalDistribution.InitialAmount = (ulong) value);
            CodeToUi.NumberToUi(0, 
                100, 
                e.AnimalDistribution?.Scattering ?? 0, 
                animalCharacteristicMenu,
                "Scattering")
                .onValueChanged.AddListener(value =>  _experience.AnimalDistribution.Scattering = value);
            e.IncludeCarnivorous.BooleanToUI(animalCharacteristicMenu, "Include Carnivorous")
                .onValueChanged.AddListener(value =>  _experience.IncludeCarnivorous = value);
            CodeToUi.FloatsToUi(e.AnimalCharacteristicsMinimumBound, 
                e.AnimalCharacteristicsMaximumBound,
                e.AnimalCharacteristics, 
                animalCharacteristicMenu)
                    .ForEach(elem =>
                    {
                        var (p, s) = elem;
                        s.onValueChanged.AddListener(value => 
                            _experience.GetType().GetProperty(p.Name)?.SetValue(_experience, value));
                    });
            
            CodeToUi.NumberToUi(0, 
                200, 
                e.VegetationDistribution?.InitialAmount ?? 0, 
                vegetationCharacteristicMenu,
                "Initial Amount")
                .onValueChanged.AddListener(value =>  _experience.VegetationDistribution.InitialAmount = (ulong) value);
            CodeToUi.NumberToUi(0, 
                100, 
                e.VegetationDistribution?.Scattering ?? 0, 
                vegetationCharacteristicMenu,
                "Scattering")
                .onValueChanged.AddListener(value =>  _experience.VegetationDistribution.Scattering = value);
            CodeToUi.FloatsToUi(e.VegetationCharacteristicsMinimumBound, 
                e.VegetationCharacteristicsMaximumBound,
                e.VegetationCharacteristics, 
                vegetationCharacteristicMenu)
                    .ForEach(elem =>
                    {
                        var (p, s) = elem;
                        s.onValueChanged.AddListener(value => 
                            _experience.GetType().GetProperty(p.Name)?.SetValue(_experience, value));
                    });
        }

        private void Save()
        {
            // If it's a new experience, notify it
            if (_experience.Save()) Added?.Invoke(); // TODO: animation
        }

        private void Delete()
        {
            _experience.Delete(); // TODO: animation
            Deleted?.Invoke();
        }

        public void Play()
        {
            Mm.instance.PopAll();
            Gm.instance.StartExperience(_experience);
        }
        
    }
}
