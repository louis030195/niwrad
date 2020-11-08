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
	public class PrepareData 
	{
		private Dictionary<IPrepare,object> prepare = new Dictionary<IPrepare, object>();

		public object this[IPrepare node]
		{
			get
			{
				if (node == null) return null; //can send null as null when checking biome mask
				if (prepare.TryGetValue(node, out object obj)) return obj;
				else return null;
			}

			set
			{
				if (prepare.ContainsKey(node)) prepare[node] = value;
				else prepare.Add(node, value);
			}
		}

		public int Count => prepare.Count;

		public void Clear () { prepare.Clear(); }
	}
}