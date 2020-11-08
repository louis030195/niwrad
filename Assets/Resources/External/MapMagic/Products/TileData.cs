using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Nodes;
using MapMagic.Terrains;
using MapMagic.Core;

namespace MapMagic.Products
{
	public delegate void FinalizeAction (TileData data, StopToken stop);
	public delegate void ApplyAction (TileData data, Terrain terrain, StopToken stop);

	[Serializable, StructLayout (LayoutKind.Sequential)] //to pass to native
	public class StopToken
	/// Data is the same in all chunk's threads. Stop token is unique per thread.
	{
		public bool stop;
		public bool restart;
	}

	public class TileData
	{
		//temporary parameters assigned before each generate
		public Area area;
		public Globals globals;
		public Noise random; //a ref to the one in parentGraph (to make it possible generate with no graph) 

		public MatrixWorld currentBiomeMask;
		public MatrixWorld lastBiomeMask;

		public bool isPreview; //should this generate be used as a preview?
		public bool isDraft; //is this terrain low-detail (to avoid applying objects and grass)?

		//generate products
		public ReadyData ready = new ReadyData();  //generators that have actual products in all outlets, even if they are null
		public ProductsData products = new ProductsData();	//per-outlet intermediate results
		public SubDatasData subDatas = new SubDatasData();  //pre-IBiome internal sub-datas
		public PrepareData prepare = new PrepareData();
		public FinalizeData finalize = new FinalizeData();
		public ApplyData apply = new ApplyData();

		public MatrixWorld heights = null; //last heights applied to floor objects


		public TileData () { }
		public TileData (TileData src)
		{
			area = src.area;
			globals = src.globals;
			isPreview = src.isPreview;
			isDraft = src.isDraft;
			random = src.random;
			currentBiomeMask = src.currentBiomeMask;
		}


		public void Clear (bool clearApply=true)
		/// Clears all of the unnecessary data in playmode
		{
			products.Clear();
			subDatas.Clear();
			prepare.Clear();
			ready.Clear();
			heights = null; //if (heights != null) heights.Clear(); //clear is faster, but tends to miss an error
			currentBiomeMask = null; //no Clear since it could be used in other biome
			finalize.Clear();
			if (clearApply) apply.Clear();
		}


		public void Remove (Generator gen, bool inSubDatas=true)
		{
			ready[gen] = false;

			if (gen is IOutlet<object> outGen)
				products.Remove(outGen);

			if (gen is IMultiOutlet mulOutGen)
				foreach (IOutlet<object> outlet in mulOutGen.Outlets())
					products.Remove(outlet);

			if (gen is IOutputGenerator output)
				finalize.Remove(output);

			if (inSubDatas)
				foreach (TileData subData in subDatas)
					if (subData != null) Remove(gen);
		}


		public int Count (bool countSubDatas=true)
		/// The number of all products for debug purpose
		{
			int count = 0;

			count += products.Count;
			count += prepare.Count;
			//count += finalize.Count;
			if (heights != null) count++;

			if (countSubDatas)
				foreach (TileData subData in subDatas)
					if (subData != null) count += subData.Count(true);

			return count;
		}

		public void SetBiomeMask (MatrixWorld mask, MatrixWorld parentBiomeMask)
		{
			currentBiomeMask = new MatrixWorld(mask); //setting a copy since generate will compare with previously applied one

			if (parentBiomeMask != null)
				currentBiomeMask.Multiply(parentBiomeMask);
		}

	}
}