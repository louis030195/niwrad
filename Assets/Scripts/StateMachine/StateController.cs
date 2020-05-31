using UnityEngine;
using UnityEngine.Serialization;

namespace StateMachine
{
	public class StateController : MonoBehaviour
	{
		private float m_DecisionFrequency;
		private float m_LastDecision;
		private State m_CurrentState;
		public bool aiActive;
		public Observation currentObservation;

		private void Awake()
		{
			currentObservation = new Observation();
		}

		private void Update()
		{
			if (!aiActive || Time.time < m_LastDecision) return;
			m_LastDecision = Time.time + m_DecisionFrequency;
			m_CurrentState.UpdateState(this);
		}

		private void OnDrawGizmos()
		{
			if (m_CurrentState != null)
			{
				Gizmos.color = m_CurrentState.SceneGizmoColor;
				Gizmos.DrawWireSphere(transform.position, 10);
			}
		}

		public void SetupAi(State currentState, bool activate, float decisionFrequency = 2f)
		{
			m_CurrentState = currentState;
			aiActive = activate;
			m_DecisionFrequency = decisionFrequency;
		}

		public void TransitionToState(State nextState)
		{
			m_CurrentState = nextState;
		}
	}
}
