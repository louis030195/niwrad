using System.Collections.Generic;
using AI;
using UnityEngine;

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
		[Header("Evolution parameters"), Range(0.1f, 5f)]
		public float decisionFrequency = 1f;
		[Header("Initial characteristics"), Range(20, 80)]
		public float initialLife = 40f;
		[Tooltip("How much life losing over time"), Range(0.1f, 2.0f)] // TODO: fix names
		public float robustness = 1f;

		[Header("Reproduction"), Range(20, 80)]
		public float reproductionThreshold = 80f;
		[Range(1, 100)]
		public float reproductionDelay = 20f;


		[HideInInspector] public Attack attack;
		[HideInInspector] public Health health;
		[HideInInspector] public ulong id;
		[HideInInspector] public MemeController controller;
		/// <summary>
		/// Map of (name; meme)
		/// </summary>
		[HideInInspector] public Dictionary<string, Meme> memes = new Dictionary<string, Meme>();

		protected float LastBreed;
		// Do plants and animals share senses ? like:
		// protected Sense<GameObject> feel;
		// protected Memory<GameObject> feelMemory;

		protected void OnEnable()
		{
			health = GetComponent<Health>();
			health.initialLife = initialLife;
			attack = GetComponent<Attack>();
			controller = GetComponent<MemeController>();

			// Not sure required, maybe could be useful to prevent hosts forgetting to implement breeding meme
			var n = "Breed";
			memes[n] = new Meme(n, null, null);
		}

		protected void Update()
		{
			health.ChangeHealth(-robustness*Time.deltaTime);
		}

		protected void OnDisable()
		{
			controller.aiActive = false;
		}

		/// <summary>
		/// Public function to bring the host to life
		/// </summary>
		public abstract void BringToLife();
		// If this function is not overrode, will setup host with random initial meme
		// if (memes.Values.Count > 0) controller.SetupAi(memes.Values.AnyItem(), true);

	}
}
