using UnityEngine;
using UnityEngine.UIElements;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/Actions/RandomMovement")]
	public class RandomMovementAction : Action
	{
		public int Radius = 10;
		public override void Act(StateController controller)
		{
			RandomMove(controller);
		}

		private void RandomMove(StateController controller)
		{
			if (controller.movement.remainingDistance <= controller.movement.stoppingDistance && !controller.movement.pathPending)
			{
				var p = controller.transform.position + Random.insideUnitSphere * Radius;
				p.y = controller.transform.position.y; // flat ?
				controller.movement.MoveTo(p);
			}

		}
	}
}
