using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AI.ECS.Components
{
    internal enum ActionType
    {
        Null,
        Eat,
        Sleep,
        Wander,
        LookForFood,
        Reach,
        LookForMate,
        Mate,
        // Drink,
        // LookForBeverage,
        // Leisure
    }

    internal struct EatAction : IComponentData { }
    internal struct LookForFoodAction : IComponentData { }
    internal struct LookForMateAction : IComponentData { }
    internal struct MateAction : IComponentData { }
    internal struct ReachAction : IComponentData { }
    internal struct SleepAction : IComponentData { }
    internal struct WanderAction : IComponentData { }
    
    /// <summary>
    /// How much every actions impact every characteristics ? (i.e. eating +0.8 satiation while -0.2 energy,
    /// sleeping +0.8 energy while -0.2 satiation ...)
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct CharacteristicChanges : IBufferElementData
    {
        public FixedListFloat32 value;
        public static implicit operator FixedListFloat32(CharacteristicChanges e) => e.value;
        public static implicit operator CharacteristicChanges(FixedListFloat32 e) => new CharacteristicChanges { value = e };
    }
    
    /// <summary>
    /// How good is it to do this action ? (i.e. looking for food now = 0.8, sleeping = 0.1 ...)
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct ActionValue : IBufferElementData
    {
        public float value;
        public static implicit operator float(ActionValue e) => e.value;
        public static implicit operator ActionValue(float e) => new ActionValue { value = e };
    }


    internal struct Herbivorous : IComponentData { }
    internal struct Carnivorous : IComponentData { }
    internal struct Animal : IComponentData { }
    internal struct Plant : IComponentData { }
    internal struct Target : IComponentData { public Entity target; }
    
    
    // Keep in the normalization 0-100 and order low = bad, high = good
    internal enum CharacteristicType
    {
        Satiation,
        Hydration,
        Youth,
        Energy
    }
    
    /// <summary>
    /// Characteristics values (i.e. this cow has 0.4 life, 0.8 energy ...)
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct CharacteristicValue : IBufferElementData
    {
        public float value;
        public static implicit operator float(CharacteristicValue e) => e.value;
        public static implicit operator CharacteristicValue(float e) => new CharacteristicValue { value = e };
    }
    internal struct Decision : IComponentData { public ActionType action; }
}
