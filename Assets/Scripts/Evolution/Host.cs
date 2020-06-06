using System;
using StateMachine;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Evolution
{
	/// <summary>
	/// Host is a survival machine carrying genes, memes or anything that follow darwinian evolution
	/// </summary>
	[RequireComponent(typeof(Health))]
	[RequireComponent(typeof(Attack))]
	[RequireComponent(typeof(MemeController))]
	public class Host : MonoBehaviour
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

		protected MemeController Controller;
		protected Meme Breed;

		protected float LastBreed;


		protected void OnEnable()
		{
			health = GetComponent<Health>();
			health.initialLife = initialLife;
			attack = GetComponent<Attack>();
			Controller = GetComponent<MemeController>();
		}

		protected void Update()
		{
			health.ChangeHealth(-robustness*Time.deltaTime);
		}

		protected void OnDisable()
		{
			Controller.aiActive = false;
		}
	}
}
