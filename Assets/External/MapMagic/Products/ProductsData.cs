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
	public class ProductsData
	{
		private Dictionary<IOutlet<object>,object> products = new Dictionary<IOutlet<object>,object>();	//per-outlet intermediate results
		public IDictionary<IInlet<object>, IOutlet<object>> linksLut; //assigned by graph before generate to find a product by inlet too

		public object this[IOutlet<object> outlet]
		{
			get 
			{
				if (outlet == null) return null;
				if (products.TryGetValue(outlet, out object obj)) return obj;
				return null;
			}

			set 
			{
				if (products.ContainsKey(outlet)) products[outlet] = value;
				else products.Add(outlet, value);
			}
		}


		public virtual T ReadInlet<T> (IInlet<T> inlet) where T:class  //virtual to override in tester mockup
		{
			if (!linksLut.TryGetValue(inlet, out IOutlet<object> outlet)) return null;
			if (outlet == null) return null;
			if (products.TryGetValue(outlet, out object obj)) return (T)obj;
			return null;
		}


		public void Remove (IOutlet<object> gen, IEnumerable subDatas=null)
		{ 
			if (products.ContainsKey(gen)) products.Remove(gen); 

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.products.Remove(gen, subDatas:subData.subDatas);
		}


		public IOutlet<object>[] AllOutlets ()
		{
			IOutlet<object>[] keys = new IOutlet<object>[products.Count];
			products.Keys.CopyTo(keys, 0);
			return keys;
		}


		public int Count => products.Count;

		public void Clear () { products.Clear(); }
	}
}