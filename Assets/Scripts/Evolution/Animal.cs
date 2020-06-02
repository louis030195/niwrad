using System;
using StateMachine;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Evolution
{
	[RequireComponent(typeof(Movement))]
	public class Animal : Host
	{
		[Header("Initial characteristics")]
		[Range(2, 20)]
		public float initialSpeed = 5f;
		[Range(1, 1000)]
		public float randomMovementRange = 20f;
		[Range(1, 1000)]
		public float sightRange = 20f;
		[Range(2f, 10.0f)]
		public float eatRange = 5f;
		[Range(1, 100.0f)]
		[Tooltip("How much life eating bring")]
		public float metabolism = 10f;

		[Header("Reproduction")]
		[Range(20, 80)]
		public float reproductionLifeLoss = 50f;

		[HideInInspector] public Movement movement;

		private Meme m_Wander;
		private Meme m_Reach;
		private Meme m_Eat;

		protected new void OnEnable()
		{
			base.OnEnable();
			movement = GetComponent<Movement>();
			movement.speed = initialSpeed;

			m_Wander = new Meme(
				new Action<MemeController>[]
				{
					RandomMovement
				},
				new Func<MemeController, Meme>[]
				{
					SeeFoodOrPartner
				},
				Color.white
			);
			m_Reach = new Meme(
				new Action<MemeController>[]
				{
					Reach
				},
				new Func<MemeController, Meme>[]
				{
					IsCloseEnough
				},
				Color.red
			);
			m_Eat = new Meme(
				new Action<MemeController>[]
				{
					Reach,
					Eat
				},
				new Func<MemeController, Meme>[]
				{
					IsTargetAlive
				},
				Color.blue
			);
			Breed = new Meme(
				new Action<MemeController>[]
				{
					Reach,
					Reproduce
				},
				new Func<MemeController, Meme>[]
				{
					SeeFoodOrPartner
				},
				Color.magenta
			);

			Controller.SetupAi(m_Wander, true, decisionFrequency);
		}

		#region Actions
		private void Reach(MemeController c)
		{
			if (c.currentObservation.Target == null) return;
			// Debug.Log($"reach {movement.remainingDistance} - {movement.stoppingDistance}");
			// if (movement.remainingDistance <= movement.stoppingDistance && !movement.pathPending)
			// {
				movement.MoveTo(c.currentObservation.Target.transform.position);
			// }
		}

		private void RandomMovement(MemeController _)
		{
			// if (movement.remainingDistance <= movement.stoppingDistance && !movement.pathPending)
			// {
				var p = transform.position + Random.insideUnitSphere * randomMovementRange;
				movement.MoveTo(p.PositionAboveGround());
			// }
		}

		private void Eat(MemeController c)
		{
			// Stop moving
			movement.isStopped = true;
			attack.EatTarget(c.currentObservation.Target);
			health.ChangeHealth(+metabolism);
		}

		private void Reproduce(MemeController c)
		{
			// Stop moving
			movement.isStopped = true;
			health.ChangeHealth(-reproductionLifeLoss);
			c.currentObservation.Target.GetComponent<Health>().ChangeHealth(-reproductionLifeLoss);

			// Spawning a child around
			// var p = (transform.position + Random.insideUnitSphere * 10).AboveGround();
			var go = Generate.instance.SpawnHost(transform.position, Quaternion.identity); //Pool.Spawn(prefab, transform.position, Quaternion.identity);
			var th = c.currentObservation.Target.GetComponent<Animal>();
			var childHost = go.GetComponent<Animal>();
			var mutate = new Func<float, float, float, float>((a, b, mutationDegree) =>
			{
				var md = Mathf.Abs(mutationDegree) > 1 ? 1 : Mathf.Abs(mutationDegree);
				return ((a + b) / 2) * (1 + Random.Range(-md, md));
			});
			var r = ReflectionExtension.GetRange(GetType(), nameof(initialLife));
			childHost.initialLife = Mathf.Clamp(mutate(initialLife, th.initialLife, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(initialSpeed));
			childHost.initialSpeed = Mathf.Clamp(mutate(initialSpeed, th.initialSpeed, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(randomMovementRange));
			childHost.randomMovementRange = Mathf.Clamp(mutate(randomMovementRange, th.randomMovementRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(sightRange));
			childHost.sightRange = Mathf.Clamp(mutate(sightRange, th.sightRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(eatRange));
			childHost.eatRange = Mathf.Clamp(mutate(eatRange, th.eatRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(metabolism));
			childHost.metabolism = Mathf.Clamp(mutate(metabolism, th.metabolism, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(robustness));
			childHost.robustness = Mathf.Clamp(mutate(robustness, th.robustness, 1f), r.min, r.max);

			// go.GetComponent<MeshFilter>().mesh.Mutation();

			// TODO: the new host should have its memes tweaked by meme controller (mutation ...)
			LastBreed = Time.time;
		}
		#endregion

		#region Transitions
		private Meme SeeFoodOrPartner(MemeController c)
		{
			int layerMask;

			// Look for partner
			if (Time.time > LastBreed + reproductionDelay && health.currentHealth > reproductionThreshold)
			{
				layerMask = LayerMask.NameToLayer("Animal");
			}
			else // Look for food
			{
				layerMask = LayerMask.NameToLayer("Vegetation");
			}

			// Any matching object around ? Try to get the closest if any
			var min = transform.position.Closest(sightRange, layerMask);
			if (min == null) return null;
			c.currentObservation.ReproductionMode = layerMask == LayerMask.NameToLayer("Animal");
			c.currentObservation.Target = min;
			return m_Reach;
		}

		private Meme IsCloseEnough(MemeController c)
		{
			return Vector3.Distance(transform.position, c.currentObservation.Target.transform.position) <
			       eatRange ? c.currentObservation.ReproductionMode ? Breed : m_Eat : null;
		}


		private Meme IsTargetAlive(MemeController c)
		{
			// Debug.Log($"Target dead: {c.currentObservation.Target.GetComponent<Health>().currentHealth} " +
			//           $"{c.currentObservation.Target.GetComponent<Health>().dead}" +
			//           $"{c.currentObservation.Target.transform.position}");
			return c.currentObservation.Target.GetComponent<Health>().currentHealth < 0.0001f ? m_Wander : null;
		}

		#endregion
	}
}
