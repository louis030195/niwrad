using System;
using System.Collections.Generic;
using AI;
using UnityEngine;
using Utils;
using Utils.Physics;
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
                var isCloseEnoughForEating = new Transition("IsCloseEnoughForEating", 0, IsCloseEnoughForEating);
                var waterAround = new Transition("WaterAround", -1, WaterAround);
                var isCloseEnoughForDrinking = new Transition("IsCloseEnoughForDrinking", -1, IsCloseEnoughForDrinking);
                var partnerAround = new Transition("PartnerAround", -2, PartnerAround);
                var isCloseEnoughForBreeding = new Transition("IsCloseEnoughForBreeding", -1, IsCloseEnoughForBreeding);
                var isTargetAlive = new Transition("IsTargetAlive", 0, IsTargetAlive);
                var timeout = new Transition("Timeout", -1, Timeout);
                
                // Actions
                var randomMovement = new Action("RandomMovement", RandomMovement);
                var reachFood = new Action("ReachFood", Reach);
                var reachWater = new Action("ReachWater", Reach);
                var reachPartner = new Action("ReachPartner", Reach);
                var eat = new Action("Eat", Eat);
                var drink = new Action("Drink", Drink);
                var reproduce = new Action("Reproduce", Reproduce);

                var n = "Wander";
                Memes[n] = new Meme(n, new List<Action> {randomMovement}, new List<Transition>
                    {foodAround, waterAround, partnerAround}, Color.white);
                n = "ReachFood";
                Memes[n] = new Meme(n, new List<Action> {reachFood},
                    new List<Transition> {isCloseEnoughForEating}, Color.red);
                n = "ReachWater";
                Memes[n] = new Meme(n, new List<Action> {reachWater}, new List<Transition>
                    {isCloseEnoughForDrinking}, Color.red);
                n = "ReachPartner";
                Memes[n] = new Meme(n, new List<Action> {reachPartner}, new List<Transition>
                    {isCloseEnoughForBreeding}, Color.red);
                n = "Eat";
                Memes[n] = new Meme(n, new List<Action> {eat}, new List<Transition>
                    {isTargetAlive, partnerAround, waterAround}, Color.blue);
                n = "Drink";
                Memes[n] = new Meme(n, new List<Action> {drink}, new List<Transition>
                    {partnerAround}, Color.blue);
                n = "Breed";
                Memes[n] = new Meme(n, new List<Action> {reproduce}, new List<Transition>
                    {foodAround, waterAround, partnerAround}, Color.magenta);
                controller.SetupAi(Memes["Wander"], 100/characteristics.Computation);
            }
        }

        #region Actions

        private void Reach(MemeController c)
        {
            // if (movement.remainingDistance <= movement.stoppingDistance)
            // {
            movement.MoveTo(_target.transform.position);
            // }
        }

        private void Eat(MemeController c)
        {
            // Stop moving
            movement.isStopped = true;
            attack.EatTarget(_target);
            // The more energy, the more damage to food ?
            _target.GetComponent<Health>().AddHealth(-characteristics.Energy/10); // TODO: ?
            characteristics.Energy += characteristics.EatEnergyGain;
        }
        
        private void Drink(MemeController c)
        {
            // Stop moving
            movement.isStopped = true;
            characteristics.Energy += characteristics.DrinkEnergyGain;
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
        
        private Meme WaterAround(MemeController c)
        {
            // Any matching object around ? Try to get the closest if any
            var closest = gameObject.Closest(characteristics.AnimalCharacteristics.SightRange, WaterLayer);
            // No water around
            if (closest == default) return null;

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
        
        private Meme IsCloseEnoughForDrinking(MemeController c)
        {
            return Vector3.Distance(transform.position, _target.transform.position) <
                   characteristics.AnimalCharacteristics.EatRange
                ? Memes["Drink"]
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
