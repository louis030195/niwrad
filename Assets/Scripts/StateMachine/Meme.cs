using System;
using UnityEngine;

namespace StateMachine
{
	/// <summary>
	/// A meme is a system that follow darwinian evolution on the "behaviour side" i.e will affects host behaviour
	/// </summary>
	public class Meme
	{
		/// <summary>
		/// Actions to perform on this state, passing current observation
		/// </summary>
		private readonly Action<MemeController>[] m_Actions;

		/// <summary>
		/// Conditions to transit to other states, passing current observation
		/// </summary>
		private readonly Func<MemeController, Meme>[] m_Transitions;

		/// <summary>
		/// Debug utilitary
		/// </summary>
		public Color SceneGizmoColor = Color.grey;
		public event Action<Action<MemeController>> acted;

		public Meme(Action<MemeController>[] actions, Func<MemeController, Meme>[] transitions)
		{
			m_Actions = actions;
			m_Transitions = transitions;
		}

		public Meme(Action<MemeController>[] actions, Func<MemeController, Meme>[] transitions, Color sceneGizmoColor)
		{
			m_Actions = actions;
			m_Transitions = transitions;
			SceneGizmoColor = sceneGizmoColor;
		}

		public void UpdateState(MemeController controller)
		{
			DoActions(controller);
			CheckTransitions(controller);
		}

		private void DoActions(MemeController controller)
		{
			if (m_Actions == null) return;
			foreach (var t in m_Actions)
			{
				acted?.Invoke(t);
				t.Invoke(controller);
			}
		}

		private void CheckTransitions(MemeController controller)
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
