using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/Actions/Attack")]
	public class AttackAction : Action
	{
		public override void Act(StateController controller)
		{
			Attack(controller);
		}

		private void Attack(StateController controller)
		{
			controller.movement.isStopped = true;
			if(controller.target != null) // This if shouldnt be needed because of ActivateStateDecision already checking but idk
				controller.attack.AttackTarget(controller.target.GetComponent<Collider>().ClosestPointOnBounds(controller.transform.position));
		}
	}
}
