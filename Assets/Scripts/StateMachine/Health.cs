using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace StateMachine
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
        [Tooltip("Clips to play when dying.")]
        [SerializeField]
        private AudioClip[] dyingClips;
        [Tooltip("Clips to play when getting hit")]
        [SerializeField]
        private AudioClip[] gettingHitClips;

        [Header("Effects")]
        [Tooltip("Death effects to spill around")]
        [SerializeField]
        private GameObject[] deathEffects;

        [Header("Animations")]
        [SerializeField]
        private string[] gettingHitAnimations;
        [SerializeField]
        private string[] dyingAnimations;

        [HideInInspector] public int currentHealth;
        [HideInInspector] public bool dead;

        private Animator m_Animator;
        private AudioSource m_AudioSource;

        private void Start()
        {
            currentHealth = maxHealth;
            m_AudioSource = GetComponent<AudioSource>();
            m_Animator = GetComponent<Animator>();
        }

        private void PlayAudio()
        {
            if (m_AudioSource && m_AudioSource.isActiveAndEnabled)
            {
                m_AudioSource.clip = !dead ? gettingHitClips[Random.Range(0, gettingHitClips.Length - 1)] :
	                dyingClips[Random.Range(0, dyingClips.Length - 1)];

                if (!m_AudioSource.isPlaying && m_AudioSource.clip != null)
                {
                    m_AudioSource.spatialBlend = 0.3f;
                    m_AudioSource.pitch = Random.Range(0.8f, 1.2f);
                    m_AudioSource.Play();
                }
            }
        }

        public void ApplyDamage(int amount)
        {
	        // Update UI
            OnChangeHealth();
            if (currentHealth <= 0 && !dead)
            {

                dead = true;
                if (destroyOnDeath)
                {
                    Destroy(gameObject);
                    if (deathEffects.Length > 0) // Unused, prob not ready for working
	                    Destroy(
		                    Instantiate(deathEffects[Random.Range(0, deathEffects.Length)],
			                    new Vector3(transform.position.x, transform.position.y, transform.position.z),
			                    new Quaternion(0, 0, 0, 0)), 3);
                }

                if (dyingAnimations.Length > 0)
                {
                    var maxRandom = dyingAnimations.Length == 1 ? 0 : dyingAnimations.Length;
                    // If there is death animations for this object
                    m_Animator.SetBool(dyingAnimations[Random.Range(0, maxRandom)],
                        true); // TODO: not rly useful if destroyed ... (maybe should add death delay idk)
                }


            }

            if (currentHealth > 0 && gettingHitAnimations.Length > 0) // If there is getting hit animations for this object
            {
                //print($"myname {gameObject.name}");
                var maxRandom = gettingHitAnimations.Length == 1 ? 0 : gettingHitAnimations.Length;
                // Debug.Log($"I am { gameObject.name }");
                m_Animator.SetTrigger(gettingHitAnimations[Random.Range(0, maxRandom)]);
            }

            PlayAudio();
        }

        private void OnChangeHealth()
        {
	        HealthChanged.Invoke((float)currentHealth / maxHealth);
        }
    }
}
