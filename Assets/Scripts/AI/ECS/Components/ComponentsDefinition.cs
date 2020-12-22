using Unity.Entities;
using UnityEngine;

namespace AI.ECS.Components
{
    internal enum ActionType
    {
        Null,
        Eat,
        // Drink,
        Sleep,
        Wander,
        // LookForFood,
        // LookForBeverage,
        // LookForMate,
        // Mate,
        // Leisure
    }

    internal struct EatAction : IComponentData
    {
        public float HungerRecoverPerSecond;
        public float TirednessCostPerSecond; // eat still get tired, but should slower than play
    }
    
    internal struct LookForFoodAction : IComponentData
    {
        
    }

    internal struct SleepAction : IComponentData
    {
        public float TirednessRecoverPerSecond;
        public float HungerCostPerSecond; // sleep still get hungry slowly
    }

    internal struct WanderAction : IComponentData
    {
        // the hunger cost and tiredness cost of play are faster than eat and sleep
        public float HungerCostPerSecond;
        public float TirednessCostPerSecond;
    }

    // InternalBufferCapacity specifies how many elements a buffer can have before
    // the buffer storage is moved outside the chunk.
    [InternalBufferCapacity(3)]
    public struct ActionValue : IBufferElementData
    {
        // Actual value each buffer element will store.
        public float Value;

        // The following implicit conversions are optional, but can be convenient.
        public static implicit operator float(ActionValue e)
        {
            return e.Value;
        }

        public static implicit operator ActionValue(float e)
        {
            return new ActionValue { Value = e };
        }
    }


    internal struct Host : IComponentData
    {
        // This component have no fields cause we just want it to be a flag
    }

    internal struct FoodTarget : IComponentData
    {
        public Entity Target;
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
