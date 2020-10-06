using System;
using System.Collections.Generic;
using AI;
using Gameplay;
using ProceduralTree;
using UnityEngine;
using Utils;
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
                        new Transition("CanBreed", 0, CanBreed)
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
                        new Transition("CanBreed", 0, CanBreed)
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
            const float reproductionProbability = 0.1f;
			// There is a probability of reproduction
			if (Random.value > reproductionProbability) return;
			
			// Spawning a child around
			var p = transform.position.RandomPositionAroundAboveGroundWithDistance(5,
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
			childHost.characteristics.Mutate(characteristics, characteristics, characteristicsMin, characteristicsMax);
			LastBreed = Time.time;
		}
		#endregion

		#region Transitions

		private Meme CanBreed(MemeController c)
        {
            return characteristics.Energy > characteristics.ReproductionCost && health.currentHealth > 50 ? Memes["Breed"] : Memes["Grow"];
        }

		#endregion
	}
}
