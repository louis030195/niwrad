using System;
using System.Dynamic;

namespace StateMachine
{
	public class Transition
	{
		private string m_Name;
		private readonly int m_Priority;
		private readonly Func<MemeController, Meme> m_Func;

		public string name => m_Name;
		public int priority => m_Priority;

		public Transition(string name, int priority, Func<MemeController, Meme> func)
		{
			m_Name = name;
			m_Priority = priority;
			m_Func = func;
		}

		public (int priority, Meme meme) Invoke(MemeController memeController)
		{
			return (priority: m_Priority, meme: m_Func.Invoke(memeController));
		}
	}
}
