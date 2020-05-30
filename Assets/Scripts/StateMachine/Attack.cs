using UnityEngine;

namespace StateMachine
{
	public class Attack : MonoBehaviour
	{
		[Header("Audio")]
		[SerializeField] private AudioClip[] attackClip; // Audio that plays when each attack is fired.

		private Animator m_Animator;
		private AudioSource m_AudioSource;

		private void Start()
		{
			m_AudioSource = GetComponent<AudioSource>();
			m_Animator = GetComponent<Animator>();
		}

		public void AttackTarget(Vector3 position)
		{

		}
	}
}
