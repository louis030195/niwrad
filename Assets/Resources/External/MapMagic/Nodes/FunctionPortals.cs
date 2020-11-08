using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using MapMagic.Products;

namespace MapMagic.Nodes
{
	[GeneratorMenu (menu = "Map/Function", name = "Enter")] public class MatrixFunctionInput : FunctionInput<Den.Tools.Matrices.MatrixWorld> { }
	[GeneratorMenu (menu = "Map/Function", name = "Exit")] public class MatrixFunctionOutput : FunctionOutput<Den.Tools.Matrices.MatrixWorld> { }
	/*[GeneratorMenu (menu = "Objects/Function", name = "Enter")] public class ObjectsFunctionInput : FunctionInput<TransitionsList> { }
	[GeneratorMenu (menu = "Objects/Function", name = "Exit")] public class ObjectsFunctionOutput : FunctionOutput<TransitionsList> { }
	[GeneratorMenu (menu = "Spline/Function", name = "Enter")] public class SplineFunctionInput : FunctionInput<Den.Tools.Splines.SplineSys> { }
	[GeneratorMenu (menu = "Spline/Function", name = "Exit")] public class SplineFunctionOutput : FunctionOutput<Den.Tools.Splines.SplineSys> { }*/
	//initializing them twice instead - one time here, and one time (with attribute) in module
	public partial class ObjectsFunctionInput : FunctionInput<TransitionsList> { }
	public partial class ObjectsFunctionOutput : FunctionOutput<TransitionsList> { }
	//public partial class SplineFunctionInput : FunctionInput<Den.Tools.Segs.SplineSys> { }
	//public partial class SplineFunctionOutput : FunctionOutput<Den.Tools.Segs.SplineSys> { }
	public partial class SplineFunctionInput : FunctionInput<Den.Tools.Splines.SplineSys> { }
	public partial class SplineFunctionOutput : FunctionOutput<Den.Tools.Splines.SplineSys> { }

	//gui interfaces
	public interface IFunctionInput<out T> : IOutlet<T>  where T: class
	{ 
		string Name { get; set; }
	}
	public interface IFunctionOutput<out T> : IInlet<T> where T: class
	{ 
		string Name { get; set; }
	}


	[System.Serializable]
	[GeneratorMenu (name ="Function Input")]
	public class FunctionInput<T> : Generator, IFunctionInput<T>, IOutlet<T> where T: class
	{
		[Val("Name")]	public string name = "Input";
		public string Name { get{return name;} set{name=value;} }

		public override void Generate (TileData data, StopToken stop) {}
	}


	[Serializable]
	[GeneratorMenu (name ="Function Output")]
	public class FunctionOutput<T> : Generator, IInlet<T>, IOutlet<T>, IFunctionOutput<T> where T: class
	{
		[Val("Name")]	public string name = "Output";
		public string Name { get{return name;} set{name=value;} }

		public override void Generate (TileData data, StopToken stop) 
		{
			if (stop!=null && stop.stop) return;  

			//just passing link products to this
			object product = data.products.ReadInlet(this);
			data.products[this] = product;
		}
	}
}
