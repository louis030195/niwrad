using UnityEngine;

namespace Evolution
{
	public class Attack : MonoBehaviour
	{
		[Header("Audio")]
		[SerializeField] private AudioClip[] attackClip; // Audio that plays when each attack is fired.
		[SerializeField] private AudioClip[] eatClip; // Audio that plays when eating is fired.

		private Animator m_Animator;
		private AudioSource m_AudioSource;

		private void Start()
		{
			m_AudioSource = GetComponent<AudioSource>();
			m_Animator = GetComponent<Animator>();
		}

		public void AttackTarget(Vector3 position)
		{
			// TODO: play audio + animation

		}

		public void EatTarget(GameObject target) // TODO: not sure if should merge Attack + Health idk
		{
			// TODO: play audio + animation
			target.GetComponent<Health>().ChangeHealth(-10); // TODO: think where to store params
		}
	}
}
