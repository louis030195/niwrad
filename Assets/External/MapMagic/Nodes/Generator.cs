using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.Matrices; //Normalize gen
//using Den.Tools.Segs;
using Den.Tools.Splines;

using MapMagic.Core;
using MapMagic.Products;

namespace MapMagic.Nodes
{
	public interface IInlet<out T> where T:class
	/// The one that linked with the outlet via graph dictionary
	/// Could be generator or layer itself or a special inlet obj
	{
		Generator Gen { get; } 
		void SetGen (Generator gen); //setting inlet for generator layers only on create
	}

	public interface IOutlet<out T> where T:class
	/// The one that generates and stores product
	/// Could be generator or layer itself or a special outlet obj
	{
		Generator Gen { get; }
		void SetGen (Generator gen); 
	}

	public interface IMultiInlet
	/// Generator that stores multiple inlets (either layered or standard)
	{ 
		IEnumerable<IInlet<object>> Inlets ();
	}


	public interface IMultiOutlet 
	/// Generator that stores multiple outlets
	{ 
		IEnumerable<IOutlet<object>> Outlets ();
	}

	[Serializable]
	public class Inlet<T> : IInlet<T> where T: class 
	/// The one that is assigned in non-layered multiinlet generators
	{
		[SerializeField] private Generator gen; 
		public Generator Gen { get{return gen;} private set{gen=value;} } //auto-property is not serialized
		public void SetGen (Generator gen) => Gen=gen;
	}

	[Serializable]
	public class Outlet<T> : IOutlet<T> where T: class
	/// The one that is assigned in non-layered multioutlet generators
	{
		[SerializeField] private Generator gen; 
		public Generator Gen { get{return gen;} private set{gen=value;} }
		public void SetGen (Generator gen) => Gen=gen;
	}

	public interface IPrepare
	/// Node has something to make in main thread before generate start in Prepare fn
	{
		void Prepare (TileData data, Terrain terrain);
	}


	public interface ISceneGizmo 
	/// Displays some gizmo in a scene view
	{ 
		void DrawGizmo(); 
		bool hideDefaultToolGizmo {get;set;} 
	}


	[Flags] public enum OutputLevel { Draft=1, Main=2, Both=3 }  //Both is for gui purpose only

	public interface IOutputGenerator
	/// Final output node (height, textures, objects, etc)
	{
		//just to mention: static Finalize (TileData data, StopToken stop);
		//Action<TileData,StopToken> FinalizeAction { get; }
		void Purge (TileData data, Terrain terrain);
		OutputLevel OutputLevel {get;}
	}

	public interface IOutput
	/// Either output layer or output generator itself
	/// TODO: merge with output generator?
	{
		Generator Gen { get; } 
		void SetGen (Generator gen);
	}


	public interface IApplyData
	{
		void Apply (Terrain terrain);
		int Resolution {get;}
	}


	public interface IApplyDataRoutine : IApplyData
	{
		IEnumerator ApplyRoutine (Terrain terrain);
	}


	public interface ICustomComplexity
	/// To implement both Complexity and Progress properties
	{
		float Complexity { get; } //default is 1
		float Progress (TileData data);  //can be different from Complexity if the generator is partly done. Max is Complexity, min 0
	}

	public interface IOutdatedBiome
	{ 
		Graph SubGraph { get; }
	}

	public interface IBiome
	/// Passes the current graph commands to sub graph(s)
	{
		Graph AssignedGraph { get; }	//the graph that was initially assigned
		Graph SubGraph { get; }			//the graph used to generate. In most biomes they match (but not in function)
	}

	public interface IMultiBiome
	/// For layered biome generators
	{
		IEnumerable<IBiome> Biomes ();
	}


	public interface ICustomClear
	/// Additional clear actions are performed on clearing this
	/// Сalled no matter if it is ready or not
	{
		void OnBeforeClear (Graph graph, TileData data);
		void OnAfterClear (Graph graph, TileData data);
	}

	public interface ICustomDependence
	/// Makes PriorGens generated before generating this one
	/// Used in Portals
	{
		IEnumerable<Generator> PriorGens ();
	}


	public interface ILayered<out T> where T:class, new() 
	/// For gui purpose only. IDEA: make an attribute?
	/// Makes sense if layer is inlet or outlet, or IOutput, or IBiome. 
	/// For all other cases, not related with Generator, just use standard layer gui
	{ 
		T[] Layers { get; }
		void SetLayers(object[] layers);
	}

	public interface IInvLayered<out T> : ILayered<T> where T:class, new() { }
	/// Auto-implementing inverse order

	public interface INormalizableLayer : IInlet<MatrixWorld>, IOutlet<MatrixWorld> 
	/// Inlet-outlet layer that used in NormalizeGenerator.NormalizeLayers
	{ 
		float Opacity { get; }
	}

	public interface IExposedGuid
	/// Uses guid for value expose
	/// Could be either Generator or Layer
	{
		Guid Guid { get; }
		Generator Gen { get; } //to know what to refresh on value change
	}

	public interface IRelevant { } 
	/// Should be generated when generating graph

	public sealed class GeneratorMenuAttribute : Attribute
	{
		public string menu;
		public int section;
		public string name;
		public string iconName;
		public bool disengageable;
		public bool disabled;
		public int priority;
		public string helpLink;
		public bool drawInlets = true;
		public bool drawOutlet = true;
		public bool drawButtons = true;
		public Type colorType = null; ///> to display the node in the color of given outlet type
		public Type updateType;  ///> The class legacy generator updates to when clicking Update

		//these are assigned on load attribute in gen and should be null by default
		public string nameUpper;
		public float nameWidth; //the size of the nameUpper in pixels
		public Texture2D icon;
		public Type type;
		public Color color;
	}



	[System.Serializable]
	public abstract class Generator : IExposedGuid
	{
		public bool enabled = true;

		public Guid guid;  //for value expose. Auto-property won't serialize
		public Guid Guid => guid;

		public double draftTime;
		public double mainTime;

		public Vector2 guiPosition;
		public Vector2 guiSize;  //to add this node to group

		public bool guiPreview; //is preview for this generator opened


		//just to avoid implementing it in each generator
		public Generator Gen { get{ return this; } }
		public void SetGen (Generator gen) { }

		public static Generator Create (Type type, Graph graph)
		///Factory instead of constructor since could not create instance of abstract class
		{
			if (type.IsGenericTypeDefinition) type = type.MakeGenericType(typeof(Den.Tools.Matrices.MatrixWorld)); //if type is open generic type - creating the default matrix world
			
			Generator gen = (Generator)Activator.CreateInstance(type);
			gen.guid = Guid.NewGuid();

			if (gen is IMultiInlet multInGen)
				foreach (IInlet<object> inlet in multInGen.Inlets())
					inlet.SetGen(gen);

			if (gen is IMultiOutlet multOutGen)
				foreach (IOutlet<object> outlet in multOutGen.Outlets())
					outlet.SetGen(gen);

			gen.OnCreate(graph);

			return gen;
		}

		public virtual void OnCreate (Graph graph) { } //make some specific tasks (like internal graph creation for function)

		public abstract void Generate (TileData data, StopToken stop);
		/// The stuff generator does to read inputs (already prepared), generate, and write product(s). Does not affect previous generators, ready state or event, just the essence of generate


		#region Generic Type

			public static Type GetGenericType (Type type)
			/// Finds out if it's map, objects or splines node/inlet/outlet.
			/// Returns T if type is T, IOutlet<T>, Inlet<T>, or inherited from any of those (where T is MatrixWorls, TransitionsList or SplineSys, or OTHER type)
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type itype in interfaces)
				{
					if (!itype.IsGenericType) continue;
					//if (!typeof(IOutlet).IsAssignableFrom(itype) && !typeof(Inlet).IsAssignableFrom(itype)) continue;
					return itype.GenericTypeArguments[0];
				}

				return null;
			}

			//shotcuts to avoid evaluating by type
			public static Type GetGenericType<T> (IOutlet<T> outlet) where T: class  =>  typeof(T);
			public static Type GetGenericType<T> (IInlet<T> inlet) where T: class  =>  typeof(T);
			public static Type GetGenericType (Generator gen) 
			{
				if (gen is IOutlet<object> outlet) return GetGenericType(outlet);
				if (gen is IInlet<object> inlet) return GetGenericType(inlet);
				return null;
			}
			public static Type GetGenericType (IOutlet<object> outlet)
			{
				if (outlet is IOutlet<MatrixWorld>) return typeof(MatrixWorld);
				else if (outlet is IOutlet<TransitionsList>) return typeof(TransitionsList);
				else if (outlet is IOutlet<SplineSys>) return typeof(SplineSys);
				else return GetGenericType(outlet.GetType());
			}
			public static Type GetGenericType (IInlet<object> inlet)
			{
				if (inlet is IInlet<MatrixWorld>) return typeof(MatrixWorld);
				else if (inlet is IInlet<TransitionsList>) return typeof(TransitionsList);
				else if (inlet is IInlet<SplineSys>) return typeof(SplineSys);
				else return GetGenericType(inlet.GetType());
			}

		#endregion

		#region Normalize Layers

			public static void NormalizeLayers (INormalizableLayer[] layers, TileData data, StopToken stop)
			{
				//reading products
				MatrixWorld[] matrices = new MatrixWorld[layers.Length];
				float[] opacities = new float[layers.Length];

				if (stop!=null && stop.stop) return;
				for (int i=0; i<layers.Length; i++)
				{
					if (stop!=null && stop.stop) return;

					MatrixWorld srcMatrix = data.products.ReadInlet(layers[i]);
					if (srcMatrix != null) matrices[i] = new MatrixWorld(srcMatrix);

					//if (matrices[i] != null)
					//	matrices[i].Clamp01();

					opacities[i] = layers[i].Opacity;
				}

				//normalizing
				if (stop!=null && stop.stop) return;
				matrices.FillNulls(() => new MatrixWorld(data.area.full.rect, (Vector3)data.area.full.worldPos, (Vector3)data.area.full.worldSize));
				matrices[0].Fill(1);
				Matrix.BlendLayers(matrices, opacities);

				//saving products
				if (stop!=null && stop.stop) return;
				for (int i=0; i<layers.Length; i++)
					data.products[layers[i]] = matrices[i];
			}

		#endregion

		#region Serialization/Placeholders

			public Type AlternativeSerializationType
			{get{
				//if (this is IInlet<object> 
				return typeof(Placeholders.InletOutletPlaceholder);
			}}

		#endregion
	}
}