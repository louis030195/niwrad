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
        [SerializeField] private Transform generalMenu;
        [SerializeField] private Transform mapMenu;
        [SerializeField] private Transform animalCharacteristicMenu;
        [SerializeField] private Transform plantCharacteristicMenu;
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
            // Map
            CodeToUi.NumberToUi(1, 
                    9, 
                    e.Map?.Size ?? 5, 
                    mapMenu,
                    "Size")// TODO: animation while generating map !
                .onValueChanged.AddListener(value =>  _experience.Map.Size = value);
            CodeToUi.NumberToUi(10, 
                    100, 
                    e.Map?.Height ?? 10, 
                    mapMenu,
                    "Height")
                .onValueChanged.AddListener(value =>  _experience.Map.Height = value);
            CodeToUi.NumberToUi(0, 
                    10, 
                    e.Map?.Spread ?? 0, 
                    mapMenu,
                    "Spread")
                .onValueChanged.AddListener(value =>  _experience.Map.Spread = value);
            CodeToUi.NumberToUi(1, 
                    10, 
                    e.Map?.SpreadReductionRate ?? 1, 
                    mapMenu,
                    "Reduction rate")
                .onValueChanged.AddListener(value =>  _experience.Map.SpreadReductionRate = value);
            
            // General
            CodeToUi.NumberToUi(1, 
                    50, 
                    e.General?.Timescale ?? 1, 
                    generalMenu,
                    "Timescale")
                .onValueChanged.AddListener(value =>  _experience.General.Timescale = (uint) value);

            // Hosts
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
                            _experience.AnimalCharacteristics
                                .GetType()
                                .GetProperty(p.Name)
                                ?.SetValue(_experience.AnimalCharacteristics, value));
                    });
            
            CodeToUi.NumberToUi(0, 
                200, 
                e.PlantDistribution?.InitialAmount ?? 0, 
                plantCharacteristicMenu,
                "Initial Amount")
                .onValueChanged.AddListener(value =>  _experience.PlantDistribution.InitialAmount = (ulong) value);
            CodeToUi.NumberToUi(0, 
                100, 
                e.PlantDistribution?.Scattering ?? 0, 
                plantCharacteristicMenu,
                "Scattering")
                .onValueChanged.AddListener(value =>  _experience.PlantDistribution.Scattering = value);
            CodeToUi.FloatsToUi(e.PlantCharacteristicsMinimumBound, 
                e.PlantCharacteristicsMaximumBound,
                e.PlantCharacteristics, 
                plantCharacteristicMenu)
                    .ForEach(elem =>
                    {
                        var (p, s) = elem;
                        s.onValueChanged.AddListener(value => 
                            _experience.PlantCharacteristics
                                .GetType()
                                .GetProperty(p.Name)
                                ?.SetValue(_experience.PlantCharacteristics, value));
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
