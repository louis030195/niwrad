using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Gameplay;
using Net.Match;
using Net.Realtime;
using Net.Utils;
using UnityEngine;
using Utils;
using Action = AI.Action;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

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
		[Range(1, 100.0f), Tooltip("How much life eating bring")]
		public float metabolism = 10f;

		[Header("Reproduction")]
		[Range(20, 80)]
		public float reproductionLifeLoss = 50f;

		[HideInInspector] public Movement movement;

		private Sense<List<GameObject>> m_VisionSense;
		private Memory<GameObject> m_VisionMemory;

		protected new void OnEnable()
		{
			base.OnEnable();
			movement = GetComponent<Movement>();
			movement.speed = initialSpeed;

			// An animal see around him, possibly several objects
			m_VisionSense = new Sense<List<GameObject>>();
			var res = new Collider[100];
			Func<List<GameObject>> collector = () =>
			{
				Physics.OverlapSphereNonAlloc(transform.position,
					sightRange,
					res,
					LayerMask.GetMask("Animal", "Vegetation"));
				return res.Where(c => c != null).Select(c => c .gameObject).ToList();
			};
			m_VisionSense.ListenTo(collector);

			// It append the seen data in current memory
			m_VisionMemory = new Memory<GameObject>();
			m_VisionSense.Triggered += o => m_VisionMemory.Input(o);


			// Collect data before update only
			controller.BeforeUpdated += () =>
			{
				m_VisionSense.Update();
				m_VisionMemory.Update();
			};

			var foodAround = new Transition("FoodAround", 0, FoodAround);
			var partnerAround = new Transition("PartnerAround", -2, PartnerAround);
			var timeout = new Transition("Timeout", -1, Timeout);
			var n = "Wander";
			memes[n] = new Meme(
				n,
				new List<Action>
				{
					new Action("RandomMovement", RandomMovement)
				},
				new List<Transition>
				{
					foodAround,
					partnerAround
				},
				Color.white
			);
			n = "ReachFood";
			memes[n] = new Meme(
				n,
				new List<Action>
				{
					new Action("ReachFood", ReachFood)
				},
				new List<Transition>
				{
					new Transition("IsCloseEnoughForEating", 0, IsCloseEnoughForEating),
					// timeout
				},
				Color.red
			);
			n = "ReachPartner";
			memes[n] = new Meme(
				n,
				new List<Action>
				{
					new Action("ReachPartner", ReachPartner)
				},
				new List<Transition>
				{
					new Transition("IsCloseEnoughForBreeding", -1, IsCloseEnoughForBreeding),
					// timeout
				},
				Color.red
			);
			n = "Eat";
			memes[n] = new Meme(
				n,
				new List<Action>
				{
					// Reach,
					new Action("Eat", Eat)
				},
				new List<Transition>
				{
					new Transition("IsTargetAlive", 0, IsTargetAlive),
					partnerAround // While eating, if it can breed, go for it
				},
				Color.blue
			);
			n = "Breed";
			memes[n] = new Meme(
				n,
				new List<Action>
				{
					new Action("Reproduce", Reproduce)
					// Reach,
				},
				new List<Transition>
				{
					foodAround,
					partnerAround
				},
				Color.magenta
			);
		}


		public override void BringToLife()
		{
			controller.SetupAi(memes["Wander"], true, decisionFrequency);
		}

		#region Actions
		private void ReachFood(MemeController c)
		{
			if (movement.remainingDistance <= movement.stoppingDistance)
			{
				var closest = m_VisionMemory.Query().Closest(transform.position,
					LayerMask.NameToLayer("Vegetation"));
				if (closest != default) movement.MoveTo(closest.transform.position);
			}
		}

		private void ReachPartner(MemeController c)
		{
			if (movement.remainingDistance <= movement.stoppingDistance)
			{
				var closest = m_VisionMemory.Query().Closest(transform.position,
					LayerMask.NameToLayer("Animal"));
				if (closest != default) movement.MoveTo(closest.transform.position);
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
			var closest = m_VisionMemory.Query().Closest(transform.position,
				LayerMask.NameToLayer("Vegetation"));
			if (closest == default) return;
			attack.EatTarget(closest);
			closest.GetComponent<Health>().ChangeHealth(-Time.deltaTime*30); // TODO: store params
			// +metabolism (10) *Time.deltaTime*0.5f // seems balanced
			// TODO: maybe age reduce life gain on eat ?
			health.ChangeHealth(+metabolism*Time.deltaTime*50f);
		}

		private void Reproduce(MemeController c)
		{
			var closest = m_VisionMemory.Query().Closest(transform.position,
				LayerMask.NameToLayer("Animal"));
			if (closest == default) return;
			var th = closest.GetComponent<Animal>();

			// Stop moving
			movement.isStopped = true;
			// It's costly to reproduce, proportional to animal age
			health.ChangeHealth(-reproductionLifeLoss*(1+Age/100));

			// Spawning a child around
			// var p = (transform.position + Random.insideUnitSphere * 10).AboveGround();
			var childHost = HostManager.instance.SpawnAnimal(transform.position, Quaternion.identity);
			if (!childHost)
			{
				Debug.LogError($"Reproduce couldn't spawn animal");
				return;
			}

			// Decrease target life now
			if (closest)
			{
				closest.GetComponent<Health>().ChangeHealth(-reproductionLifeLoss);
			}
			else
			{
				Debug.LogWarning($"Partner died while breeding");
			}


			var r = ReflectionExtension.GetRange(GetType(), nameof(initialLife));
			childHost.initialLife = Mathf.Clamp(Mutate(initialLife, th.initialLife, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(initialSpeed));
			childHost.initialSpeed = Mathf.Clamp(Mutate(initialSpeed, th.initialSpeed, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(randomMovementRange));
			childHost.randomMovementRange = Mathf.Clamp(Mutate(randomMovementRange, th.randomMovementRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(sightRange));
			childHost.sightRange = Mathf.Clamp(Mutate(sightRange, th.sightRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(eatRange));
			childHost.eatRange = Mathf.Clamp(Mutate(eatRange, th.eatRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(metabolism));
			childHost.metabolism = Mathf.Clamp(Mutate(metabolism, th.metabolism, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(robustness));
			childHost.robustness = Mathf.Clamp(Mutate(robustness, th.robustness, 1f), r.min, r.max);

			// go.GetComponent<MeshFilter>().mesh.Mutation();

			// TODO: the new host should have its memes tweaked by meme controller (mutation ...)
			LastBreed = Time.time;
		}
		#endregion

		#region Transitions
		private Meme FoodAround(MemeController c)
		{
			// TODO: params
			if (health.currentHealth > 90f) return memes["Wander"];

			var closest = m_VisionMemory.Query().Closest(transform.position,
				LayerMask.NameToLayer("Vegetation"));

			// No food around OR target is dead / too weak
			if (closest == default || closest.GetComponent<Health>().dead) return null;

			// Stop current movement
			// movement.navMeshAgent.destination = transform.position;

			return memes["ReachFood"];
		}

		private Meme PartnerAround(MemeController c)
		{
			// Look for partner
			if (Time.time > LastBreed + reproductionDelay && health.currentHealth > reproductionThreshold)
			{
				var closest = m_VisionMemory.Query().Closest(transform.position,
					LayerMask.NameToLayer("Animal"));
				// TODO: closest with enough life to breed
				// No animal to breed with around
				if (closest == default) return null;

				// Stop current movement
				// movement.navMeshAgent.destination = transform.position;

				return memes["ReachPartner"];
			}

			return null;
		}

		private Meme IsCloseEnoughForEating(MemeController c)
		{
			var closest = m_VisionMemory.Query().Closest(transform.position,
				LayerMask.NameToLayer("Vegetation"));
			if (closest != default && Vector3.Distance(transform.position, closest.transform.position) < eatRange)
				return memes["Eat"];
			return null;
		}

		private Meme IsCloseEnoughForBreeding(MemeController c)
		{
			var closest = m_VisionMemory.Query().Closest(transform.position,
				LayerMask.NameToLayer("Animal"));
			if (closest != default && Vector3.Distance(transform.position, closest.transform.position) < 1)
				return memes["Breed"];
			return null;
		}


		private Meme IsTargetAlive(MemeController c)
		{
			var closest = m_VisionMemory.Query().Closest(transform.position,
				LayerMask.NameToLayer("Animal"));

			return closest != default && closest.GetComponent<Health>().dead ? memes["Wander"] : null;
		}

		private Meme Timeout(MemeController c)
		{
			// Could be improved
			return c.lastTransition + 10f > Time.time ? memes["Wander"] : null;
		}

		#endregion
	}
}
