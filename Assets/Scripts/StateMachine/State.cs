using System;
using UnityEngine;

namespace StateMachine
{
	public class State
	{
		/// <summary>
		/// Actions to perform on this state, passing current observation
		/// </summary>
		private readonly Action<StateController>[] m_Actions;

		/// <summary>
		/// Conditions to transit to other states, passing current observation
		/// </summary>
		private readonly Func<StateController, State>[] m_Transitions;

		/// <summary>
		/// Debug utilitary
		/// </summary>
		public Color SceneGizmoColor = Color.grey;
		public event Action<Action<StateController>> acted;

		public State(Action<StateController>[] actions, Func<StateController, State>[] transitions)
		{
			m_Actions = actions;
			m_Transitions = transitions;
		}

		public State(Action<StateController>[] actions, Func<StateController, State>[] transitions, Color sceneGizmoColor)
		{
			m_Actions = actions;
			m_Transitions = transitions;
			SceneGizmoColor = sceneGizmoColor;
		}

		public void UpdateState(StateController controller)
		{
			DoActions(controller);
			CheckTransitions(controller);
		}

		private void DoActions(StateController controller)
		{
			if (m_Actions == null) return;
			foreach (var t in m_Actions)
			{
				acted?.Invoke(t);
				t.Invoke(controller);
			}
		}

		private void CheckTransitions(StateController controller)
		{
			// For each transitions, invoke the decision function and accordingly change state
			foreach (var t in m_Transitions)
			{
				var next = t.Invoke(controller);
				if (next == null) continue;
				controller.TransitionToState(next);
				return;
			}
		}


	}
}
