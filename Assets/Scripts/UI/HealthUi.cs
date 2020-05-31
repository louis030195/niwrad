using Evolution;
using UnityEngine;

namespace UI
{
	[RequireComponent(typeof(Health))]
	public class HealthUi : MonoBehaviour
	{
		private void Awake()
		{
			GetComponent<Health>().HealthChanged += UpdateUi;
		}

		private void UpdateUi(float value)
		{
			var c =GetComponent<Renderer>().material.color;
			// Gray-ize the block to look less alive
			for (var i = 0; i < 3; i++)
			{
				c[i] = Mathf.Clamp(4.0f * value, 0.2f, 0.8f);
			}
		}
	}
}
