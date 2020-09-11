using UnityEngine;

namespace Evolution
{
    // TODO: decides which parameters / characteristic is destined to individual level or species level
    enum ReproductionType {
        Asexual,
        Allogamy,
        Autogamy,
    }

    enum ReproductionCycle
    {
        Polycyclic,
        Semelparous,
        Iteroparous
    }
    [CreateAssetMenu(fileName = "Experience", menuName = "ScriptableObjects/Experience", order = 2)]
    public class Experience : ScriptableObject
    {
        // TODO: can we factorize some common parameters between vegetation & animals (struct ..)
        [Header("Evolution Characteristics")] 
        public AnimalCharacteristics animalCharacteristics;
        public VegetationCharacteristics vegetationCharacteristics;

        [Header("Parameters")] 
        [Header("Animals")]
        [Range(0, 10000), Tooltip("Number of initial animals to spawn")] public int initialAnimal;
        [Range(1, 100), Tooltip("Distance between spawns")] public int sprayAnimal;
        [Tooltip("Whether or not to include carnivorism in animals")] public bool includeCarnivorous;
        [Header("Vegetation")]
        [Range(0, 10000), Tooltip("Number of initial vegetations to spawn")] public int initialVegetation;
        [Range(1, 100), Tooltip("Distance between spawns")] public int sprayVegetation;
        // [Tooltip("Whether or not to include carnivorism in vegetation")] public bool includeCarnivorous;

        [Header("Map")] public int todo;
        public bool water;
        [Range(0, 10), Tooltip("E.g only desert ? or try to maximize diversity")] public int diversity;
        [Header("General")] 
        // TODO: graphic level
        [Range(1, 50)] public int initialTimescale;
        [Range(0, 10)] public int timeLimit;
        public bool repeat;
        [Tooltip("Save statistics ? TODO: how ? csv ?")] public bool save;
    }
}
