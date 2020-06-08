using System.Collections.Generic;
using UnityEngine;

namespace AI
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
			if (m_Transitions.Count == 0)
			{
				Debug.LogError($"This meme: {Name} has not transitions !");
				return;
			}
			var minPm = m_Transitions[0].Invoke(controller);
			// For each transitions, invoke the decision function
			// Get the transition with the lowest priority it is the one which will be ran
			for (var i = 1; i < m_Transitions.Count; i++)
			{
				var pm = m_Transitions[i].Invoke(controller);
				// If this transition doesn't request a change, keep iterating
				if (pm.meme == null) continue;

				// Otherwise if we found a more important transition, pick the minimum one
				if (pm.priority < minPm.priority)
				{
					minPm = pm;
				}
			}

			// Transition to the picked meme if it's valid
			if (minPm.meme != null) controller.Transition(minPm.meme);
		}
	}
}
