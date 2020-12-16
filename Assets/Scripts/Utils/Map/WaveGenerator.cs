using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Utils.Map
{
    public class WaveGenerator : MonoBehaviour
    {
        public MeshFilter waterMeshFilter;
        
        [Header("Wave Parameters")]
        public float waveScale;
        public float waveOffsetSpeed;
        public float waveHeight;

        private Mesh _waterMesh;
        private NativeArray<Vector3> _waterVertices;
        private NativeArray<Vector3> _waterNormals;
        private JobHandle _meshModificationJobHandle;
        private UpdateMeshJob _meshModificationJob;

        private void Start()
        {
            _waterMesh = waterMeshFilter.mesh;
            _waterMesh.MarkDynamic(); // GPU
            _waterVertices = new NativeArray<Vector3>(_waterMesh.vertices, Allocator.Persistent);
            _waterNormals = new NativeArray<Vector3>(_waterMesh.normals, Allocator.Persistent);
        }

        private void Update()
        {
            _meshModificationJob = new UpdateMeshJob()
            {
                Vertices = _waterVertices,
                Normals = _waterNormals,
                OffsetSpeed = waveOffsetSpeed,
                Time = Time.time,
                Scale = waveScale,
                Height = waveHeight
            };

            _meshModificationJobHandle = _meshModificationJob.Schedule(_waterVertices.Length, 64);
        }

        private void LateUpdate()
        {
            _meshModificationJobHandle.Complete();
            _waterMesh.SetVertices(_meshModificationJob.Vertices);
            _waterMesh.RecalculateNormals();
        }

        private void OnDestroy()
        {
            _waterVertices.Dispose();
            _waterNormals.Dispose();
        }

        private struct UpdateMeshJob : IJobParallelFor
        {
            public NativeArray<Vector3> Vertices;
            public NativeArray<Vector3> Normals;
            
            [ReadOnly]
            public float OffsetSpeed;
            [ReadOnly]
            public float Scale;
            [ReadOnly]
            public float Height;
            [ReadOnly]
            public float Time;

            private float Noise(float x, float y)
            {
                float2 pos = math.float2(x, y);
                return noise.snoise(pos);
            }

            public void Execute(int i)
            {
                // Only executing on facing vertices
                if (!(Normals[i].z > 0f)) return;
                var vertex = Vertices[i]; 
                float noiseValue = Noise(vertex.x * Scale + OffsetSpeed * Time, vertex.y * Scale + 
                    OffsetSpeed * Time); 
                Vertices[i] = new Vector3(vertex.x , vertex.y, noiseValue * Height + 0.3f);

            }
        }
    }
}
