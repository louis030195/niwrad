using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Utils
{
	public static class ReflectionExtension
	{
		[CanBeNull]
        public static RangeAttribute GetRange(Type c, string prop)
		{
			var ca = c.GetField(prop).GetCustomAttributes(true);
			foreach (var cc in ca)
			{
				if (cc is RangeAttribute ret) return ret;
			}

            return null;
			throw new Exception($"Type {c}, property {prop} has no RangeAttribute");
		}

	}
}
