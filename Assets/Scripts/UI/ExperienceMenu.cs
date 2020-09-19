using Evolution;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ExperienceMenu : Menu
    {
        [SerializeField] private Transform animalCharacteristicMenu;
        [SerializeField] private Transform vegetationCharacteristicMenu;
        private void Start()
        {
            var e = ExperienceExtensions.Load($"Assets/Scripts/Tests/Data/BasicExperience.json", true);
            CodeToUi.NumberToUi(0, 
                200, 
                e.AnimalDistribution.InitialAmount, 
                animalCharacteristicMenu,
                "Initial Amount");
            CodeToUi.NumberToUi(0, 
                100, 
                e.AnimalDistribution.Scattering, 
                animalCharacteristicMenu,
                "Scattering");
            e.IncludeCarnivorous.BooleanToUI(animalCharacteristicMenu, "Include Carnivorous");
            CodeToUi.FloatsToUi(e.AnimalCharacteristicsMinimumBound, 
                e.AnimalCharacteristicsMaximumBound,
                e.AnimalCharacteristics, 
                animalCharacteristicMenu);
            CodeToUi.FloatsToUi(e.VegetationCharacteristicsMinimumBound, 
                e.VegetationCharacteristicsMaximumBound,
                e.VegetationCharacteristics, 
                vegetationCharacteristicMenu);
        }
    }
}
