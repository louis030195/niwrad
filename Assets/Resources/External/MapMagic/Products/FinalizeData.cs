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
	public class FinalizeData
	{
		private class PerTypeData
		{
			public bool markRe; //marked for re-finalize. This one is set and clears each generate
			public Dictionary<IOutput, (object product, MatrixWorld mask)> 
				perOutputProducts = new Dictionary<IOutput, (object,MatrixWorld)>(); 
				//and this one is persistent
		}

		private Dictionary<FinalizeAction,PerTypeData> dict = new Dictionary<FinalizeAction,PerTypeData>();


		public void Add (FinalizeAction action, IOutput output, object product, MatrixWorld biomeMask)
		/// Adding new finalize data and marking finalize type to re-generate
		{
			PerTypeData perTypeData;
			if (!dict.TryGetValue(action, out perTypeData))
			{
				perTypeData = new PerTypeData();
				dict.Add(action, perTypeData);
			}

			if (!perTypeData.perOutputProducts.ContainsKey(output))
				perTypeData.perOutputProducts.Add(output, (product,biomeMask));
			else
				perTypeData.perOutputProducts[output] = (product,biomeMask);

			perTypeData.markRe = true;
		}


		public void Remove (FinalizeAction action, IOutput output, IEnumerable<TileData> subDatas = null)
		{
			if (!dict.TryGetValue(action, out PerTypeData perTypeData)) return;
			if (!perTypeData.perOutputProducts.ContainsKey(output)) return;
			perTypeData.perOutputProducts.Remove(output);

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.finalize.Remove(action, output, subDatas:subData.subDatas);
		}


		public void Remove (IOutput output)
		{
			foreach (PerTypeData perTypeData in dict.Values)
			{
				if (perTypeData.perOutputProducts.ContainsKey(output))
					perTypeData.perOutputProducts.Remove(output);
			}
		}


		public void Remove (IOutputGenerator gen)
		{
			if (gen is IOutput outputGen)
				Remove (outputGen);
			
			if (gen is ILayered<object> layeredGen)
			{
				foreach (PerTypeData perTypeData in dict.Values)
				{
					List<IOutput> outputsToRemove = new List<IOutput>();
				
					foreach (IOutput output in perTypeData.perOutputProducts.Keys)
						if (output.Gen == gen)
							outputsToRemove.Add(output);

					foreach (IOutput output in outputsToRemove)
						 perTypeData.perOutputProducts.Remove(output);
				}
			}
		}


		public bool IsMarked (FinalizeAction action, IEnumerable<TileData> subDatas = null)
		/// Checks if current type (=finalize action) is marked for re-generate
		{
			if (dict.TryGetValue(action, out PerTypeData perTypeData)  &&
				perTypeData.markRe)
					return true;

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					if (subData.finalize.IsMarked(action, subDatas:subData.subDatas))
						return true;

			return false;
		}


		public void Mark (bool mark, FinalizeAction action, IEnumerable<TileData> subDatas=null)
		/// Adds generate mark if data present
		{ 
			if (dict.TryGetValue(action, out PerTypeData perTypeData))
				perTypeData.markRe = mark;

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.finalize.Mark(mark, action, subDatas:subData.subDatas);
		}


		public void MarkAll (bool mark, IEnumerable<TileData> subDatas=null)
		{
			foreach (PerTypeData perTypeData in dict.Values)
				perTypeData.markRe = mark;

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.finalize.MarkAll(mark, subDatas:subData.subDatas);
		}


		public int GetTypeCount (FinalizeAction action, IEnumerable<TileData> subDatas = null)
		{
			int count = 0;

			if (dict.TryGetValue(action, out PerTypeData perTypeData))
				count += perTypeData.perOutputProducts.Count;

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					count += subData.finalize.GetTypeCount(action, subDatas:subData.subDatas);

			return count;
		}


		public IEnumerable<FinalizeAction> MarkedActions (IEnumerable<TileData> subDatas=null)
		/// Enumerating actions that have re-generate mark
		/// Using hashset so one action is marked only once, no matter if it is mentioned in different subdatas
		{
			HashSet<FinalizeAction> actions = new HashSet<FinalizeAction>();
			FillMarkedActions(actions, subDatas:subDatas);
			foreach (FinalizeAction action in actions)
				yield return action;
		}


		public HashSet<FinalizeAction> GetMarkedActions (IEnumerable<TileData> subDatas=null)
		/// Returns the collection of actions that have a re-generate mark
		/// Using hashset so one action is marked only once, no matter if it is mentioned in different subdatas
		{
			HashSet<FinalizeAction> actions = new HashSet<FinalizeAction>();
			FillMarkedActions(actions, subDatas:subDatas);
			return actions;
		}


		private void FillMarkedActions (HashSet<FinalizeAction> actions, IEnumerable<TileData> subDatas=null)
		{
			foreach (var kvp in dict)
			{
				FinalizeAction action = kvp.Key;
				PerTypeData perTypeData = kvp.Value;

				if (perTypeData.markRe && !actions.Contains(action))
					actions.Add(action);
			}

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.finalize.FillMarkedActions(actions, subDatas:subData.subDatas);
		}


		public IEnumerable<(TOutput output, TProduct product, MatrixWorld biomeMask)> ProductSets<TOutput,TProduct,TBiomeMask> (
			FinalizeAction action,
			//TileData data,  //to get current biome mask
			IEnumerable<TileData> subDatas = null ) //could read subDatas from data, but doing so just for uniformity
		/// Gets enumerable of output,product,mask of action/type (no matter of markRe)
		{
			if (dict.TryGetValue(action, out PerTypeData perTypeData))
			{
				foreach (var kvp in perTypeData.perOutputProducts)
				{
					TOutput output = (TOutput)kvp.Key;
					TProduct product = (TProduct)kvp.Value.product;
					MatrixWorld biomeMask = kvp.Value.mask;

					yield return (output, product, biomeMask);
				}
			}

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					foreach (var subProductSet in subData.finalize.ProductSets<TOutput,TProduct,TBiomeMask>(action, subDatas:subData.subDatas))
						yield return subProductSet;
		}


		public (TOutput[] output, TProduct[] product, MatrixWorld[] biomeMask) ProductArrays<TOutput,TProduct,TBiomeMask> (
			FinalizeAction action,
			IEnumerable<TileData> subDatas = null)
		{
			List<TOutput> outputs = new List<TOutput>();
			List<TProduct> products = new List<TProduct>();
			List<MatrixWorld> biomeMasks = new List<MatrixWorld>();

			foreach ((TOutput output, TProduct product, MatrixWorld biomeMask) in ProductSets<TOutput, TProduct, MatrixWorld>(action, subDatas))
			{
				outputs.Add(output);
				products.Add(product);
				biomeMasks.Add(biomeMask);
			}

			return (outputs.ToArray(), products.ToArray(), biomeMasks.ToArray());
		}


		public void Clear (IEnumerable<TileData> subDatas = null)
		/// Clears not only marks, but the generate data
		{ 
			dict.Clear(); 

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.finalize.Clear(subDatas:subData.subDatas);
		}
	}
}