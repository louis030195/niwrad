using System;
using System.Collections.Generic;
using AI;
using Api.Realtime;
using Api.Session;
using Gameplay;
using UnityEngine;
using Meme = AI.Meme;

namespace Evolution
{
    
	/// <summary>
	/// Host is a survival machine carrying genes, memes or anything that follow darwinian evolution
	/// </summary>
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(Attack))]
	[RequireComponent(typeof(MemeController))]
	public abstract class Host : MonoBehaviour
    {

        public Characteristics characteristics;
        public Characteristics characteristicsMin;
        public Characteristics characteristicsMax;
		[HideInInspector] public Attack attack;
		[HideInInspector] public Health health;
		[HideInInspector] public ulong id;
		[HideInInspector] public MemeController controller;
		/// <summary>
		/// Map of (name; meme)
		/// </summary>
        public readonly Dictionary<string, Meme> Memes = new Dictionary<string, Meme>();
        // public event System.Action<Host> Bred;
		protected float LastBreed;
        protected const int WaterLayer = 4;

		/// <summary>
		/// Age represents the life length of an host between 0 and 100
		/// </summary>
        private float _age;

        private Renderer _renderer; // TODO: nothing to do here
        private Color _originalColor;
        
        protected void Awake()
        {
            _age = 0;
            health = GetComponent<Health>();
            attack = GetComponent<Attack>();
            controller = GetComponent<MemeController>();
            // Not sure required, maybe could be useful to prevent hosts forgetting to implement breeding meme
            var n = "Breed";
            Memes[n] = new Meme(n, null, null);
            _renderer = GetComponent<Renderer>();
            _originalColor = _renderer.material.color;
        }

        protected void Update()
		{
			if (Time.frameCount % 5 != 0) return;

            // All hosts loses energy over time depending on energy loss and the host size (big hosts consume more energy)
            characteristics.Energy -= Time.timeScale * (characteristics.EnergyLoss / 100) * transform.localScale.magnitude; //* Age;
			_age++;
			// The older, the weaker
            var energyThreshold = 0.1f;
            // If high energy, gain health
            if (characteristics.Energy > characteristicsMax.Energy * (1 - energyThreshold))
            {
                health.AddHealth(0.1f * Time.timeScale*(1 + Mathf.Clamp(characteristics.Robustness/_age, 0, 1)));
                // Debug.Log($"{name} high energy health.AddHealth {Time.deltaTime*(1 + Mathf.Clamp(characteristics.Robustness/Age, 0, 1))}");
            } 
            // If low energy, lose health
            else if (characteristics.Energy < characteristicsMin.Energy * (1 + energyThreshold))
            {
                health.AddHealth(-0.1f * Time.timeScale*(1 - Mathf.Clamp(characteristics.Robustness/_age, 0, 1)));
                // Debug.Log($"{name} low energy health.AddHealth {-Time.deltaTime*(1 - Mathf.Clamp(characteristics.Robustness/Age, 0, 1))}");
            }

            _renderer.material.color = _originalColor * (100 / health.currentHealth);
        }

        /// <summary>
        /// Public function to bring the host to life, or deactivate
        /// </summary>
        public void EnableBehaviour(bool value)
        {
            if (value)
            {
                if (!Gm.instance.online)
                {
                    health.Died += OnDeath;
                    return;
                } 
                if (Sm.instance && Sm.instance.isServer)
                {
                    health.Died += OnDeath;
                }
                health.initialLife = characteristics.Life;
            }
            else
            {
                if (Sm.instance && Sm.instance.isServer)
                {
                    health.Died -= OnDeath;
                }
            }
        }
		// If this function is not overrode, will setup host with random initial meme
		// if (memes.Values.Count > 0) controller.SetupAi(memes.Values.AnyItem(), true);

        protected abstract void OnDeath();
        public abstract bool CanBreed();
    }
}
