using AI;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Api.Utils;
using UnityEngine;
using Utils;
using Meme = AI.Meme;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Evolution
{
	[RequireComponent(typeof(Movement))]
	public class CommonAnimal : Host<AnimalCharacteristics>
	{
		[HideInInspector] public Movement movement;

        private void OnDied()
		{
			Hm.instance.DestroyAnimalSync(id);
		}

        protected void BreedAndMutate(GameObject other)
		{
			var th = other.GetComponent<CommonAnimal>();

			// Stop moving
			movement.isStopped = true;
			// It's costly to reproduce, proportional to animal age
			health.ChangeHealth(-characteristics.reproductionLifeLoss*(1+Age/100));

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
				other.GetComponent<Health>().ChangeHealth(-characteristics.reproductionLifeLoss);
			}
			else
			{
				Debug.LogWarning($"Partner died while breeding");
			}

            // Make a copy of the scriptable object to avoid serializing runtime changes
            childHost.characteristics = Instantiate(th.characteristics);
            childHost.characteristics.Mutate(th.characteristics, characteristics);
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

        public new void EnableBehaviour(bool value)
		{
            base.EnableBehaviour(value);
            if (value)
            {
                movement = GetComponent<Movement>();
                movement.navMeshAgent.enabled = true;
                // TODO: how costly is it to cast everytime ?
                movement.speed = characteristics.initialSpeed;
                if (Sm.instance && Sm.instance.isServer)
                {
                    health.Died += OnDied;
                    movement.destinationChanged += OnDestinationChanged;
                }
            }
            else
            {
                movement.navMeshAgent.enabled = false;
                if (Sm.instance && Sm.instance.isServer)
                {
                    health.Died -= OnDied;
                }
            }
        }

		#region Actions
		protected void RandomMovement(MemeController _)
		{
			if (movement.remainingDistance <= movement.stoppingDistance + 1)
			{
				// Try to find a random position on map, otherwise will just go to zero
				var p = transform.position.RandomPositionAroundAboveGroundWithDistance(
                    characteristics.randomMovementRange,
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
			return c.lastTransition + 10f > Time.time ? Memes["Wander"] : null;
		}

		#endregion
	}
}
