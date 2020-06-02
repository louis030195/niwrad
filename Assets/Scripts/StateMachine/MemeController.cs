using UnityEngine;
using UnityEngine.Serialization;

namespace StateMachine
{
	public class MemeController : MonoBehaviour
	{
		private float m_DecisionFrequency;
		private float m_LastDecision;
		private Meme m_CurrentMeme;
		public bool aiActive;
		public Observation currentObservation;

		private void OnEnable()
		{
			currentObservation = new Observation();
		}

		private void Update()
		{
			if (!aiActive || Time.time < m_LastDecision) return;
			m_LastDecision = Time.time + m_DecisionFrequency;
			m_CurrentMeme.UpdateState(this);
		}

		private void OnDrawGizmos()
		{
			if (m_CurrentMeme != null)
			{
				Gizmos.color = m_CurrentMeme.SceneGizmoColor;
				Gizmos.DrawWireSphere(transform.position, 10);
			}
		}

		public void SetupAi(Meme currentMeme, bool activate, float decisionFrequency = 2f)
		{
			m_CurrentMeme = currentMeme;
			aiActive = activate;
			m_DecisionFrequency = decisionFrequency;
		}

		public void TransitionToState(Meme nextMeme)
		{
			m_CurrentMeme = nextMeme;
		}
	}
}
