using System;

namespace AI
{
	public class Action
	{
		private string _name;
		private readonly Action<MemeController> _func;
		public event Action<Action> Acted;

		public Action(string name, Action<MemeController> func)
		{
			_name = name;
			_func = func;
		}

		public void Invoke(MemeController memeController)
		{
			_func?.Invoke(memeController);
			Acted?.Invoke(this);
		}
	}
}
