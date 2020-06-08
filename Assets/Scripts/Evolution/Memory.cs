using System.Collections.Generic;
using UnityEngine;

namespace Evolution
{
	/// <summary>
	/// Memory role is to listen for data, store it and forget regularly.
	/// </summary>
	public class Memory<T>
	{
		// How long the data should be kept
		private const float Retention = 50f;

		// TODO: it could have been a stack but we lose the quick search advantages
		private readonly List<(float time, T data)> m_Memories = new List<(float, T)>();

		/// <summary>
		/// A memory receive input(s) that are stored according to their time of recording
		/// </summary>
		/// <param name="input"></param>
		public void Input(List<T> input)
		{
			input.ForEach(i => m_Memories.Add((Time.time, i)));
		}

		/// <summary>
		/// Retrieves all memory
		/// </summary>
		/// <returns></returns>
		public List<(float time, T data)> Query()
		{
			return m_Memories;
		}

		/// <summary>
		/// Retrieves a memory at a given time in O(log(n)) time complexity.
		/// TODO: test precision see
		/// https://docs.microsoft.com/en-us/dotnet/api/system.single.compareto?view=netcore-3.1#System_Single_CompareTo_System_Object_
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public T Query(float time)
		{
			// See https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.binarysearch?view=netcore-3.1#definition
			return m_Memories[m_Memories.BinarySearch((time, default), new TupleFloatT<T>())].data;
		}

		/// <summary>
		/// Retrieves a memory in an interval.
		/// TODO: should be an interval of time, think timescale ?
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public T Query(int index, int count, float time)
		{
			// See https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.binarysearch?view=netcore-3.1#definition
			return m_Memories[m_Memories.BinarySearch(index, count, (time, default), new TupleFloatT<T>())].data;
		}
		// TODO: maybe some helpers like: i want memories from last 5 seconds, or between 5h30 and 6h10
		public void Update()
		{
			// After a while, forget the old memories :)
			for (var i = m_Memories.Count - 1; i > 0; i--)
			{
				if (Time.time > m_Memories[i].time + Retention) m_Memories.RemoveAt(i);
				else return; // It's ordered ;)
			}
		}
	}
}

public class TupleFloatT<T> : IComparer<(float, T)>
{
	public int Compare((float, T) x, (float, T) y)
	{
		return x.Item1.CompareTo(y);
	}
}
