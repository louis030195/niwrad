using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/Decisions/Look")]
	public class LookDecision : Decision
	{
		public override bool Decide(StateController controller)
		{
			return Look(controller);
		}

		private bool Look(StateController controller)
		{
			// the origin of the ray need to start a bit behind
			var castOrigin = controller.eyes.position - controller.eyes.forward * controller.parameters.lookSphereCastRadius;

			Debug.DrawRay(castOrigin,
				controller.eyes.forward.normalized * controller.parameters.lookRange, controller.currentState.sceneGizmoColor);

			return Physics.SphereCast(castOrigin, controller.parameters.lookSphereCastRadius,
				       controller.eyes.forward, out var hit, controller.parameters.lookRange) && HitTarget(controller, hit.collider);
		}
	}
}
