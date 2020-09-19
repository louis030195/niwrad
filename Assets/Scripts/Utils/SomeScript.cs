using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Evolution;
using UnityEngine;

namespace Utils
{
    public class SomeScript : MonoBehaviour
    {
        [SerializeField] private Transform animalCharacteristicMenu;
        [SerializeField] private Transform vegetationCharacteristicMenu;
        private void Start()
        {
            var e = ExperienceExtensions.Load($"Assets/Scripts/Tests/Data/BasicExperience.json", true);

        }
    }
}
