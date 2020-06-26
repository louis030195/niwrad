using Unity.Entities;

namespace AI.Components
{
	public struct MemeComponent : IComponentData
	{
		private float m_DecisionFrequency;
		private float m_LastDecision;
		private Meme m_CurrentMeme;
		public bool aiActive;
		public float lastTransition;
		// public event Action<Meme> MemeChanged;
		public event System.Action BeforeUpdated;
	}
}
