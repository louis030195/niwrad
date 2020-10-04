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
        
  //       protected new void Update()
		// {
		// 	// Sun gives life (maybe could multiply by "sun intensity here")
		// 	// health.ChangeHealth(Time.deltaTime*(1-Age/characteristics.Robustness));
		// 	// health.dead = !(health.currentHealth > characteristics.Life);
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
			// return; // off now
			// There is a probability of reproduction
			// if (Random.value * 100 > reproductionProbability) return;
			//
			// // Spawning a child around
			// var p = transform.position.RandomPositionAroundAboveGroundWithDistance(reproductionSprayRadius,
			// 	LayerMask.GetMask("Plant"),
			// 	reproductionDistanceBetween);
			// // Couldn't find free position
			// if (p == Vector3.zero) return;
			// var childHost = Hm.instance.SpawnTreeSync(p, Quaternion.identity);
			// var mutate = new Func<float, float, float>((a, mutationDegree) =>
			// {
			// 	var md = Mathf.Abs(mutationDegree) > 1 ? 1 : Mathf.Abs(mutationDegree);
			// 	return a * (1 + Random.Range(-md, md));
			// });
			// var r = ReflectionExtension.GetRange(GetType(), nameof(initialLife));
			// childHost.initialLife = Mathf.Clamp(mutate(initialLife, 1f), r.min, r.max);
			//
			// r = ReflectionExtension.GetRange(GetType(), nameof(robustness));
			// childHost.robustness = Mathf.Clamp(mutate(robustness, 1f), r.min, r.max);
			//
			// // go.GetComponent<MeshFilter>().mesh.Mutation();
			//
			// // TODO: the new host should have its memes tweaked by meme controller (mutation ...)
			// LastBreed = Time.time;
		}
		#endregion

		#region Transitions

		private Meme CanBreed(MemeController c)
		{
			// if (health.currentHealth > characteristics.ReproductionCost)
			// {
			// 	return Memes["Breed"];
			// }
			return Memes["Grow"];
		}

		#endregion
	}
}
