using System;
using System.Collections.Generic;
using AI;
using Gameplay;
using ProceduralTree;
using UnityEngine;
using Utils;
using Action = AI.Action;
using Random = UnityEngine.Random;

namespace Evolution
{
	public class Vegetation : Host
	{
		[Header("Reproduction"), Range(5, 1000)]
		public float reproductionSprayRadius = 100f;
		[Range(2, 100)]
		public float reproductionDistanceBetween= 5f;
		[Range(1, 100)]
		public float reproductionProbability = 10f;

		protected new void OnEnable()
		{
			base.OnEnable();
			var n = "Grow";
			memes[n] = new Meme(
				n,
				new List<Action>
				{
					new Action("Grow", Grow)
				},
				new List<Transition>
				{
					new Transition("CanBreed", 0, CanBreed)
				},
				Color.green
			);
			n = "Breed";
			memes[n] = new Meme(
				n,
				new List<Action>
				{
					new Action("Reproduce", Reproduce)
				},
				new List<Transition>
				{
					new Transition("CanBreed", 0, CanBreed)
				},
				Color.magenta
			);

		}

		protected new void Update()
		{
			// Sun gives life (maybe could multiply by "sun intensity here")
			health.ChangeHealth(robustness*Time.deltaTime*10);
			health.dead = !(health.currentHealth > initialLife); // :)
		}

		public override void BringToLife()
		{
			controller.SetupAi(memes["Grow"], true, decisionFrequency);
		}

		#region Actions
		private void Grow(MemeController c)
		{

		}
		private void Reproduce(MemeController c)
		{
			// There is a probability of reproduction
			if (Random.value * 100 > reproductionProbability) return;

			// Spawning a child around
			var p = transform.position.RandomPositionAroundAboveGroundWithDistance(reproductionSprayRadius,
				LayerMask.GetMask("Vegetation"),
				reproductionDistanceBetween);
			// Couldn't find free position
			if (p == Vector3.zero) return;
			var childHost = HostManager.instance.SpawnTree(p, Quaternion.identity);
			var mutate = new Func<float, float, float>((a, mutationDegree) =>
			{
				var md = Mathf.Abs(mutationDegree) > 1 ? 1 : Mathf.Abs(mutationDegree);
				return a * (1 + Random.Range(-md, md));
			});
			var r = ReflectionExtension.GetRange(GetType(), nameof(initialLife));
			childHost.initialLife = Mathf.Clamp(mutate(initialLife, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(robustness));
			childHost.robustness = Mathf.Clamp(mutate(robustness, 1f), r.min, r.max);

			// go.GetComponent<MeshFilter>().mesh.Mutation();

			// TODO: the new host should have its memes tweaked by meme controller (mutation ...)
			LastBreed = Time.time;
		}
		#endregion

		#region Transitions

		private Meme CanBreed(MemeController c)
		{
			if (Time.time > LastBreed + reproductionDelay &&
			    health.currentHealth > reproductionThreshold)
			{
				return memes["Breed"];
			}
			return memes["Grow"];
		}

		#endregion
	}
}
