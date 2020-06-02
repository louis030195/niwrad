using System;
using StateMachine;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Evolution
{
	public class Vegetation : Host
	{

		private Meme m_Grow;
		protected new void OnEnable()
		{
			base.OnEnable();
			m_Grow = new Meme(
				new Action<MemeController>[]
				{
					Grow
				},
				new Func<MemeController, Meme>[]
				{
					CanBreed
				},
				Color.green
			);
			Breed = new Meme(
				new Action<MemeController>[]
				{
					Reproduce
				},
				new Func<MemeController, Meme>[]
				{
					CanBreed
				},
				Color.magenta
			);

			Controller.SetupAi(m_Grow, true, decisionFrequency);
		}

		protected new void Update()
		{
			// Sun gives life (maybe could multiply by "sun intensity here")
			health.ChangeHealth(robustness*Time.deltaTime);
		}


		#region Actions
		private void Grow(MemeController c)
		{

		}
		private void Reproduce(MemeController c)
		{
			// Debug.Log($"Reproduce");
			// Spawning a child around
			// TODO: extension physics to find some close point that keep a distance with other vegetation
			// See Evol code
			var p = transform.position.RandomPositionAroundAboveGroundWithDistance(10f,
				LayerMask.GetMask("Vegetation"),
				1f);
			// Couldn't find free position
			if (p == Vector3.zero) return;
			var go = Generate.instance.SpawnVegetation(p, Quaternion.identity);
			var childHost = go.GetComponent<Vegetation>();
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
				c.currentObservation.ReproductionMode = true;
				return Breed;
			}
			c.currentObservation.ReproductionMode = false;
			return m_Grow;
		}

		#endregion
	}
}
