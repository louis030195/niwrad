using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
	public static class EnumerableExtension
	{
		/// <summary>
		/// Return a random element among the array
		/// </summary>
		/// <param name="source"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T AnyItem<T>(this IEnumerable<T> source)
		{
			return source.AnyItem(1).SingleOrDefault();
		}

		public static IEnumerable<T> AnyItem<T>(this IEnumerable<T> source, int count)
		{
			return source.Shuffle().Take(count);
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.OrderBy(x => Guid.NewGuid());
		}
	}
}
