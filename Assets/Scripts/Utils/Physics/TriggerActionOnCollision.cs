using System;
using UnityEngine;

namespace Utils.Physics
{
    /// <summary>
    /// Code only based (no need to define OnCollisionEnter handler for each object ...)
    /// collision detection component to attach to any object
    /// </summary>
    public class TriggerActionOnCollision : MonoBehaviour
    {
        private Action _actionToTrigger;
        private string _tagToCollideWith;

        public void CollisionEnter(Action action, string tagToCollideWith)
        {
            _actionToTrigger = action;
            _tagToCollideWith = tagToCollideWith;
        }
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag(_tagToCollideWith))
            {
                _actionToTrigger?.Invoke();
            }
        }
    }
}
