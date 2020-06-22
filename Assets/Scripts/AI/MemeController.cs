using System;
using Evolution;
using UnityEngine;

namespace AI
{
	public class MemeController : MonoBehaviour
	{
		private float m_DecisionFrequency;
		private float m_LastDecision;
		private Meme m_CurrentMeme;

		public bool aiActive;
		public float lastTransition;
		public event Action<Meme> MemeChanged;
		public event System.Action BeforeUpdated;

		private void Update()
		{
			if (!aiActive || Time.time < m_LastDecision + m_DecisionFrequency) return;
			m_LastDecision = Time.time;
			BeforeUpdated?.Invoke();
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
			MemeChanged?.Invoke(m_CurrentMeme);
		}

		public void Transition(Meme nextMeme)
		{
			m_CurrentMeme = nextMeme;
			MemeChanged?.Invoke(m_CurrentMeme);
			lastTransition = Time.time;
		}
	}
}
