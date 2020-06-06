using System;
using System.Collections;

namespace StateMachine
{
	public class Action
	{
		private string m_Name;
		private readonly Action<MemeController> m_Func;
		public event Action<Action> acted;

		public Action(string name, Action<MemeController> func)
		{
			m_Name = name;
			m_Func = func;
		}

		public void Invoke(MemeController memeController)
		{
			m_Func.Invoke(memeController);
			acted?.Invoke(this);
		}
	}
}
