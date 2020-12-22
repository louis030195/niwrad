// TODO
// using AI.ECS.Components;
// using Unity.Entities;
// using Unity.Mathematics;
//
// namespace AI.ECS.Systems.ActionGroup
// {
//     [DisableAutoCreation]
//     [UpdateInGroup(typeof(ActionSystemGroup))]
//     public class DrinkActionSystem : SystemBase
//     {
//         protected override void OnUpdate()
//         {
//             float deltaTime = Time.DeltaTime;
//
//             Entities.ForEach((ref Hungriness hunger,
//                 ref Tiredness tired,
//                 in EatAction eatAct) =>
//             {
//                 // recover hungriness
//                 hunger.Value = math.clamp(
//                     hunger.Value - eatAct.HungerRecoverPerSecond * deltaTime, 0f, 100f);
//
//                 // eat still get tired, but should slower than play
//                 tired.Value = math.clamp(
//                     tired.Value + eatAct.TirednessCostPerSecond * deltaTime, 0f, 100f);
//             }).ScheduleParallel();
//         }
//     }
// }
