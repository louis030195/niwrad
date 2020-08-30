using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Gameplay;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Api.Utils;
using UnityEngine;
using Utils;
using Action = AI.Action;
using Meme = AI.Meme;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Evolution
{
	[RequireComponent(typeof(Movement))]
	public class CommonAnimal : Host
	{
		[Header("Initial characteristics")]
		[Range(2, 20)]
		public float initialSpeed = 5f;
		[Range(1, 1000)]
		public float randomMovementRange = 20f;
		[Range(1, 1000)]
		public float sightRange = 20f;
		[Range(2f, 10.0f)]
		public float eatRange = 5f;
		[Range(1, 100.0f), Tooltip("How much life eating bring")]
		public float metabolism = 10f;

		[Header("Reproduction")]
		[Range(20, 80)]
		public float reproductionLifeLoss = 50f;

		[HideInInspector] public Movement movement;

		private void OnDied()
		{
			Hm.instance.DestroyAnimalSync(id);
		}

		protected new void OnEnable()
		{
			base.OnEnable();
			movement = GetComponent<Movement>();
			movement.speed = initialSpeed;
			if (Sm.instance && Sm.instance.isServer)
			{
				health.Died += OnDied;
				movement.destinationChanged += OnDestinationChanged;
			}
		}

		protected new void OnDisable()
		{
			base.OnDisable();
			if (Sm.instance && Sm.instance.isServer)
			{
				health.Died -= OnDied;
			}
		}

		protected void BreedAndMutate(GameObject other)
		{
			var th = other.GetComponent<CommonAnimal>();

			// Stop moving
			movement.isStopped = true;
			// It's costly to reproduce, proportional to animal age
			health.ChangeHealth(-reproductionLifeLoss*(1+Age/100));

			// Spawning a child around
			// var p = (transform.position + Random.insideUnitSphere * 10).AboveGround();
			var childHost = Hm.instance.SpawnAnimalSync(transform.position, Quaternion.identity);
			if (childHost == null)
			{
				Debug.LogError($"Reproduce couldn't spawn animal");
				return;
			}

			// Decrease target life now
			if (other != null)
			{
				other.GetComponent<Health>().ChangeHealth(-reproductionLifeLoss);
			}
			else
			{
				Debug.LogWarning($"Partner died while breeding");
			}


			var r = ReflectionExtension.GetRange(GetType(), nameof(initialLife));
			childHost.initialLife = Mathf.Clamp(Mutate(initialLife, th.initialLife, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(initialSpeed));
			childHost.initialSpeed = Mathf.Clamp(Mutate(initialSpeed, th.initialSpeed, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(randomMovementRange));
			childHost.randomMovementRange = Mathf.Clamp(Mutate(randomMovementRange, th.randomMovementRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(sightRange));
			childHost.sightRange = Mathf.Clamp(Mutate(sightRange, th.sightRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(eatRange));
			childHost.eatRange = Mathf.Clamp(Mutate(eatRange, th.eatRange, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(metabolism));
			childHost.metabolism = Mathf.Clamp(Mutate(metabolism, th.metabolism, 1f), r.min, r.max);

			r = ReflectionExtension.GetRange(GetType(), nameof(robustness));
			childHost.robustness = Mathf.Clamp(Mutate(robustness, th.robustness, 1f), r.min, r.max);

			// go.GetComponent<MeshFilter>().mesh.Mutation();

			// TODO: the new host should have its memes tweaked by meme controller (mutation ...)
			LastBreed = Time.time;
		}

		private void OnDestinationChanged(Vector3 obj)
		{
			Mcm.instance.RpcAsync(new Packet
			{
				NavMeshUpdate = new NavMeshUpdate
				{
					Id = id,
					Destination = obj.Net()
				}
			});
		}


		public override void BringToLife()
		{
			controller.SetupAi(memes["Wander"], true, decisionFrequency);
		}

		#region Actions
		protected void RandomMovement(MemeController _)
		{
			if (movement.remainingDistance <= movement.stoppingDistance + 1)
			{
				// Try to find a random position on map, otherwise will just go to zero
				var p = transform.position.RandomPositionAroundAboveGroundWithDistance(randomMovementRange,
					default,
					0);
				movement.MoveTo(p);
			}
		}

		#endregion

		#region Transitions

		private Meme Timeout(MemeController c)
		{
			// Could be improved
			return c.lastTransition + 10f > Time.time ? memes["Wander"] : null;
		}

		#endregion
	}
}
