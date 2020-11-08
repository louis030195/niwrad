using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Nodes;
using MapMagic.Terrains;

namespace MapMagic.Products
{
	public class ApplyData
	{
		private Dictionary<Type,IApplyData> dict = new Dictionary<Type,IApplyData>();

		public ApplyData () { }

		public ApplyData (ApplyData src) { dict = new Dictionary<Type, IApplyData>(src.dict); }

		public void Add (IApplyData data)
		{
			Type type = data.GetType();
			if (dict.ContainsKey(type))
				dict[type] = data;
			else
				dict.Add(type, data);
		}

		public T Get<T> () where T: class, IApplyData
		{
			Type type = typeof(T);
			if (dict.ContainsKey(type))
				return (T)dict[type];
			else 
				return null;
		}

		public IApplyData Dequeue ()
		{
			//using priorities
			{
				foreach (Type type in dequeuePriorities)
				{
					if (dict.ContainsKey(type))
					{
						IApplyData data = dict[type];
						dict.Remove(type);
						return data;
					}
				}
			}

			//common case (other apply types not mentioned in priorities)
			{
				Type type=null; IApplyData data=null;
				foreach (var kvp in dict)
				{
					type = kvp.Key;
					data = kvp.Value;
					break;
				}
				if (type != null)
					dict.Remove(type);
				return data;
			}
		}

		private static readonly Type[] dequeuePriorities = {
			typeof(Nodes.MatrixGenerators.HeightOutput200),
			typeof(Nodes.MatrixGenerators.TexturesOutput200),
			typeof(Nodes.MatrixGenerators.CustomShaderOutput200),
			typeof(Nodes.MatrixGenerators.GrassOutput200) };
			//typeof(Nodes.ObjectsGenerators.TreesOutput),
			//typeof(Nodes.ObjectsGenerators.ObjectsOutput) };

		public int Count => dict.Count;

		public void Clear () { dict.Clear(); }
	}
}