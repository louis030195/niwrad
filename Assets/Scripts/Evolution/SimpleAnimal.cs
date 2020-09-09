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
        private GameObject m_Target;


        public new void EnableBehaviour(bool value)
        {
            base.EnableBehaviour(value);
            if (value)
            {
                var foodAround = new Transition("FoodAround", 0, FoodAround);
                var partnerAround = new Transition("PartnerAround", -2, PartnerAround);
                var timeout = new Transition("Timeout", -1, Timeout);
                var n = "Wander";
                Memes[n] = new Meme(
                    n,
                    new List<Action>
                    {
                        new Action("RandomMovement", RandomMovement)
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
                        new Action("ReachFood", ReachFood)
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
                        new Action("ReachPartner", ReachPartner)
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
                        // Reach,
                        new Action("Eat", Eat)
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
                        new Action("Reproduce", Reproduce)
                        // Reach,
                    },
                    new List<Transition>
                    {
                        foodAround,
                        partnerAround
                    },
                    Color.magenta
                );
                controller.SetupAi(Memes["Wander"], characteristics.decisionFrequency);
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
            movement.MoveTo(m_Target.transform.position);
            // }
        }

        private void ReachPartner(MemeController c)
        {
            // if (movement.remainingDistance <= movement.stoppingDistance)
            // {
            movement.MoveTo(m_Target.transform.position);
            // }
        }

        private void Eat(MemeController c)
        {
            // Stop moving
            movement.isStopped = true;
            attack.EatTarget(m_Target);
            m_Target.GetComponent<Health>().ChangeHealth(-Time.deltaTime * characteristics.hunger); // TODO: store params
            // +metabolism (10) *Time.deltaTime*0.5f // seems balanced
            // TODO: maybe age reduce life gain on eat ?
            health.ChangeHealth(+characteristics.metabolism * Time.deltaTime * 50f);
        }

        private void Reproduce(MemeController c)
        {
            BreedAndMutate(m_Target);
        }

        #endregion

        #region Transitions

        private Meme FoodAround(MemeController c)
        {
            // TODO: params
            if (health.currentHealth > 90f) return Memes["Wander"];

            var layerMask = 1 << LayerMask.NameToLayer("Vegetation");

            // Any matching object around ? Try to get the closest if any
            var closest = gameObject.Closest(characteristics.sightRange, layerMask);

            // No food around OR target is dead / too weak
            if (closest == default || closest.GetComponent<Health>().dead) return null;
            m_Target = closest;

            // Stop current movement
            // movement.navMeshAgent.destination = transform.position;

            return Memes["ReachFood"];
        }

        private Meme PartnerAround(MemeController c)
        {
            // Look for partner
            if (Time.time > LastBreed + characteristics.reproductionDelay && health.currentHealth >
                characteristics.reproductionThreshold)
            {
                // TODO: closest with enough life to breed
                // No animal to breed with around
                var layerMask = 1 << LayerMask.NameToLayer("Animal");

                // Any matching object around ? Try to get the closest if any
                var closest = gameObject.Closest(characteristics.sightRange, layerMask);
                // No animal to breed with around
                if (closest == default) return null;
                m_Target = closest;

                // Stop current movement
                // movement.navMeshAgent.destination = transform.position;

                return Memes["ReachPartner"];
            }

            return null;
        }

        private Meme IsCloseEnoughForEating(MemeController c)
        {
            return Vector3.Distance(transform.position, m_Target.transform.position) <
                   characteristics.eatRange
                ? Memes["Eat"]
                : null;
        }

        private Meme IsCloseEnoughForBreeding(MemeController c)
        {
            return Vector3.Distance(transform.position, m_Target.transform.position) <
                   1
                ? Memes["Breed"]
                : null;
        }


        private Meme IsTargetAlive(MemeController c)
        {
            return m_Target.GetComponent<Health>().dead ? Memes["Wander"] : null;
        }

        private Meme Timeout(MemeController c)
        {
            // Could be improved
            return c.lastTransition + 10f > Time.time ? Memes["Wander"] : null;
        }

        #endregion
    }
}
