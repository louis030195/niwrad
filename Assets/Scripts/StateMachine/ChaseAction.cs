using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/Actions/Chase")]
	public class ChaseAction : Action
	{
		public bool ally;
		public override void Act(StateController controller)
		{
			Chase(controller);
		}

		private void Chase(StateController controller)
		{
			if (!ally)
			{
				if (controller.target)
				{
					Debug.DrawLine(controller.transform.position, controller.target.GetComponent<Collider>().ClosestPoint(controller.transform.position));
					controller.movement.MoveTo(controller.target.position);
				}
			}
			else
			{
				if (controller.master)
				{
					Debug.DrawLine(controller.transform.position, controller.master.GetComponent<Collider>().ClosestPoint(controller.transform.position));
					controller.movement.MoveTo(controller.master.position);
				}
			}
		}
	}
}
