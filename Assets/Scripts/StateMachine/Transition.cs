namespace StateMachine
{
	[System.Serializable]
	public class Transition
	{
		public Decision decision;
		public State trueState;
		public State falseState;
	}
}
