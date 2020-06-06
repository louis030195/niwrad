using System;
using System.Collections.Generic;
using StateMachine;
using UnityEngine;
using Utils;
using Action = StateMachine.Action;
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

			var seeFood = new Transition("SeeFood", 0, SeeFood);
			var seePartner = new Transition("SeePartner", -1, SeePartner);
			var timeout = new Transition("Timeout", 1, Timeout);
			m_Wander = new Meme(
				"Wander",
				new List<Action>
				{
					new Action("RandomMovement", RandomMovement)
				},
				new List<Transition>
				{
					seeFood,
					seePartner

				},
				Color.white
			);
			m_Reach = new Meme(
				"Reach",
				new List<Action>
				{
					new Action("Reach", Reach)
				},
				new List<Transition>
				{
					new Transition("IsCloseEnoughForEating", 0, IsCloseEnoughForEating),
					new Transition("IsCloseEnoughForBreeding", -1, IsCloseEnoughForBreeding)
				},
				Color.red
			);
			m_Eat = new Meme(
				"Eat",
				new List<Action>
				{
					// Reach,
					new Action("Eat", Eat)
				},
				new List<Transition>
				{
					new Transition("IsTargetAlive", 0, IsTargetAlive)
				},
				Color.blue
			);
			Breed = new Meme(
				"Breed",
				new List<Action>
				{
					new Action("Reproduce", Reproduce)
					// Reach,
				},
				new List<Transition>
				{
					seeFood,
					seePartner
				},
				Color.magenta
			);

			Controller.SetupAi(m_Wander, true, decisionFrequency);
		}

		#region Actions
		private void Reach(MemeController c)
		{
			// Debug.Log($"reach {movement.remainingDistance} - {movement.stoppingDistance}");
			if (movement.remainingDistance <= movement.stoppingDistance + 1)
			{
				movement.MoveTo(c.currentObservation.Target.transform.position);
			}
		}

		private void RandomMovement(MemeController _)
		{
			if (movement.remainingDistance <= movement.stoppingDistance + 1)
			{
				// Try to find a random position on map, otherwise will just go to zero
				var p = transform.position.RandomPositionAroundAboveGroundWithDistance(randomMovementRange,
					default,
					0);
				movement.MoveTo(p);
			}
		}

		private void Eat(MemeController c)
		{
			// Stop moving
			movement.isStopped = true;
			attack.EatTarget(c.currentObservation.Target);
			c.currentObservation.Target.GetComponent<Health>().ChangeHealth(-Time.deltaTime*10); // TODO: store params
			health.ChangeHealth(+metabolism*Time.deltaTime*10);
		}

		private void Reproduce(MemeController c)
		{
			// Stop moving
			movement.isStopped = true;
			health.ChangeHealth(-reproductionLifeLoss);

			// Spawning a child around
			// var p = (transform.position + Random.insideUnitSphere * 10).AboveGround();
			var go = Generate.instance.SpawnHost(transform.position, Quaternion.identity); //Pool.Spawn(prefab, transform.position, Quaternion.identity);
			var th = c.currentObservation.Target.GetComponent<Animal>();

			// Decrease target life now
			c.currentObservation.Target.GetComponent<Health>().ChangeHealth(-reproductionLifeLoss);

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
		private Meme SeeFood(MemeController c)
		{
			var layerMask = 1 << LayerMask.NameToLayer("Vegetation");

			// Any matching object around ? Try to get the closest if any
			var min = transform.position.Closest(sightRange, layerMask);

			// No food around OR target is dead / too weak
			if (min == null || min.GetComponent<Health>().dead) return null;
			c.currentObservation.Target = min;

			return m_Reach;
		}

		private Meme SeePartner(MemeController c)
		{
			// Look for partner
			if (Time.time > LastBreed + reproductionDelay && health.currentHealth > reproductionThreshold)
			{
				var layerMask = 1 << LayerMask.NameToLayer("Animal");

				// Any matching object around ? Try to get the closest if any
				var min = transform.position.Closest(sightRange, layerMask);
				// TODO: closest with enough life to breed
				// No animal to breed with around
				if (min == null) return null;
				c.currentObservation.Target = min;

				return m_Reach;
			}

			return null;
		}

		private Meme IsCloseEnoughForEating(MemeController c)
		{
			return Vector3.Distance(transform.position, c.currentObservation.Target.transform.position) <
			       eatRange ? m_Eat : null;
		}

		private Meme IsCloseEnoughForBreeding(MemeController c)
		{
			return Vector3.Distance(transform.position, c.currentObservation.Target.transform.position) <
			       1 ? Breed : null;
		}


		private Meme IsTargetAlive(MemeController c)
		{
			return c.currentObservation.Target.GetComponent<Health>().dead ? m_Wander : null;
		}

		private Meme Timeout(MemeController c)
		{
			// Could be improved
			return c.lastTransition + 10f > Time.time ? m_Wander : null;
		}

		#endregion
	}
}
