using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/State")]
	public class State : ScriptableObject
	{

		public Action[] actions;
		public Transition[] transitions;
		public Color sceneGizmoColor = Color.grey;

		public void UpdateState(StateController controller)
		{
			DoActions(controller);
			CheckTransitions(controller);
		}

		private void DoActions(StateController controller)
		{
			foreach (var t in actions)
			{
				t.Act(controller);
			}
		}

		private void CheckTransitions(StateController controller)
		{
			foreach (var t in transitions)
			{
				controller.TransitionToState(t.decision.Decide(controller) ? t.trueState : t.falseState);
			}
		}


	}
}
