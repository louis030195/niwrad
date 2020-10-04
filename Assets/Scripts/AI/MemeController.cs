using System;
using Evolution;
using UnityEngine;

namespace AI
{
	public class MemeController : MonoBehaviour
	{
		private float _decisionFrequency;
		private float _lastDecision;
		private Meme _currentMeme;

		public bool aiActive;
		public float lastTransition;
		public event Action<Meme> MemeChanged;
		public event System.Action BeforeUpdated;

		private void Update()
		{
			if (!aiActive || Time.time < _lastDecision + _decisionFrequency / Time.timeScale) return;
			_lastDecision = Time.time;
			BeforeUpdated?.Invoke();
			_currentMeme.UpdateState(this); // TODO: nullref here sometime
		}

		private void OnDrawGizmos()
        {
            if (_currentMeme == null) return;
            Gizmos.color = _currentMeme.SceneGizmoColor;
            Gizmos.DrawWireSphere(transform.position, 10);
        }

		public void SetupAi(Meme currentMeme, float decisionFrequency = 2f)
        {
            aiActive = true;
			_currentMeme = currentMeme;
			_decisionFrequency = decisionFrequency;
			MemeChanged?.Invoke(_currentMeme);
		}

		public void Transition(Meme nextMeme)
		{
			_currentMeme = nextMeme;
			MemeChanged?.Invoke(_currentMeme);
			lastTransition = Time.time;
		}
	}
}
