using UnityEngine;

namespace StateMachine
{
	public abstract class Decision : ScriptableObject
	{
		public abstract bool Decide(StateController controller);

		/// <summary>
		/// From a raycasthit, will return if we hit a decision target
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="hit"></param>
		/// <returns></returns>
		protected bool HitTarget(StateController controller, Collider hit)
		{
			if (!hit.CompareTag("vegetation")) return false; // TODO: fix
			// If we hit something
			controller.target = hit.transform;
			return true;
		}
	}
}
