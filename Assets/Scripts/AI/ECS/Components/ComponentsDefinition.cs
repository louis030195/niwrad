using Unity.Entities;
using UnityEngine;

namespace AI.ECS.Components
{
    internal enum ActionType
    {
        Null,
        Eat,
        Sleep,
        Play,
    }

    internal struct EatAction : IComponentData 
    {
        public float HungerRecoverPerSecond;
        public float TirednessCostPerSecond;// eat still get tired, but should slower than play
    }

    internal struct SleepAction : IComponentData 
    {
        public float TirednessRecoverPerSecond;
        public float HungerCostPerSecond;// sleep still get hungry slowly
    }

    internal struct PlayAction : IComponentData 
    {
        // the hunger cost and tiredness cost of play are faster than eat and sleep
        public float HungerCostPerSecond;
        public float TirednessCostPerSecond;
    }

    internal struct EatScorer : IComponentData { public float Score; }

    internal struct SleepScorer : IComponentData { public float Score; }

    internal struct PlayScorer : IComponentData { public float Score; }

    internal struct Host : IComponentData
    {
        // This component have no fields cause we just want it to be a flag
    }

    internal struct Hungriness : IComponentData
    {
        public float Value; // 0: not hungry, 100: hungry to death
    }

    internal struct Tiredness : IComponentData
    {
        public float Value; // 0: not tired, 100: tired to death
    }

    internal struct Decision : IComponentData
    {
        public ActionType Action; // current action to perform
    }
}
