using System.ComponentModel;
using AI.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AI.Systems
{
	// https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/ECSSamples/Assets/HelloCube/2.%20IJobChunk/RotationSpeedSystem_IJobChunk.cs
	public class MemeSystem : SystemBase
	{
		private EntityQuery m_Group;

		protected override void OnCreate()
		{
			m_Group = GetEntityQuery(typeof(MemeComponent), ComponentType.ReadOnly<MemeSystem>());
		}

		[BurstCompile]
		private struct StateJob : IJobChunk // Best name ? ActionTransitionJob ? ...
		{
			public float DeltaTime;
			public ArchetypeChunkComponentType<MemeComponent> MemeType;

			public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
			{
				var chunkRotations = chunk.GetNativeArray(MemeType);
				for (var i = 0; i < chunk.Count; i++)
				{
					// Do actions
					// Check transitions
				}
			}
		}

		protected override void OnUpdate()
		{
			var memeType = GetArchetypeChunkComponentType<MemeComponent>();

			var job = new StateJob()
			{
				MemeType = memeType,
				DeltaTime = Time.DeltaTime
			};

			Dependency = job.Schedule(m_Group, Dependency);
		}
	}
}
