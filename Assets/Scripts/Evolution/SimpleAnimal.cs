using System;
using System.Collections.Generic;
using AI;
using UnityEngine;
using Utils;
using Action = AI.Action;
using Vector3 = UnityEngine.Vector3;

namespace Evolution
{
    [RequireComponent(typeof(Movement))]
    public class SimpleAnimal : CommonAnimal
    {
        private GameObject _target;

        public new void EnableBehaviour(bool value)
        {
            base.EnableBehaviour(value);
            if (value)
            {
                // Transitions
                var foodAround = new Transition("FoodAround", 0, FoodAround);
                var partnerAround = new Transition("PartnerAround", -2, PartnerAround);
                var timeout = new Transition("Timeout", -1, Timeout);
                
                // Actions
                var randomMovement = new Action("RandomMovement", RandomMovement);
                var reachFood = new Action("ReachFood", ReachFood);
                var reachPartner = new Action("ReachPartner", ReachPartner);
                var eat = new Action("Eat", Eat);
                var reproduce = new Action("Reproduce", Reproduce);

                var n = "Wander";
                Memes[n] = new Meme(
                    n,
                    new List<Action>
                    {
                        randomMovement
                    },
                    new List<Transition>
                    {
                        foodAround,
                        partnerAround
                    },
                    Color.white
                );
                n = "ReachFood";
                Memes[n] = new Meme(
                    n,
                    new List<Action>
                    {
                        reachFood
                    },
                    new List<Transition>
                    {
                        new Transition("IsCloseEnoughForEating", 0, IsCloseEnoughForEating),
                        // timeout
                    },
                    Color.red
                );
                n = "ReachPartner";
                Memes[n] = new Meme(
                    n,
                    new List<Action>
                    {
                        reachPartner
                    },
                    new List<Transition>
                    {
                        new Transition("IsCloseEnoughForBreeding", -1, IsCloseEnoughForBreeding),
                        // timeout
                    },
                    Color.red
                );
                n = "Eat";
                Memes[n] = new Meme(
                    n,
                    new List<Action>
                    {
                        eat
                    },
                    new List<Transition>
                    {
                        new Transition("IsTargetAlive", 0, IsTargetAlive),
                        partnerAround // While eating, if it can breed, go for it
                    },
                    Color.blue
                );
                n = "Breed";
                Memes[n] = new Meme(
                    n,
                    new List<Action>
                    {
                        reproduce
                    },
                    new List<Transition>
                    {
                        foodAround,
                        partnerAround
                    },
                    Color.magenta
                );
                controller.SetupAi(Memes["Wander"], 100/characteristics.Computation);
            }
            else
            {
                // ?
            }
        }

        #region Actions

        private void ReachFood(MemeController c)
        {
            // if (movement.remainingDistance <= movement.stoppingDistance)
            // {
            movement.MoveTo(_target.transform.position);
            // }
        }

        private void ReachPartner(MemeController c)
        {
            // if (movement.remainingDistance <= movement.stoppingDistance)
            // {
            movement.MoveTo(_target.transform.position);
            // }
        }

        private void Eat(MemeController c)
        {
            // Debug.Log($"i carnivorous {isCarnivorous} eat animal: {_target.GetComponent<SimpleAnimal>() != null}");

            // Stop moving
            movement.isStopped = true;
            attack.EatTarget(_target);
            // The more energy, the more damage to food ?
            _target.GetComponent<Health>().AddHealth(-characteristics.Energy/10); // TODO: ?
            // +metabolism (10) *Time.deltaTime*0.5f // seems balanced
            // TODO: maybe age reduce life gain on eat ?
            characteristics.Energy += characteristics.EatEnergyGain;
            // health.ChangeHealth(+characteristics.AnimalCharacteristics.Metabolism * Time.deltaTime * 50f);
        }

        private void Reproduce(MemeController c)
        {
            BreedAndMutate(_target);
        }

        #endregion

        #region Transitions

        private Meme FoodAround(MemeController c)
        {
            // TODO: params
            if (health.currentHealth > 90f) return null;

            // Any matching object around ? Try to get the closest if any
            var closest = gameObject.Closest(characteristics.AnimalCharacteristics.SightRange, _foodLayer/*, 
                filter: go => !go.GetComponent<Health>().dead*/);
            // No food around OR target is dead / too weak
            if (closest == default) return null;
            // Debug.Log($"found food {closest}");

            _target = closest;

            // Stop current movement
            // movement.navMeshAgent.destination = transform.position;

            return Memes["ReachFood"];
        }

        private Meme PartnerAround(MemeController c)
        {
            // Look for partner
            if (!CanBreed()) return null;
            // Any matching object around ? Try to get the closest if any
            var closest = gameObject.Closest(characteristics.AnimalCharacteristics.SightRange, _specieLayer/*,
                filter: go => go.GetComponent<SimpleAnimal>().characteristics.Energy > characteristics.ReproductionCost*/);
            // No animal to breed with around, TODO: move the canbreed filter to Closest()
            if (closest == default || !closest.GetComponent<Host>().CanBreed()) return null;
            _target = closest;

            // Stop current movement
            // movement.navMeshAgent.destination = transform.position;

            return Memes["ReachPartner"];

        }

        private Meme IsCloseEnoughForEating(MemeController c)
        {
            return Vector3.Distance(transform.position, _target.transform.position) <
                   characteristics.AnimalCharacteristics.EatRange
                ? Memes["Eat"]
                : null;
        }

        private Meme IsCloseEnoughForBreeding(MemeController c)
        {
            return Vector3.Distance(transform.position, _target.transform.position) <
                   1
                ? Memes["Breed"]
                : null;
        }


        private Meme IsTargetAlive(MemeController c)
        {
            return _target.GetComponent<Health>().dead ? Memes["Wander"] : null;
        }

        private Meme Timeout(MemeController c)
        {
            // Could be improved
            return c.lastTransition + 10f > Time.time ? Memes["Wander"] : null;
        }

        #endregion
    }
}
