using System;
using UnityEngine;

namespace Utils
{
	public static class ReflectionExtension
	{
		public static RangeAttribute GetRange(Type c, string prop)
		{
			var ca = c.GetField(prop).GetCustomAttributes(true);
			foreach (var cc in ca)
			{
				if (cc is RangeAttribute ret) return ret;
			}
			throw new Exception($"Type {c}, property {prop} has no RangeAttribute");
		}

	}
}
