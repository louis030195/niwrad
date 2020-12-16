using System;
using System.Collections.Generic;
using AI;
using UnityEngine;
using Utils;
using Utils.Physics;
using Action = AI.Action;
using Random = UnityEngine.Random;

namespace Evolution
{
	public class Plant : Host
	{
        // private void LateUpdate()
        // {
        //     health.dead = health.currentHealth < 40f; // TODO 
        // }

        protected override void OnDeath()
        {
            Hm.instance.DestroyPlantSync(id);
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

        public new void EnableBehaviour(bool value)
		{
            base.EnableBehaviour(value);
            if (value)
            {
                var n = "Grow";
                Memes[n] = new Meme(
                    n,
                    new List<Action>
                    {
                        new Action("Grow", Grow)
                    },
                    new List<Transition>
                    {
                        new Transition("CanBreed", 0, Breed)
                    },
                    Color.green
                );
                n = "Breed";
                Memes[n] = new Meme(
                    n,
                    new List<Action>
                    {
                        new Action("Reproduce", Reproduce)
                    },
                    new List<Transition>
                    {
                        new Transition("CanBreed", 0, Breed)
                    },
                    Color.magenta
                );
                controller.SetupAi(Memes["Grow"], 100/characteristics.Computation);
            }
            else
            {
                // ?
            }
        }

		#region Actions
		private void Grow(MemeController c)
        {
            characteristics.Energy += characteristics.EatEnergyGain; // Feed from sun ?
        }
		private void Reproduce(MemeController c)
        {
            if (!CanBreed()) return;
			
			// Spawning a child around
			var p = transform.position.Spray(5,
				LayerMask.GetMask("Plant"),
				3);
			// Couldn't find free position
			if (p.Equals(Vector3.positiveInfinity)) return;
			var childHost = Hm.instance.SpawnPlantSync(p, 
                Quaternion.identity,
                characteristics,
                characteristicsMin,
                characteristicsMax);
            characteristics.Energy -= characteristics.ReproductionCost;

            // TODO: mutate should handle asexual reproduction
			childHost.characteristics.Mutate(characteristics, 
                characteristics, 
                characteristicsMin, 
                characteristicsMax,
                new []{"Descriptor", "Parser", "Carnivorous", "ReproductionDelay", "Life", "Energy"});
			LastBreed = Time.time;
		}
		#endregion

		#region Transitions

		private Meme Breed(MemeController c) => CanBreed() ? Memes["Breed"] : Memes["Grow"];

		#endregion
	}
}
