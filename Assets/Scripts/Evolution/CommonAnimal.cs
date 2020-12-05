using AI;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Api.Utils;
using Gameplay;
using UnityEngine;
using Utils;
using Meme = AI.Meme;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Evolution
{
	[RequireComponent(typeof(Movement))]
	public class CommonAnimal : Host
	{
		[HideInInspector] public Movement movement;
        protected int _foodLayer;
        // No sex between different species allowed
        protected int _specieLayer;
        public bool isCarnivorous;

        protected override void OnDeath()
        {
            Hm.instance.DestroyAnimalSync(id);
        }
        
        public override bool CanBreed()
        {
            // Scaling between 10 and 310 second the reproduction delay
            // Plus hard-coded probability (90% fail to reproduce)
            var luckyNewBorn = 100*Random.value < characteristics.ReproductionProbability;
            var canBreedAgain = Time.time - LastBreed > 10 + 300 * (characteristics.ReproductionDelay / 100);
            var enoughEnergy = characteristics.Energy > characteristics.ReproductionCost;
            // Debug.Log($"luckyNewBorn {luckyNewBorn} e {characteristics.Energy} {canBreedAgain}");
            return luckyNewBorn && canBreedAgain && enoughEnergy;
        }
        
        protected void BreedAndMutate(GameObject other)
		{
			var th = other.GetComponent<CommonAnimal>();
            if (th == null) return; // TODO: need to investigate this ? Died while breeding ?
			// Stop moving
			movement.isStopped = true;

			// Spawning a child around
			// var p = (transform.position + Random.insideUnitSphere * 10).AboveGround();
			var childHost = Hm.instance.SpawnAnimalSync(transform.position, Quaternion.identity, 
                characteristics, characteristicsMin, characteristicsMax);
            childHost.characteristics.Mutate(th.characteristics, 
                characteristics, 
                characteristicsMin, 
                characteristicsMax, // Description and Parser are protobuf garbage
                new []{"Descriptor", "Parser", "Carnivorous", "ReproductionDelay", "Life", "Energy"});

            // It's costly to reproduce, proportional to animal age ?
            characteristics.Energy -= characteristics.ReproductionCost;
            // health.ChangeHealth(-characteristics.ReproductionCost*(1+Age/100));
            if (other != null && th != null)
            {
                th.characteristics.Energy -= th.characteristics.ReproductionCost;
                // other.GetComponent<Health>().ChangeHealth(-th.characteristics.ReproductionCost*(1+th.Age/100));
            }
            else
            {
                Debug.LogWarning($"A partner died while breeding");
            }

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
                _foodLayer = isCarnivorous ? // TODO: LayerMask.get stuff return strange values so just going straight
                    LayerMask.NameToLayer("HerbivorousAnimal") : 
                    LayerMask.NameToLayer("Plant");
                _specieLayer = isCarnivorous ? 
                    LayerMask.NameToLayer("CarnivorousAnimal") : 
                    LayerMask.NameToLayer("HerbivorousAnimal");
                movement = GetComponent<Movement>();
                movement.navMeshAgent.enabled = true;
                // TODO: how costly is it to cast everytime ?
                movement.speed = characteristics.AnimalCharacteristics.Speed;
                if (Sm.instance && Sm.instance.isServer)
                {
                    movement.destinationChanged += OnDestinationChanged;
                }
            }
            else
            {
                movement.navMeshAgent.enabled = false;
            }
        }

		#region Actions
		protected void RandomMovement(MemeController _)
		{
			if (movement.remainingDistance <= movement.stoppingDistance + 1)
			{
				// Try to find a random position on map, otherwise will just go to zero
				var p = transform.position.RandomPositionAroundAboveGroundWithDistance(
                    characteristics.AnimalCharacteristics.RandomMovementRange,
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
