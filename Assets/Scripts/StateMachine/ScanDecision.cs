using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/Decisions/Scan")]
	public class ScanDecision : Decision
	{
		public override bool Decide(StateController controller)
		{
			return Scan(controller);
		}

		private bool Scan(StateController controller)
		{
			controller.movement.isStopped = true;
			controller.transform.Rotate(0, controller.parameters.searchingTurnSpeed * Time.deltaTime, 0);
			return controller.CheckIfCountDownElapsed(controller.parameters.searchDuration);
		}
	}
}
