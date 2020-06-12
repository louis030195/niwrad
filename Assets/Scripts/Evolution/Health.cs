using System;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Evolution
{
	public class Health : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField]
        private int maxHealth = 100;
        [SerializeField]
        private bool destroyOnDeath;
        public event Action<float> HealthChanged;

        [Header("Audio")]
        [Tooltip("Clips to play when dying."), SerializeField]
        private AudioClip[] dyingClips;
        [Tooltip("Clips to play when getting hit"), SerializeField]
        private AudioClip[] gettingHitClips;
        [Tooltip("Clips to play when healed."), SerializeField]
        private AudioClip[] healingClips;

        [Header("Effects")]
        [Tooltip("Death effects to spill around"), SerializeField]
        private GameObject[] deathEffects;

        [Header("Animations")]
        [SerializeField]
        private string[] gettingHitAnimations;
        [SerializeField]
        private string[] dyingAnimations;
        public float currentHealth => m_CurrentHealth;
        [HideInInspector] public bool dead;
        [HideInInspector] public float initialLife = 40f;

        private float m_CurrentHealth;
        private Animator m_Animator;
        private AudioSource m_AudioSource;

        private void OnEnable()
        {
	        dead = false;
	        m_CurrentHealth = initialLife;
	        m_AudioSource = GetComponent<AudioSource>();
	        m_Animator = GetComponent<Animator>();
        }

        // TODO: fix audio / anims someday
        private void PlayAudio(bool damage = true)
        {
            if (m_AudioSource && m_AudioSource.isActiveAndEnabled)
            {
	            if (damage) m_AudioSource.clip = !dead ? gettingHitClips.AnyItem() : dyingClips.AnyItem();
	            else m_AudioSource.clip = healingClips.AnyItem();

                if (!m_AudioSource.isPlaying && m_AudioSource.clip != null)
                {
                    m_AudioSource.spatialBlend = 0.3f;
                    m_AudioSource.pitch = Random.Range(0.8f, 1.2f);
                    m_AudioSource.Play();
                }
            }
        }

        private void Damage(float amount)
        {
	        // Debug.Log($"Current  life {m_CurrentHealth}, got damaged {amount}");

	        if (m_CurrentHealth > 0 && gettingHitAnimations.Length > 0) // If there is getting hit animations for this object
            {
                m_Animator.SetTrigger(gettingHitAnimations.AnyItem());
            }
            // Update UI
            PlayAudio();
        }




        public void ChangeHealth(float amount)
        {
	        m_CurrentHealth += amount;
	        if (m_CurrentHealth > maxHealth)
	        {
		        m_CurrentHealth = maxHealth;
	        }
	        HealthChanged?.Invoke(m_CurrentHealth / maxHealth);

	        if (m_CurrentHealth <= 0 && !dead)
	        {
		        dead = true;
		        if (destroyOnDeath)
		        {
			        Pool.Despawn(gameObject);
			        if (deathEffects.Length > 0) // Unused, prob not ready for working
			        {
				        var p = transform.position;
				        Pool.Despawn(
					        Pool.Spawn(deathEffects.AnyItem(),
						        new Vector3(p.x, p.y, p.z),
						        new Quaternion(0, 0, 0, 0)), 3);
			        }
		        }

		        if (dyingAnimations.Length > 0)
		        {
			        // If there is death animations for this object
			        m_Animator.SetBool(dyingAnimations.AnyItem(),
				        true); // TODO: not rly useful if destroyed ... (maybe should add death delay idk)
		        }
	        }
        }
    }
}
