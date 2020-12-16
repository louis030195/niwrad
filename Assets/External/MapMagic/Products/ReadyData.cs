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
	public class ReadyData
	{
		private HashSet<Generator> ready = new HashSet<Generator>();  //generators that have actual products in all outlets, even if they are null
		private Dictionary<ICustomComplexity, float> progress = new Dictionary<ICustomComplexity, float>();


		public bool this[Generator gen]
		{
			get { return ready.Contains(gen); }
			set 
			{ 
				if (value)
				{
					if (ready.Contains(null))
						Debug.Log("Null");

					if (!ready.Contains(gen)) 
						ready.Add(gen); 
							
					if (gen is ICustomComplexity ccGen && progress.ContainsKey(ccGen))
						progress.Remove(ccGen); 
				}

				else if (ready.Contains(gen)) ready.Remove(gen);
			}
		}


		public bool AreAllReady (Generator[] gens)
		{
			for (int g=0; g<gens.Length; g++)
				if (!ready.Contains(gens[g])) return false;
			return true;
		}


		public void Remove (Generator gen, IEnumerable subDatas=null) 
		{
			if (ready.Contains(gen)) ready.Remove(gen);

			if (gen is ICustomComplexity ccGen && progress.ContainsKey(ccGen))
						progress.Remove(ccGen); 

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.ready.Remove(gen, subDatas:subData.subDatas);
		}


		public void RemoveOfType<T> (IEnumerable subDatas=null) 
		{
			ready.RemoveWhere(g => g is T);
			progress.RemoveWhere(g => g is T);

			if (subDatas != null)
				foreach (TileData subData in subDatas)
					subData.ready.RemoveOfType<T>(subDatas:subData.subDatas);
		}


		public void SetProgress (ICustomComplexity gen, float progress)
		{
			if (this.progress.ContainsKey(gen)) this.progress[gen] = progress;
			else this.progress.Add(gen, progress);
		}


		public float GetProgress (ICustomComplexity gen)
		{
			if (this.progress.TryGetValue(gen, out float progress)) return progress;
			else return 0;
		}


		public void Clear () { ready.Clear(); progress.Clear(); }
	}
}