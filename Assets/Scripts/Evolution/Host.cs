using System;
using StateMachine;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Evolution
{
	/// <summary>
	/// Host is a survival machine carrying genes, memes or anything that follow darwinian evolution
	/// </summary>
	[RequireComponent(typeof(Movement))]
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(Attack))]
	public class Host : MonoBehaviour
	{
		[Header("Evolution parameters")]
		[Range(0.1f, 5f)]
		public float decisionFrequency = 1f;
		[Header("Initial characteristics")]
		[Range(20, 80)]
		public float initialLife = 40f;
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
		[Range(0.1f, 2.0f)]
		[Tooltip("How much life losing over time")] // TODO: fix names
		public float robustness = 1f;


		[Header("Reproduction")]
		[Range(20, 80)]
		public float reproductionThreshold = 80f;
		[Range(20, 80)]
		public float reproductionLifeLoss = 50f;
		[Range(1, 100)]
		public float reproductionDelay = 20f;



		[HideInInspector] public Movement movement;
		[HideInInspector] public Attack attack;
		[HideInInspector] public Health health;
		[HideInInspector] public GameObject prefab;

		private MemeController m_Controller;
		private Meme m_Wander;
		private Meme m_Reach;
		private Meme m_Eat;
		private Meme m_Reproduce;
		private float m_LastReproduction;


		private void OnEnable()
		{
			movement = GetComponent<Movement>();
			movement.speed = initialSpeed;
			health = GetComponent<Health>();
			health.initialLife = initialLife;
			attack = GetComponent<Attack>();
			m_Controller = GetComponent<MemeController>();

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
			m_Reproduce = new Meme(
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

			m_Controller.SetupAi(m_Wander, true, decisionFrequency);
		}

		private void Update()
		{
			health.ChangeHealth(-robustness*Time.deltaTime);
		}

		private void OnDisable()
		{
			m_Controller.aiActive = false;
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
				movement.MoveTo(p.AboveGround());
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
			var go = Pool.Spawn(prefab, transform.position, Quaternion.identity);
			var th = c.currentObservation.Target.GetComponent<Host>();
			var childHost = go.GetComponent<Host>();
			var mutate = new Func<float, float, float, float>((a, b, mutationDegree) =>
			{
				var md = Mathf.Abs(mutationDegree) > 1 ? 1 : Mathf.Abs(mutationDegree);
				return ((a + b) / 2) * (1 + Random.Range(-md, md));
			});
			childHost.initialLife = mutate(initialLife, th.initialLife, 1f);
			childHost.initialSpeed = mutate(initialSpeed, th.initialSpeed, 1f);
			childHost.randomMovementRange = mutate(randomMovementRange, th.randomMovementRange, 1f);
			childHost.sightRange = mutate(sightRange, th.sightRange, 1f);
			childHost.eatRange = mutate(eatRange, th.eatRange, 0.1f);
			childHost.metabolism = mutate(metabolism, th.metabolism, 0.1f);
			childHost.robustness = mutate(robustness, th.robustness, 0.1f);
			// TODO: somehow: metabolism.GetRangePropertyValue() ...
			// this.GetType().GetField(nameof(bar)).GetAttributes()

			// go.GetComponent<MeshFilter>().mesh.Mutation();

			// TODO: the new host should have its memes tweaked by meme controller (mutation ...)
			m_LastReproduction = Time.time;
		}
		#endregion

		#region Transitions
		private Meme SeeFoodOrPartner(MemeController c)
		{
			int layerMask;

			// Look for partner
			if (Time.time > m_LastReproduction + reproductionDelay && health.currentHealth > reproductionThreshold)
			{
				// Bit shift the index of the layer (9) to get a bit mask
				layerMask = 1 << 9;
				c.currentObservation.ReproductionMode = true;
			}
			else // Look for food
			{
				// Bit shift the index of the layer (8) to get a bit mask
				layerMask = 1 << 8;
				c.currentObservation.ReproductionMode = false;
			}

			// Any matching object around ? Try to get the closest if any
			var min = transform.position.Closest(sightRange, layerMask);
			if (min == null) return null;
			c.currentObservation.Target = min;
			return m_Reach;
		}

		private Meme IsCloseEnough(MemeController c)
		{
			return Vector3.Distance(transform.position, c.currentObservation.Target.transform.position) <
			       eatRange ? c.currentObservation.ReproductionMode ? m_Reproduce : m_Eat : null;
		}


		private Meme IsTargetAlive(MemeController c)
		{
			// Debug.Log($"Target dead: {c.currentObservation.Target.GetComponent<Health>().currentHealth} " +
			//           $"{c.currentObservation.Target.GetComponent<Health>().dead}" +
			//           $"{c.currentObservation.Target.transform.position}");
			return c.currentObservation.Target == null ||
			       !c.currentObservation.Target.activeInHierarchy ||
			       c.currentObservation.Target.GetComponent<Health>().dead ? m_Wander : null;
		}

		#endregion
	}
}
