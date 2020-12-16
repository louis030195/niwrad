using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using random = Unity.Mathematics.Random;

namespace Utils.Map
{
    public class FishGenerator : MonoBehaviour
    {
        [Header("References")]
        public Transform waterObject;
        public Transform objectPrefab;

        [Header("Spawn Settings")]
        public int amountOfFish;
        public Vector3 spawnBounds;
        public float spawnHeight;
        public int swimChangeFrequency;

        [Header("Settings")]
        public float swimSpeed;
        public float turnSpeed;

        private PositionUpdateJob _positionUpdateJob;
        private JobHandle _positionUpdateJobHandle;

        private NativeArray<Vector3> _velocities;
        private TransformAccessArray _transformAccessArray;

        private void Start()
        {
            _velocities = new NativeArray<Vector3>(amountOfFish, Allocator.Persistent);
            _transformAccessArray = new TransformAccessArray(amountOfFish);

            for (int i = 0; i < amountOfFish; i++)
            {
                float distanceX = Random.Range(-spawnBounds.x / 2, spawnBounds.x / 2);
                float distanceZ = Random.Range(-spawnBounds.z / 2, spawnBounds.z / 2);

                //Spawn off the ground at a height and in a random X and Z position without affecting height
                Vector3 spawnPoint = (transform.position + Vector3.up * spawnHeight) + new Vector3(distanceX, 0, distanceZ);

                //Creating transform and transform access at a spawn point
                Transform t = Instantiate(objectPrefab, spawnPoint, Quaternion.identity);
                _transformAccessArray.Add(t);
            }
        }

        private void Update()
        {
            //Setting parameters of position update
            _positionUpdateJob = new PositionUpdateJob()
            {
                ObjectVelocities = _velocities,
                JobDeltaTime = Time.deltaTime,
                SwimSpeed = swimSpeed,
                TurnSpeed = turnSpeed,
                Time = Time.time,
                SwimChangeFrequency = swimChangeFrequency,
                Center = waterObject.position,
                Bounds = spawnBounds,
                Seed = System.DateTimeOffset.Now.Millisecond
            };

            _positionUpdateJobHandle = _positionUpdateJob.Schedule(_transformAccessArray);
        }

        private void LateUpdate()
        {
            _positionUpdateJobHandle.Complete();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position + Vector3.up * spawnHeight, spawnBounds);
        }

        [BurstCompile]
        struct PositionUpdateJob : IJobParallelForTransform
        {
            public NativeArray<Vector3> ObjectVelocities;

            public Vector3 Bounds;
            public Vector3 Center;

            public float JobDeltaTime;
            public float Time;
            public float SwimSpeed;
            public float TurnSpeed;

            public float Seed;

            public int SwimChangeFrequency;

            public void Execute(int i, TransformAccess transform)
            {
                Vector3 currentVelocity = ObjectVelocities[i];
                random randomGen = new random((uint)(i * Time + 1 + Seed));

                transform.position += transform.localToWorldMatrix.MultiplyVector(new Vector3(0, 0, 1)) * SwimSpeed * JobDeltaTime * randomGen.NextFloat(0.3f, 1.0f);

                if (currentVelocity != Vector3.zero)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(currentVelocity), TurnSpeed * JobDeltaTime);
                }

                Vector3 currentPosition = transform.position;

                bool randomise = true;
                if (currentPosition.x > Center.x + Bounds.x / 2 || currentPosition.x < Center.x - Bounds.x/2 || currentPosition.z > Center.z + Bounds.z / 2 || currentPosition.z < Center.z - Bounds.z / 2)
                {
                    Vector3 internalPosition = new Vector3(Center.x + randomGen.NextFloat(-Bounds.x / 2, Bounds.x / 2)/1.3f, 0, Center.z + randomGen.NextFloat(-Bounds.z / 2, Bounds.z / 2)/1.3f);
                    currentVelocity = (internalPosition- currentPosition).normalized;
                    ObjectVelocities[i] = currentVelocity;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(currentVelocity), TurnSpeed * JobDeltaTime * 2);
                    randomise = false;
                }

                if (randomise)
                {
                    if (randomGen.NextInt(0, SwimChangeFrequency) <= 2)
                    {
                        ObjectVelocities[i] = new Vector3(randomGen.NextFloat(-1f, 1f), 0, randomGen.NextFloat(-1f, 1f));
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _transformAccessArray.Dispose();
            _velocities.Dispose();
        }
    }
}
