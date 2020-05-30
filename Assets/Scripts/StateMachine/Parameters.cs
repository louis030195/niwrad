using UnityEngine;

namespace StateMachine
{
	[CreateAssetMenu(menuName = "StateMachine/Parameters")]
	public class Parameters : ScriptableObject
	{
		[Range(1, 100)] public float lookSphereCastRadius = 10;
		[Range(1, 20)] public float lookRange = 10;
		[Range(1, 2)] public float attackRange = 1;
		[Range(5, 20)] public float moveSpeed = 10;
		[Range(5, 20)] public float searchingTurnSpeed = 10;
		[Range(5, 20)] public float searchDuration = 10;
		[Range(5, 20)] public float timeOut = 10;
	}
}
