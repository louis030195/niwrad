using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Core;
using MapMagic.Products;

namespace MapMagic.Nodes.MatrixGenerators
{

	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Curve", iconName="GeneratorIcons/Curve", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/curve")]
	public class Curve200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld> 
	{
		public Curve curve = new Curve( new Vector2(0,0), new Vector2(1,1) );   

		[NonSerialized] public float[] histogram = null;
		public const int histogramSize = 256;


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null) return; 
			if (!enabled) { data.products[this]=src; return; }

			if (stop!=null && stop.stop) return;
			if (data.isPreview)
				histogram = src.Histogram(histogramSize, max:1, normalize:true);

			if (stop!=null && stop.stop) return;
			MatrixWorld dst = new MatrixWorld(src);

			if (stop!=null && stop.stop) return;
			curve.Refresh(updateLut:true);

			if (stop!=null && stop.stop) return;
			//for (int i=0; i<dst.arr.Length; i++) dst.arr[i] = curve.EvaluateLuted(dst.arr[i]);
			dst.UniformCurve(curve.lut);

			if (stop!=null && stop.stop) return;
			data.products[this] = dst;
		}
	}


	/*[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="NonExisting", iconName="GeneratorIcons/Curve", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/curve")]
	public class NonExisting200_ : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld>, IMultiInlet
	{
		[Val(name="Intensity")] public float brightness = 0f;
		[Val(name="Contrast")] public float contrast = 1f;
		[Val(name="Texture")] public Texture2D texture;

		[Val("Inlet", "Inlet")] public IInlet<MatrixWorld> srcIn = new Inlet<MatrixWorld>();
		[Val("Mask", "Inlet")]	public IInlet<MatrixWorld> maskIn = new Inlet<MatrixWorld>();
		public IEnumerable<IInlet<object>> Inlets() { yield return srcIn; yield return maskIn; }

		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null) return; 
			if (!enabled) { data.products[this]=src; return; }

			if (stop!=null && stop.stop) return;
			MatrixWorld dst = new MatrixWorld(src);

			if (stop!=null && stop.stop) return;
			dst.BrighnesContrast(brightness, contrast);

			if (stop!=null && stop.stop) return;
			data.products[this] = dst;
		}
	}*/



	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Levels", iconName="GeneratorIcons/Levels", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/curve")]
	public class Levels200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld> 
	{
		//public Vector2 min = new Vector2(0,0);
		//public Vector2 max = new Vector2(1,1);
		public float inMin = 0;
		public float inMax = 1;
		public float gamma = 1f; //min/max bias. 0 for min 2 for max, 1 is straight curve

		public float outMin = 0;
		public float outMax = 1;

		[NonSerialized] public float[] histogram = null;
		public const int histogramSize = 256;

		public bool guiParams = false;


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null) return; 
			if (!enabled) { data.products[this]=src; return; }

			if (stop!=null && stop.stop) return;
			if (data.isPreview)
				histogram = src.Histogram(histogramSize, max:1, normalize:true);

			if (stop!=null && stop.stop) return;
			MatrixWorld dst = new MatrixWorld(src);

			if (stop!=null && stop.stop) return;
			dst.Levels(inMin, inMax, gamma, outMin, outMax);

			if (stop!=null && stop.stop) return;
			data.products[this] = dst;
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Contrast", iconName="GeneratorIcons/Contrast", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/curve")]
	public class Contrast200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld> 
	{
		[Val(name="Intensity")] public float brightness = 0f;
		[Val(name="Contrast")] public float contrast = 1f;


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null) return; 
			if (!enabled) { data.products[this]=src; return; }

			if (stop!=null && stop.stop) return;
			//src.Histogram(256, max:1, normalize:true);
			//if (data.isPreview)
			//	histogram = src.Histogram(histogramSize, max:1, normalize:true);

			if (stop!=null && stop.stop) return;
			MatrixWorld dst = new MatrixWorld(src);

			if (stop!=null && stop.stop) return;
			dst.BrighnesContrast(brightness, contrast);

			if (stop!=null && stop.stop) return;
			data.products[this] = dst;
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Unity Curve", iconName="GeneratorIcons/UnityCurve", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/curve")]
	public class UnityCurve200 : Generator, IMultiInlet, IOutlet<MatrixWorld> 
	{
		[Val("Inlet", "Inlet")] public readonly IInlet<MatrixWorld> srcIn = new Inlet<MatrixWorld>();
		[Val("Mask", "Inlet")]	public readonly IInlet<MatrixWorld> maskIn = new Inlet<MatrixWorld>();
		public IEnumerable<IInlet<object>> Inlets() { yield return srcIn; yield return maskIn; }

		public AnimationCurve curve = new AnimationCurve( new Keyframe[] { new Keyframe(0,0,1,1), new Keyframe(1,1,1,1) } );
		public Vector2 min = new Vector2(0,0);
		public Vector2 max = new Vector2(1,1);

		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixWorld src = data.products.ReadInlet(srcIn);
			MatrixWorld mask = data.products.ReadInlet(maskIn);
			if (src == null) return; 
			if (!enabled) { data.products[this]=src; return; }

			//preparing output
			if (stop!=null && stop.stop) return;
			MatrixWorld dst = new MatrixWorld(src);

			//curve
			if (stop!=null && stop.stop) return;
			AnimCurve c = new AnimCurve(curve);
			for (int i=0; i<dst.arr.Length; i++) dst.arr[i] = c.Evaluate(dst.arr[i]);

			//mask
			if (stop!=null && stop.stop) return;
			if (mask != null) dst.InvMix(src,mask);

			data.products[this] = dst;
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Mask", iconName="GeneratorIcons/MapMask", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/curve")]
	public class Mask200 : Generator, IMultiInlet, IOutlet<MatrixWorld> 
	{
		[Val("Input A", "Inlet")]	public readonly Inlet<MatrixWorld> aIn = new Inlet<MatrixWorld>();
		[Val("Input B", "Inlet")]	public readonly Inlet<MatrixWorld> bIn = new Inlet<MatrixWorld>();
		[Val("Mask", "Inlet")]	public readonly Inlet<MatrixWorld> maskIn = new Inlet<MatrixWorld>();
		public IEnumerable<IInlet<object>> Inlets () { yield return aIn; yield return bIn; yield return maskIn; }

		[Val("Invert")]	public bool invert = false;


		public override void Generate (TileData data, StopToken stop)
		{
			if (stop!=null && stop.stop) return;
			MatrixWorld matrixA = data.products.ReadInlet(aIn);
			MatrixWorld matrixB = data.products.ReadInlet(bIn);
			MatrixWorld mask = data.products.ReadInlet(maskIn);
			if (matrixA == null || matrixB == null) return; 
			if (!enabled || mask == null) { data.products[this]=matrixA; return; }

			if (stop!=null && stop.stop) return;
			MatrixWorld dst = new MatrixWorld(matrixA);

			if (stop!=null && stop.stop) return;
			dst.Mix(matrixB, mask, 0, 1, invert, false, 1);

			if (stop!=null && stop.stop) return;
			data.products[this] = dst;
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Blend", iconName="GeneratorIcons/Blend", disengageable = true, colorType = typeof(MatrixWorld), helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/blend")]
	public class Blend200 : Generator, IMultiInlet, IOutlet<MatrixWorld>, ILayered<Blend200.Layer>
	{
		public class Layer
		{
			public readonly Inlet<MatrixWorld> inlet = new Inlet<MatrixWorld>();
			public BlendAlgorithm algorithm = BlendAlgorithm.add;
			public float opacity = 1;
			public bool guiExpanded = false;
		}

		public Layer[] layers = new Layer[] { new Layer(), new Layer() };
		public Layer[] Layers => layers; 
		public void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(Layer)i);

		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i].inlet;
		}

		public override void Generate (TileData data, StopToken stop) 
		{
			if (stop!=null && stop.stop) return;
			if (!enabled) return;
			MatrixWorld matrix = new MatrixWorld(data.area.full.rect, data.area.full.worldPos, data.area.full.worldSize, data.globals.height);
			
			if (stop!=null && stop.stop) return;

			if (stop!=null && stop.stop) return;
				for (int i = 0; i < layers.Length; i++)
			{
				Layer layer = layers[i];
				if (layer.inlet == null) continue;

				MatrixWorld blendMatrix = data.products.ReadInlet(layer.inlet);
				if (blendMatrix == null) continue;

				Blend(matrix, blendMatrix, layer.algorithm, layer.opacity);
			}
			
			data.products[this] = matrix;
		}


		public enum BlendAlgorithm {
			mix=0, 
			add=1, 
			subtract=2, 
			multiply=3, 
			divide=4, 
			difference=5, 
			min=6, 
			max=7, 
			overlay=8, 
			hardLight=9, 
			softLight=10} 
			
		public static void Blend (Matrix m1, Matrix m2, BlendAlgorithm algorithm, float opacity=1)
		{
			switch (algorithm)
			{
				case BlendAlgorithm.mix: default: m1.Mix(m2, opacity); break;
				case BlendAlgorithm.add: m1.Add(m2, opacity); break;
				case BlendAlgorithm.subtract: m1.Subtract(m2, opacity); break;
				case BlendAlgorithm.multiply: m1.Multiply(m2, opacity); break;
				case BlendAlgorithm.divide: m1.Divide(m2, opacity); break;
				case BlendAlgorithm.difference: m1.Difference(m2, opacity); break;
				case BlendAlgorithm.min: m1.Min(m2, opacity); break;
				case BlendAlgorithm.max: m1.Max(m2, opacity); break;
				case BlendAlgorithm.overlay: m1.Overlay(m2, opacity); break;
				case BlendAlgorithm.hardLight: m1.HardLight(m2, opacity); break;
				case BlendAlgorithm.softLight: m1.SoftLight(m2, opacity); break;
			}
		}
	}


	[System.Serializable]
	[GeneratorMenu (
		menu="Map/Modifiers", 
		name ="Normalize", 
		disengageable = true, 
		helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/normalize",
		iconName="GeneratorIcons/Normalize",
		drawInlets = false,
		drawOutlet = false,
		colorType = typeof(MatrixWorld))]
	public class Normalize200 : Generator, IMultiInlet, IMultiOutlet, ILayered<Normalize200.NormalizeLayer>
	{
		public class NormalizeLayer : INormalizableLayer, IInlet<MatrixWorld>, IOutlet<MatrixWorld>
		{
			public float Opacity { get; set; }

			public Generator Gen { get; private set; }
			public void SetGen (Generator gen) => Gen=gen;
			public NormalizeLayer (Generator gen) { this.Gen = gen; }
			public NormalizeLayer () { Opacity = 1; }

		}

		public NormalizeLayer[] layers = new NormalizeLayer[0];
		public NormalizeLayer[] Layers => layers; 
		public void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(NormalizeLayer)i);


		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i];
		}

		public IEnumerable<IOutlet<object>> Outlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i];
		}

		public override void Generate (TileData data, StopToken stop)
		{
			if (layers.Length == 0) return;
			NormalizeLayers(layers, data, stop);
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Blur", iconName="GeneratorIcons/Blur", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/blur")]
	public class Blur200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld>
	{
		[Val("Downsample")] public float downsample = 10f;
		[Val("Blur")] public float blur = 3f;

		public override void Generate (TileData data, StopToken stop)
		{
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null) return; 
			if (!enabled) { data.products[this]=src; return; }

			MatrixWorld dst = new MatrixWorld(src);

			int rrDownsample = (int)(downsample / Mathf.Sqrt(dst.PixelSize.x));
			float rrBlur = blur / dst.PixelSize.x;

			if (rrDownsample > 1)
				MatrixOps.DownsampleBlur(src, dst, rrDownsample, rrBlur);
			else
				MatrixOps.GaussianBlur(src, dst, rrBlur);

			data.products[this] = dst;
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Cavity", iconName="GeneratorIcons/Cavity", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/cavity")]
	public class Cavity200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld>
	{
		public enum CavityType { Convex, Concave, Both }
		[Val("Type")]		public CavityType type = CavityType.Convex;
		[Val("Intensity")]	public float intensity = 3;
		[Val("Spread")]		public float spread = 10; //actually the pixel size (in world units) of the lowerest mipmap. Same for draft and main

		public override void Generate (TileData data, StopToken stop)
		{
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null) return; 
			if (!enabled) { data.products[this]=src; return; }

			if (stop!=null && stop.stop) return;
			MatrixWorld dst = new MatrixWorld(src.rect, src.worldPos, src.worldSize);
			MatrixOps.Cavity(src, dst); //produces the map with range -1 - 1
			dst.Multiply(1f / Mathf.Pow(dst.PixelSize.x, 0.25f));

			float minResolution = data.area.active.worldSize.x / spread;  //area worldsize / (spread = min pixel size)
			float downsample = Mathf.Log(src.rect.size.x, 2);
			downsample -= Mathf.Log(minResolution, 2);

			if (stop!=null && stop.stop) return;
			MatrixOps.OverblurMipped(dst, downsample:Mathf.Max(0,downsample), escalate:1.5f);

			if (stop!=null && stop.stop) return;
			dst.Multiply(intensity*100f);

			switch (type)
			{
				case CavityType.Convex: dst.Invert(); break;
				//case CavityType.Concave: break;
				case CavityType.Both: dst.Invert(); dst.Multiply(0.5f); dst.Add(0.5f); break;
			}
			
			dst.Clamp01();

			//blending 50% map if downsample doesn't allow cavity here (for drafts or low-res)
			if (stop!=null && stop.stop) return;
			if (downsample < 0f)
			{
				float subsample = -downsample/4; 
				if (subsample > 1) subsample = 1;

				float avg = dst.Average();
				dst.Fill(avg, subsample);
			}
			
			if (stop!=null && stop.stop) return;
			data.products[this] = dst;
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Slope", iconName="GeneratorIcons/Slope", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/slope")]
	public class Slope200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld>
	{
		[Val("From")]			public float from = 30;
		[Val("To")]				public float to = 90;
		[Val("Smooth Range")]	public float range = 30f;
		
		public override void Generate (TileData data, StopToken stop)
		{
			MatrixWorld src = data.products.ReadInlet(this);
			if (src==null) return; 
			if (!enabled) { data.products[this]=src; return; }

			MatrixWorld dst = Slope(src, data.globals.height);

			data.products[this] = dst;
		}

		public MatrixWorld Slope (MatrixWorld heights, float height)
		{
			//delta map
			MatrixWorld delta = new MatrixWorld(heights.rect, heights.worldPos, heights.worldSize);
			MatrixOps.Delta(heights, delta);

			//slope map
			float minAng0 = from-range/2;
			float minAng1 = from+range/2;
			float maxAng0 = to-range/2;
			float maxAng1 = to+range/2;

			float pixelSize = 1f * heights.worldSize.x / heights.rect.size.x; //using the terain-height relative values
			
			float minDel0 = Mathf.Tan(minAng0*Mathf.Deg2Rad) * pixelSize / height;
			float minDel1 = Mathf.Tan(minAng1*Mathf.Deg2Rad) * pixelSize / height;
			float maxDel0 = Mathf.Tan(maxAng0*Mathf.Deg2Rad) * pixelSize / height;
			float maxDel1 = Mathf.Tan(maxAng1*Mathf.Deg2Rad) * pixelSize / height;

			//dealing with 90-degree
			if (maxAng0 > 89.9f) maxDel0 = 20000000; 
			if (maxAng1 > 89.9f) maxDel1 = 20000000;

			if (from < 0.00001f) { minDel0=-1; minDel1=-1; }
			//not right, but intuitive - if user wants to mask from 0 don't add gradient here

			//ignoring min if it is zero
			//if (steepness.x<0.0001f) { minDel0=0; minDel1=0; }

			delta.SelectRange(minDel0, minDel1, maxDel0, maxDel1);

			return delta;
		}

	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Selector", iconName="GeneratorIcons/Selector", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/slope")]
	public class Selector200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld>
	{
		public enum RangeDet { Transition, MinMax}
		public RangeDet rangeDet = RangeDet.Transition;
		public enum Units { Map, World }
		public Units units = Units.Map;
		public Vector2 from = new Vector2(0.4f, 0.6f);
		public Vector2 to = new Vector2(1f, 1f);
		
		public override void Generate (TileData data, StopToken stop)
		{
			MatrixWorld src = data.products.ReadInlet(this);
			if (src==null) return; 
			if (!enabled) { data.products[this] = src; return; }

			MatrixWorld dst = new MatrixWorld(src);
			float min0 = from.x;  if (units==Units.World) min0 /= data.globals.height;
			float min1 = from.y;  if (units==Units.World) min1 /= data.globals.height;
			float max0 = to.x;    if (units==Units.World) max0 /= data.globals.height;
			float max1 = to.y;    if (units==Units.World) max1 /= data.globals.height;
			dst.SelectRange(min0, min1, max0, max1);

			data.products[this] = dst;
		}
	}


	[System.Serializable]
	[GeneratorMenu (
		menu="Map/Modifiers", 
		name ="Terrace", 
		iconName="GeneratorIcons/Terrace", 
		disengageable = true, 
		helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/terrace")]
	public class Terrace200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld>
	{
		[Val("Seed")]		 public int seed = 12345;
		[Val("Num")]		 public int num = 10;
		[Val("Uniformity")] public float uniformity = 0.5f;
		[Val("Steepness")]	 public float steepness = 0.5f;
		//[Val("Intensity")]	 public float intensity = 1f;

		
		public override void Generate (TileData data, StopToken stop)
		{
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null || num <= 1) return; 
			if (!enabled) { data.products[this]=src; return; }

			MatrixWorld dst = new MatrixWorld(src); 
			float[] terraceLevels = TerraceLevels(new Noise(data.random,seed));
			
			if (stop!=null && stop.stop) return;
			dst.Terrace(terraceLevels, steepness);

			data.products[this] = dst;
		}


		public float[] TerraceLevels (Noise random)
		{
			//creating terraces
			float[] terraces = new float[num];

			float step = 1f / (num-1);
			for (int t=1; t<num; t++)
				terraces[t] = terraces[t-1] + step;

			for (int i=0; i<10; i++)
				for (int t=1; t<num-1; t++)
				{
					float rndVal = random.Random(i);
					rndVal = terraces[t-1] +  rndVal*(terraces[t+1]-terraces[t-1]);
					terraces[t] = terraces[t]*uniformity + rndVal*(1-uniformity);
				}

			return terraces;
		}
	}


	[System.Serializable]
	[GeneratorMenu (menu="Map/Modifiers", name ="Erosion", iconName="GeneratorIcons/Erosion", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/erosion")]
	public class Erosion200 : Generator, IInlet<MatrixWorld>, IOutlet<MatrixWorld>, ICustomComplexity
	{
		[Val("Iterations")]		 public int iterations = 3;
		[Val("Durability")] public float terrainDurability=0.9f;
		//[Val("Erosion")]	 
			public float erosionAmount=1f;
		[Val("Sediment")]	 public float sedimentAmount=0.75f;
		[Val("Fluidity")] public int fluidityIterations=3;
		//[Val("Ruffle")]			 public float ruffle=0.4f;
		//[Val("Safe Borders")]		 public int safeBorders = 10;
		//[Val("Cliff Opacity")]		 public float cliffOpacity = 1f;
		//[Val("Sediment Opacity")]	 public float sedimentOpacity = 1f;

		//public enum Algorithm { Managed, Native }
		//#if UNITY_EDITOR_WIN
		//[Val("Algorithm")]	public Algorithm algorithm = Algorithm.Native;
		//#else
		//[Val("Algorithm")]	public Algorithm algorithm = Algorithm.Managed;
		//#endif

		[DllImport ("NativePlugins", EntryPoint = "SetOrder")]
		private static extern void SetOrder (float[] refArr, int[] orderArr, int length);

		[DllImport ("NativePlugins", EntryPoint = "MaskBorders")]
		private static extern int MaskBorders (int[] orderArr, CoordRect matrixRect);

		[DllImport ("NativePlugins", EntryPoint = "CreateTorrents")]
		private static extern int CreateTorrents (float[] heights, int[] order, float[] torrents, CoordRect matrixRect);

		[DllImport ("NativePlugins", EntryPoint = "Erode")]
		private static extern int Erode (float[] heights, float[] torrents, float[] mudflow, int[] order, CoordRect matrixRect,
			float erosionDurability = 0.9f, float erosionAmount = 1, float sedimentAmount = 0.5f);

		[DllImport ("NativePlugins", EntryPoint = "TransferSettleMudflow")]
		private static extern int TransferSettleMudflow(float[] heights, float[] mudflow, float[] sediments, int[] order, CoordRect matrixRect, int erosionFluidityIterations = 3);

		public float Complexity {get{ return iterations*2; }}
		public float Progress (TileData data) { return data.ready.GetProgress(this); }


		public override void Generate (TileData data, StopToken stop)
		{
			MatrixWorld src = data.products.ReadInlet(this);
			if (src == null) return; 
			if (!enabled || iterations <= 0) { data.products[this]=src; return; }

			MatrixWorld dst = new MatrixWorld(src);
			Erosion(dst, data.isDraft, data, stop);

			data.products[this] = dst;
		}


		public void Erosion (MatrixWorld dstHeight, bool isDraft, TileData data, StopToken stop=null)
		{
			//allocating temporary matrices
			Matrix2D<int> order = new Matrix2D<int>(dstHeight.rect);
			Matrix torrents = new Matrix(dstHeight.rect);
			Matrix mudflow = new Matrix(dstHeight.rect);
			Matrix sediment = new Matrix(dstHeight.rect);

			int curIterations = iterations;
			int curFluidity = fluidityIterations;

			if (isDraft)
			{
				curIterations = iterations/3;
				curFluidity = fluidityIterations/3;
			}

			//calculate erosion
			for (int i=0; i<curIterations; i++) 
			{
				Den.Tools.Erosion.SetOrder(dstHeight, order);
				if (stop!=null && stop.stop) return;

				Den.Tools.Erosion.MaskBorders(order);
				if (stop!=null && stop.stop) return;

				Den.Tools.Erosion.CreateTorrents(dstHeight, order, torrents);
				if (stop!=null && stop.stop) return;

				Den.Tools.Erosion.Erode(dstHeight, torrents, mudflow, order, terrainDurability, erosionAmount, sedimentAmount);
				if (stop!=null && stop.stop) return;

				Den.Tools.Erosion.TransferSettleMudflow(dstHeight, mudflow, sediment, order, curFluidity);
				if (stop!=null && stop.stop) return;

				data.ready.SetProgress(this, i*2);
			}
		}
	}
}



/*	[System.Serializable]
	[GeneratorMenu (menu="Map", name ="Shore", disengageable = true, helpLink ="https://gitlab.com/denispahunov/mapmagic/wikis/map_generators/shore")]
	public class ShoreGenerator : Generator, IOutlet<MatrixWorld>
	{
		[Val("Height", priority=3)]		public IOutlet<MatrixWorld> heightIn { get; set; }
		[Val("Mask", priority=2)]			public IOutlet<MatrixWorld> maskIn { get; set; }
		[Val("Ridge Noise", priority=1)]	public IOutlet<MatrixWorld> ridgeNoiseIn { get; set; }

		[Val("Intensity")]		public float intensity = 1f;
		[Val("Beach Level")]	public float beachLevel = 20f;
		[Val("Beach Size")]	public float beachSize = 10f;
		[Val("Ridge Min")]		public float ridgeMinGlobal = 2;
		[Val("Ridge Max")]		public float ridgeMaxGlobal = 10;

		
		public override void Generate (Results results, Area area, int seed, StopCallback stop)
		{
			MatrixWorld src = results.GetProduct(heightIn);
			if (src==null) { results.SetProduct(this, null); return; }

			MatrixWorld dst = new MatrixWorld(area.full.resolution, area.full.position, area.full.size);
			MatrixWorld ridgeNoise = results.GetProduct<MatrixWorld>(ridgeNoiseIn);

			MatrixWorld sands = new MatrixWorld(area.full.resolution, area.full.position, area.full.size);

			//converting ui values to internal (??!)
			float beachMin = beachLevel / dst.worldSize.y;
			float beachMax = (beachLevel+beachSize) / dst.worldSize.y;
			float ridgeMin = ridgeMinGlobal / dst.worldSize.y;
			float ridgeMax = ridgeMaxGlobal / dst.worldSize.y;

			Coord min = src.rect.Min; Coord max = src.rect.Max;
			for (int x=min.x; x<max.x; x++)
			   for (int z=min.z; z<max.z; z++)
			{
				float srcHeight = src[x,z];

				//creating beach
				float beachHeight = srcHeight;
				if (srcHeight > beachMin && srcHeight < beachMax) beachHeight = beachMin;
				
				float sand = 0;
				if (srcHeight <= beachMax) sand = 1;

				//blurring ridge
				float curRidgeDist = 0;
				float noise = 0; if (ridgeNoise != null) noise = ridgeNoise[x,z];
				curRidgeDist = ridgeMin*(1-noise) + ridgeMax*noise;
				
				if (srcHeight >= beachMax && srcHeight <= beachMax+curRidgeDist) 
				{
					float percent = (srcHeight-beachMax) / curRidgeDist;
					percent = Mathf.Sqrt(percent);
					percent = 3*percent*percent - 2*percent*percent*percent;
					
					beachHeight = beachMin*(1-percent) + srcHeight*percent;
					
					sand = 1-percent;
				}

				//setting height
				beachHeight = beachHeight*intensity + srcHeight*(1-intensity);
				dst[x,z] = beachHeight;
				sands[x,z] = sand;
			}

			//mask
			Matrix mask = results.GetProduct<MatrixWorld>(maskIn);
			if (mask != null)  Matrix.Mask(src, dst, mask); // Matrix.Mask(null, sands, mask); }

			if (stop!=null && stop(0)) return;
			results.SetProduct(this, dst); 
		}
	}
*/
