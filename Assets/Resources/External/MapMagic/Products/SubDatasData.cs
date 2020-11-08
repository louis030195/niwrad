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
	public class SubDatasData : IEnumerable<TileData>
	{
		private Dictionary<IBiome,TileData> dict = new Dictionary<IBiome, TileData>();

		public TileData this[IBiome biome]
		{
			get
			{
				if (biome == null) return null; //can send null as null when checking biome mask
				if (dict.TryGetValue(biome, out TileData sub)) return sub;
				else return null;
			}

			set
			{
				if (dict.ContainsKey(biome)) dict[biome] = value;
				else dict.Add(biome, value);
			}
		}


		public TileData GetOrCreate (IBiome biome, TileData parentData)
		{
			TileData data = this[biome];

			if (data == null)
			{
				data = new TileData(parentData);
				this[biome] = data;
			}
			
			return data;
		}


		public void Remove (IBiome node, IEnumerable<TileData> subDatas=null)
		{
			Debug.Log("SubData Removed");

			if (dict.ContainsKey(node)) dict.Remove(node);

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.subDatas.Remove(node, subDatas:subData.subDatas);
		}


		public IEnumerator<TileData> GetEnumerator ()
		{
			return dict.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return dict.Values.GetEnumerator();
		}

		public void Clear () => dict.Clear();

		public int Count => dict.Count;
	}
}