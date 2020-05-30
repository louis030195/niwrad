using System.Linq;
using UnityEngine;


namespace StateMachine
{
	public class StateController : MonoBehaviour
	{

		public State currentState;
		public Parameters parameters;
		public Transform eyes;
		public State remainState;

		[HideInInspector] public Movement movement;
		[HideInInspector] public Attack attack;
		[HideInInspector] public Health health;
		[HideInInspector] public AudioSource audioSource;
		[HideInInspector] public Transform master;
		[HideInInspector] public Transform target;
		[HideInInspector] public float stateTimeElapsed;

		private bool m_AiActive;


		private void Awake()
		{
			attack = GetComponent<Attack>();
			movement = GetComponent<Movement>();
			health = GetComponent<Health>();
			audioSource = GetComponent<AudioSource>();
		}

		private void Start()
		{
			movement.speed = parameters.moveSpeed;
		}

		public void SetupAi(bool activate)
		{
			movement.enabled = m_AiActive = activate;
		}

		private void Update()
		{
			if (!m_AiActive)
				return;
			currentState.UpdateState(this);
		}

		private void OnDrawGizmos()
		{
			if (currentState != null && eyes != null)
			{
				Gizmos.color = currentState.sceneGizmoColor;
				Gizmos.DrawWireSphere(eyes.position, parameters.lookSphereCastRadius);
			}
		}

		public void TransitionToState(State nextState)
		{
			if (nextState != remainState)
			{
				currentState = nextState;
				OnExitState();
			}
		}

		public bool CheckIfCountDownElapsed(float duration)
		{
			stateTimeElapsed += Time.deltaTime;
			return (stateTimeElapsed >= duration);
		}

		private void OnExitState()
		{
			stateTimeElapsed = 0;
		}
	}
}
