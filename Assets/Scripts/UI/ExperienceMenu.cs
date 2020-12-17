using System;
using System.IO;
using System.Linq;
using Api.Realtime;
using Api.Session;
using Cysharp.Threading.Tasks;
using Evolution;
using Gameplay;
using Lean.Gui;
using TMPro;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace UI
{
    public class ExperienceMenu : Menu
    {
        [SerializeField] private Transform generalMenu;
        [SerializeField] private Transform mapMenu;
        [SerializeField] private Transform animalCharacteristicMenu;
        [SerializeField] private Transform plantCharacteristicMenu;
        [SerializeField] private LeanButton saveButton;
        [SerializeField] private LeanButton deleteButton;
        [SerializeField] private LeanButton shareButton;
        [SerializeField] private LeanButton playButton;
        [SerializeField] private TMP_InputField experienceNameInputField;
        [SerializeField] private TextMeshProUGUI experienceOwnerText;
        private Experience _experience;
        [HideInInspector] public string owner;

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
            saveButton.OnClick.AddListener(Save);
            deleteButton.OnClick.AddListener(Delete);
            playButton.OnClick.AddListener(Play); // TODO: show TOASTS
            shareButton.OnClick.AddListener(Share);
            experienceNameInputField.onEndEdit.AddListener(value => _experience.Name = value);
        }

        public void Load(Experience e, string givenOwner = "")
        {
            _experience = e;
            experienceNameInputField.text = e.Name;
            owner = givenOwner;
            experienceOwnerText.text = owner == "" ? "" : $"<color=blue>Created by</color> {owner}";
            e.IncludeCarnivorous.BooleanToUI(generalMenu, "Include Carnivorous")
                .onValueChanged.AddListener(value => _experience.IncludeCarnivorous = value);
            CodeToUi.NumberToUi(0,
                    100,
                    e.CarnivorousPercent,
                    generalMenu,
                    "Carnivorous Percent")
                .onValueChanged.AddListener(value => _experience.CarnivorousPercent = (int) value);
            CodeToUi.NumberToUi(1,
                    50,
                    e.Timescale == 0 ? 1 : e.Timescale,
                    generalMenu,
                    "Timescale")
                .onValueChanged.AddListener(value => _experience.Timescale = (uint) value);
            Hm.instance.maxHostsUntilPause = 1000; // Base value (this is not in experience proto params)
            var sp = CodeToUi.NumberToUi(500,
                5000,
                1000,
                generalMenu,
                "Automatic Pause");
            sp.onValueChanged.AddListener(value => Hm.instance.maxHostsUntilPause = (uint) value);

            // Hosts
            var charTooltips = new[]
            {
                "Influences the <color=red>decision frequency</color>, reaction time",
                "",
                "Resistance to life",
                "Used for all actions, <color=red>influences life</color>",
                "How energy costly it is",
                "How much energy eating brings",
                "How much energy drinking brings",
                "Carnivorous host ?",
                "Hard-delay between reproductions",
                ""
            };

            // Animals
            CodeToUi.NumberToUi(0,
                    500,
                    e.AnimalDistribution?.InitialAmount ?? 0,
                    animalCharacteristicMenu,
                    "Initial Amount")
                .onValueChanged.AddListener(value => _experience.AnimalDistribution.InitialAmount = (ulong) value);
            CodeToUi.NumberToUi(0,
                    100,
                    e.AnimalDistribution?.Scattering ?? 0,
                    animalCharacteristicMenu,
                    "Spawn Scattering")
                .onValueChanged.AddListener(value => _experience.AnimalDistribution.Scattering = value);
            CodeToUi.NumberToUi(10,
                    150,
                    e.AnimalDistribution?.Radius ?? 0,
                    animalCharacteristicMenu,
                    "Spawn Radius",
                    tooltip: "Radius within which it tries to spawn animals")
                .onValueChanged.AddListener(value => _experience.AnimalDistribution.Radius = value);
            CodeToUi.FloatsToUi(e.AnimalCharacteristicsMinimumBound,
                    e.AnimalCharacteristicsMaximumBound,
                    e.AnimalCharacteristics,
                    animalCharacteristicMenu,
                    tooltips: charTooltips)
                .ForEach(elem =>
                {
                    var (p, s) = elem;
                    s.onValueChanged.AddListener(value =>
                        _experience.AnimalCharacteristics
                            .GetType()
                            .GetProperty(p.Name)
                            ?.SetValue(_experience.AnimalCharacteristics, value));
                });

            // Plants
            CodeToUi.NumberToUi(0,
                    500,
                    e.PlantDistribution?.InitialAmount ?? 0,
                    plantCharacteristicMenu,
                    "Initial Amount")
                .onValueChanged.AddListener(value => _experience.PlantDistribution.InitialAmount = (ulong) value);
            CodeToUi.NumberToUi(0,
                    100,
                    e.PlantDistribution?.Scattering ?? 0,
                    plantCharacteristicMenu,
                    "Spawn Scattering")
                .onValueChanged.AddListener(value => _experience.PlantDistribution.Scattering = value);
            CodeToUi.NumberToUi(10,
                    150,
                    e.PlantDistribution?.Radius ?? 0,
                    plantCharacteristicMenu,
                    "Spawn Radius",
                    tooltip: "Radius within which it tries to spawn plants")
                .onValueChanged.AddListener(value => _experience.PlantDistribution.Radius = value);
            CodeToUi.FloatsToUi(e.PlantCharacteristicsMinimumBound,
                    e.PlantCharacteristicsMaximumBound,
                    e.PlantCharacteristics,
                    plantCharacteristicMenu,
                    tooltips: charTooltips)
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
            try
            {
                // If it's a new experience, notify it
                if (_experience.Save()) Added?.Invoke(); // TODO: animation
                NiwradMenu.instance.ShowNotification($"Experience {_experience.Name} has been saved !");
            }
            catch (DirectoryNotFoundException)
            {
                NiwradMenu.instance.ShowNotification($"{_experience.Name} is not a valid name," +
                                                     $" avoid using special characters please");
            }
        }

        private void Delete()
        {
            try
            {
                _experience.Delete(); // TODO: animation
                Deleted?.Invoke();
                NiwradMenu.instance.ShowNotification($"Experience {_experience.Name} has been deleted !");
            }
            catch (DirectoryNotFoundException)
            {
                NiwradMenu.instance.ShowNotification($"Failed to delete {_experience.Name} !");
            }
        }

        private async void Play()
        {
            NiwradMenu.instance.ShowNotification($"Loading experience {_experience.Name} ...");
            NiwradMenu.instance.PopAll();
            Gm.instance.StartExperience(_experience);
        }

        private async void Share()
        {
            // TODO: name validation ?
            // TODO: limit number of shares, propose to delete some idk ..
            var res = await Sm.instance.ShareExperience(_experience);
            var message = res.Acks.LongCount() == 0
                ? $"Failed to share experience {_experience.Name}"
                : $"Experience {_experience.Name} has been shared to the community !";
            NiwradMenu.instance.ShowNotification(message);
        }
    }
}
