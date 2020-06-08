using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evolution
{
	/// <summary>
	/// Senses collect surrounding data and trigger events with these.
	/// Senses should be dynamic, e.g. be removed, added to objects over time and their parameters evolve.
	/// For example vision on human is different than vision on fly ...
	/// </summary>
	public class Sense<T>
	{
		private Func<T> m_Sense;
		public event Action<T> Triggered;

		/// <summary>
		/// You can subscribe to a sense giving a function to be ran every frame that will return
		/// either null: ignored, either some data, then triggered into the Triggered event.
		/// var sub = myVisionSense.Sub(() => Physics.Raycast(Vector3.zero,
		/// 		Vector3.up,
		/// 		out var hit,
		/// 		Mathf.Infinity)
		/// 	? hit.collider.gameObject
		/// 	: null;
		/// )
		/// myVisionSense.Triggered += somethingAboveMe => Debug.Log($"Wow there is {somethingAboveMe.name} above me !!");
		/// </summary>
		/// <param name="sense"></param>
		/// <returns></returns>
		public void ListenTo(Func<T> sense)
		{
			m_Sense = sense;
		}

		public void Update()
		{
			var detected = m_Sense.Invoke();
			if (detected != null) Triggered?.Invoke(detected);
		}
	}
}
