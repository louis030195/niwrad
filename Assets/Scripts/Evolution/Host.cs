﻿using System;
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
		protected float LastBreed;

		/// <summary>
		/// Age represents the life length of an host between 0 and 100
		/// </summary>
		protected float Age;
		// Do plants and animals share senses ? like:
		// protected Sense<GameObject> feel;
		// protected Memory<GameObject> feelMemory;

        protected void Awake()
        {
            Age = 0;
            health = GetComponent<Health>();
            attack = GetComponent<Attack>();
            controller = GetComponent<MemeController>();
            // Not sure required, maybe could be useful to prevent hosts forgetting to implement breeding meme
            var n = "Breed";
            Memes[n] = new Meme(n, null, null);
        }

        protected void Update()
		{
			if (Time.frameCount % 5 != 0) return;
			
			Age++;
			// The older, the weaker
            var energyThreshold = 0.1f;
            // If high energy, gain health
            if (characteristics.Energy > characteristicsMax.Energy * (1 - energyThreshold))
            {
                health.ChangeHealth(Time.deltaTime*(1-Age/10));
            } 
            // If low energy, lose health
            else if (characteristics.Energy < characteristicsMin.Energy * (1 + energyThreshold))
            {
                health.ChangeHealth(-Time.deltaTime*(1+Age/10));
            }
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
    }
}