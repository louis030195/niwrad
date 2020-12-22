// using Unity.Burst;
// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
//
// // TODO: whole file to the trash, overlapsphere not supported by job and its
// namespace Utils.Physics
// {
//     [BurstCompile]
//     public struct PositionAboveGroundJob : IJob
//     {
//         public NativeArray<RaycastHit> Results;
//         public NativeArray<RaycastCommand> Commands;
//         
//         public void Execute()
//         {
//             // Schedule the batch of raycasts
//             var handle = RaycastCommand.ScheduleBatch(Commands, Results, 1);
//
//             // Wait for the batch processing job to complete
//             handle.Complete();
//         }
//     }
//
//     public class PhysicsJob : MonoBehaviour // Why not static ? Job + Static data
//     {
//         private LayerMask _mask;
//         private NativeArray<RaycastHit> _results;
//         private NativeArray<RaycastCommand> _commands;
//         private JobHandle _positionAboveGroundHandle;
//         private PositionAboveGroundJob _positionAboveGroundJob;
//         private readonly Collider[] _overlapResults = new Collider[1];
//         public bool isCreated;
//
//         private void Awake()
//         {
//             _mask = LayerMask.GetMask("Ground", "Water");
//             _results = new NativeArray<RaycastHit>(1, Allocator.Persistent);
//             _commands = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
//             isCreated = true;
//         }
//
//         public Vector3 PositionAboveGroundJob(Vector3 p, float prefabHeight = 1f)
//         {
    //             _commands[0] = new RaycastCommand(p, Vector3.down, _mask);
//             _positionAboveGroundJob = new PositionAboveGroundJob
//             {
//                 Results = _results,
//                 Commands = _commands
//             };
//             _positionAboveGroundHandle = _positionAboveGroundJob.Schedule();
//             // Wait for the batch processing job to complete
//             _positionAboveGroundHandle.Complete();
//
//             // Copy the result. If batchedHit.collider is null there was no hit
//             var batchedHit = _results[0];
//             
//             // No hit
//             if (batchedHit.collider == null) return Vector3.positiveInfinity;
//             
//             // Hit water ? Bad
//             if (batchedHit.transform.gameObject.layer.Equals(LayerMask.NameToLayer("Water"))) return Vector3.positiveInfinity;
//             p.y -= batchedHit.distance - prefabHeight * 0.5f;
//             return p;
//         }
//         
//         /// <summary>
//         /// This function will find a position to spawn above ground and far enough from other objects of the given layer
//         /// Returns Vector3 zero in case it couldn't find a free position
//         /// </summary>
//         /// <returns></returns>
//         ///
//         public Vector3 SprayJob(Vector3 p,
//             float areaRadius,
//             LayerMask layerMask,
//             float radiusBetweenObjects,
//             float areaHeight = 1000f,
//             float prefabHeight = 1f,
//             int numberOfTries = 10)
//         {
//             if (radiusBetweenObjects > areaRadius)
//             {
//                 Debug.LogError("Distance must be inferior to radius");
//                 return Vector3.positiveInfinity;
//             }
//             var tries = 0;
//             // While we didn't find a free position
//             while (tries < numberOfTries)
//             {
//                 // We pick a random position around above ground
//                 var newPos = p + Random.insideUnitSphere * areaRadius;
//                 p.y += areaHeight;
//                 newPos = PositionAboveGroundJob(newPos, prefabHeight);
//                 if (newPos.Equals(Vector3.positiveInfinity)) // Outside map
//                 {
//                     tries++;
//                     continue;
//                 }
//                 // Then we check if this spot is free (from the given layer)
//                 var size = UnityEngine.Physics.OverlapSphereNonAlloc(newPos, radiusBetweenObjects, _overlapResults, layerMask);
//                 
//                 // If no objects of the same layer is detected, this spot is free, return
//                 if (size == 0) return newPos;
//
//                 tries++;
//             }
//
//             return Vector3.positiveInfinity;
//         }
//
//         private void OnDestroy()
//         {
//             _results.Dispose();
//             _commands.Dispose();
//         }
//     }
// }
