using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;
using Utils;
using Action = AI.Action;
using Vector3 = UnityEngine.Vector3;


namespace Evolution
{
    [RequireComponent(typeof(Movement))]
    public class ComplexAnimal : CommonAnimal
    {
        private Sense<List<GameObject>> m_VisionSense;
        private Memory<GameObject> m_VisionMemory;

        public new void EnableBehaviour(bool value)
        {
            base.EnableBehaviour(value);
            if (value)
            {
                // An animal see around him, possibly several objects
                m_VisionSense = new Sense<List<GameObject>>();
                var res = new Collider[100];
                Func<List<GameObject>> collector = () =>
                {
                    Physics.OverlapSphereNonAlloc(transform.position,
                        characteristics.AnimalCharacteristics.SightRange,
                        res,
                        LayerMask.GetMask("Animal", "Vegetation"));
                    return res.Where(c => c != null).Select(c => c.gameObject).ToList();
                };
                m_VisionSense.ListenTo(collector);

                // It append the seen data in current memory
                m_VisionMemory = new Memory<GameObject>();
                m_VisionSense.Triggered += o => m_VisionMemory.Input(o);


                // Collect data before update only
                controller.BeforeUpdated += () =>
                {
                    m_VisionSense.Update();
                    m_VisionMemory.Update();
                };

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
            }
            else
            {
                // ?
            }
        }

        #region Actions

        private void ReachFood(MemeController c)
        {
            if (movement.remainingDistance <= movement.stoppingDistance)
            {
                var closest = m_VisionMemory.Query().Closest(transform.position,
                    LayerMask.NameToLayer("Vegetation"));
                if (closest != default) movement.MoveTo(closest.transform.position);
            }
        }

        private void ReachPartner(MemeController c)
        {
            if (movement.remainingDistance <= movement.stoppingDistance)
            {
                var closest = m_VisionMemory.Query().Closest(transform.position,
                    LayerMask.NameToLayer("Animal"));
                if (closest != default) movement.MoveTo(closest.transform.position);
            }
        }

        private void Eat(MemeController c)
        {
            // Stop moving
            movement.isStopped = true;
            var closest = m_VisionMemory.Query().Closest(transform.position,
                LayerMask.NameToLayer("Vegetation"));
            if (closest == default) return;
            attack.EatTarget(closest);
            closest.GetComponent<Health>().ChangeHealth(-Time.deltaTime * 30); // TODO: store params
            // +metabolism (10) *Time.deltaTime*0.5f // seems balanced
            // TODO: maybe age reduce life gain on eat ?
            health.ChangeHealth(+characteristics.AnimalCharacteristics.Metabolism * Time.deltaTime * 50f);
        }

        private void Reproduce(MemeController c)
        {
            var closest = m_VisionMemory.Query().Closest(transform.position,
                LayerMask.NameToLayer("Animal"));
            if (closest == default) return;
            BreedAndMutate(closest);
        }

        #endregion

        #region Transitions

        private Meme FoodAround(MemeController c)
        {
            // TODO: params
            if (health.currentHealth > 90f) return Memes["Wander"];

            var closest = m_VisionMemory.Query().Closest(transform.position,
                LayerMask.NameToLayer("Vegetation"));

            // No food around OR target is dead / too weak
            if (closest == default || closest.GetComponent<Health>().dead) return null;

            // Stop current movement
            // movement.navMeshAgent.destination = transform.position;

            return Memes["ReachFood"];
        }

        private Meme PartnerAround(MemeController c)
        {
            // Look for partner
            if (health.currentHealth > characteristics.ReproductionCost)
            {
                var closest = m_VisionMemory.Query().Closest(transform.position,
                    LayerMask.NameToLayer("Animal"));
                // TODO: closest with enough life to breed
                // No animal to breed with around
                if (closest == default) return null;

                // Stop current movement
                // movement.navMeshAgent.destination = transform.position;

                return Memes["ReachPartner"];
            }

            return null;
        }

        private Meme IsCloseEnoughForEating(MemeController c)
        {
            var closest = m_VisionMemory.Query().Closest(transform.position,
                LayerMask.NameToLayer("Vegetation"));
            if (closest != default && Vector3.Distance(transform.position, closest.transform.position) <
                characteristics.AnimalCharacteristics.EatRange)
                return Memes["Eat"];
            return null;
        }

        private Meme IsCloseEnoughForBreeding(MemeController c)
        {
            var closest = m_VisionMemory.Query().Closest(transform.position,
                LayerMask.NameToLayer("Animal"));
            if (closest != default && Vector3.Distance(transform.position, closest.transform.position) < 1)
                return Memes["Breed"];
            return null;
        }


        private Meme IsTargetAlive(MemeController c)
        {
            var closest = m_VisionMemory.Query().Closest(transform.position,
                LayerMask.NameToLayer("Animal"));

            return closest != default && closest.GetComponent<Health>().dead ? Memes["Wander"] : null;
        }

        private Meme Timeout(MemeController c)
        {
            // Could be improved
            return c.lastTransition + 10f > Time.time ? Memes["Wander"] : null;
        }

        #endregion
    }
}
