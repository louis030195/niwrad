using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/Actions/Flee")]
	public class FleeAction : Action
	{
		public override void Act(StateController controller)
		{
			Flee(controller);
		}

		private void Flee(StateController controller)
		{
			try
			{
				var pos = controller.target.position - controller.transform.position;
				controller.movement.MoveTo(pos);
			}
			catch (Exception e)
			{
				// ignored
			}
		}
	}
}
