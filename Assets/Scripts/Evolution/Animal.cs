using System;
using StateMachine;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Evolution
{
	[RequireComponent(typeof(Movement))]
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(Attack))]
	public class Animal : MonoBehaviour
	{
		[Header("Evolution parameters")]
		[Header("Initial configuration")]
		[Range(20, 80)]
		[SerializeField]
		private float initialLife = 40f;
		[Range(2, 20)]
		[SerializeField]
		private float initialSpeed = 5f;

		[Header("Reproduction")]
		[Range(20, 80)]
		[SerializeField]
		private float reproductionThreshold = 80f;
		[Range(20, 80)]
		[SerializeField]
		private float reproductionLifeLoss = 50f;
		[Range(1, 100)]
		[SerializeField]
		private float reproductionDelay = 20f;


		[HideInInspector] public Movement movement;
		[HideInInspector] public Attack attack;
		[HideInInspector] public Health health;

		private StateController m_Controller;
		private State m_Wander;
		private State m_Reach;
		private State m_Eat;
		private State m_Reproduce;
		private float m_LastReproduction;


		private void OnEnable()
		{
			movement = GetComponent<Movement>();
			movement.speed = initialSpeed;
			health = GetComponent<Health>();
			health.initialLife = initialLife;
			attack = GetComponent<Attack>();
			m_Controller = GetComponent<StateController>();

			m_Wander = new State(
				new Action<StateController>[]
				{
					RandomMovement
				},
				new Func<StateController, State>[]
				{
					SeeFoodOrPartner
				},
				Color.white
			);
			m_Reach = new State(
				new Action<StateController>[]
				{
					Reach
				},
				new Func<StateController, State>[]
				{
					IsCloseEnough
				},
				Color.red
			);
			m_Eat = new State(
				new Action<StateController>[]
				{
					Reach,
					Eat
				},
				new Func<StateController, State>[]
				{
					IsTargetAlive
				},
				Color.blue
			);
			m_Reproduce = new State(
				new Action<StateController>[]
				{
					Reach,
					Reproduce
				},
				new Func<StateController, State>[]
				{
					SeeFoodOrPartner
				},
				Color.magenta
			);

			m_Controller.SetupAi(m_Wander, true);
		}

		private void OnDisable()
		{
			m_Controller.aiActive = false;
		}


		#region Actions
		private void Reach(StateController c)
		{
			if (c.currentObservation.Target == null) return;
			// Debug.Log($"reach {movement.remainingDistance} - {movement.stoppingDistance}");
			// if (movement.remainingDistance <= movement.stoppingDistance && !movement.pathPending)
			// {
				movement.MoveTo(c.currentObservation.Target.transform.position);
			// }
		}

		private void RandomMovement(StateController _)
		{
			// if (movement.remainingDistance <= movement.stoppingDistance && !movement.pathPending)
			// {
				var p = transform.position + Random.insideUnitSphere * 10;
				movement.MoveTo(p.AboveGround());
			// }
		}

		private void Eat(StateController c)
		{
			// Stop moving
			movement.isStopped = true;
			attack.EatTarget(c.currentObservation.Target);
			health.ChangeHealth(+10);
		}

		private void Reproduce(StateController c)
		{
			// Stop moving
			movement.isStopped = true;
			health.Reproduce(c.currentObservation.Target, reproductionLifeLoss);
			m_LastReproduction = Time.time;
		}
		#endregion

		#region Transitions
		private State SeeFoodOrPartner(StateController c)
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
			var min = transform.position.Closest(100_000, layerMask);
			if (min != null)
			{
				c.currentObservation.Target = min;
				return m_Reach;
			}

			return null;
		}

		private State IsCloseEnough(StateController c)
		{
			return Vector3.Distance(transform.position, c.currentObservation.Target.transform.position) <
			       2f ? c.currentObservation.ReproductionMode ? m_Reproduce : m_Eat : null;
		}


		private State IsTargetAlive(StateController c)
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
