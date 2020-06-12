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
	{ // TODO: prob gotta move evolution params into scriptable objects ? for saving good params ... ?
		[Header("Evolution parameters"), Range(0.1f, 50f)]
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

		/// <summary>
		/// Age represents the life length of an host between 0 and 100
		/// </summary>
		protected float Age;
		// Do plants and animals share senses ? like:
		// protected Sense<GameObject> feel;
		// protected Memory<GameObject> feelMemory;

		protected void OnEnable()
		{
			health = GetComponent<Health>();
			health.initialLife = initialLife;
			attack = GetComponent<Attack>();
			controller = GetComponent<MemeController>();
			Age = 0;

			// Not sure required, maybe could be useful to prevent hosts forgetting to implement breeding meme
			var n = "Breed";
			memes[n] = new Meme(n, null, null);
		}

		protected void Update()
		{
			if (Age < 100 && Time.frameCount % 5 == 0) Age++;
			// The older, the weaker
			health.ChangeHealth(-robustness*Time.deltaTime*(1+Age/100));
		}

		protected void OnDisable()
		{
			controller.aiActive = false;
		}

		protected float Mutate(float a, float b, float mutationDegree)
		{
			var md = Mathf.Abs(mutationDegree) > 1 ? 1 : Mathf.Abs(mutationDegree);
			return (a + b) / 2 * (1 + Random.Range(-md, md));
		}

		/// <summary>
		/// Public function to bring the host to life
		/// </summary>
		public abstract void BringToLife();
		// If this function is not overrode, will setup host with random initial meme
		// if (memes.Values.Count > 0) controller.SetupAi(memes.Values.AnyItem(), true);

	}
}
