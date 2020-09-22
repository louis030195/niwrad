using System;

namespace AI
{
	public class Transition
	{
        private readonly int _priority;
		private readonly Func<MemeController, Meme> _func;

		public string Name { get; }

        public int priority => _priority;

		public Transition(string name, int priority, Func<MemeController, Meme> func)
		{
			this.Name = name;
			_priority = priority;
			_func = func;
		}

		public (int priority, Meme meme) Invoke(MemeController memeController)
		{
			return (priority: _priority, meme: _func.Invoke(memeController));
		}
	}
}
