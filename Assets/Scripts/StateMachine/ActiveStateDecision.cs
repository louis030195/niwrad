using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/Decisions/ActiveState")]
	public class ActiveStateDecision : Decision
	{
		public override bool Decide(StateController controller)
		{
			return controller.target != null;
		}
	}
}
