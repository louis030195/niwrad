using System;
using System.Collections.Generic;
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
		private readonly List<Action> m_Actions;

		/// <summary>
		/// Conditions to transit to other states, passing current observation
		/// </summary>
		private readonly List<Transition> m_Transitions;

		public Color SceneGizmoColor = Color.grey;
		public string Name;

		public Meme(string name, List<Action> actions, List<Transition> transitions)
		{
			m_Actions = actions;
			m_Transitions = transitions;
			Name = name;
		}

		public Meme(string name, List<Action> actions, List<Transition> transitions, Color sceneGizmoColor)
		{
			m_Actions = actions;
			m_Transitions = transitions;
			SceneGizmoColor = sceneGizmoColor;
			Name = name;
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
				t.Invoke(controller);
			}
		}

		private void CheckTransitions(MemeController controller)
		{
			var minPm = (i: int.MaxValue, m: new Meme("", null, null));
			// For each transitions, invoke the decision function
			// Get the transition with the minimum priority (reversed)
			// And pass the most prioritized meme
			foreach (var t in m_Transitions)
			{
				var pm = t.Invoke(controller);
				if (pm.meme == null) continue;
				if (minPm.i == int.MaxValue || pm.priority < t.priority)
				{
					minPm = pm;
				}
			}

			// No valid transition, stay in the same meme
			if (minPm.i != int.MaxValue) controller.Transition(minPm.m);
		}
	}
}
