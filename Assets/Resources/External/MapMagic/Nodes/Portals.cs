using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using MapMagic.Products;

namespace MapMagic.Nodes
{
	[GeneratorMenu (menu = "Map/Portals", name = "Enter", iconName = "GeneratorIcons/PortalIn")] public class MatrixPortalEnter : PortalEnter<Den.Tools.Matrices.MatrixWorld> { }
	[GeneratorMenu (menu = "Map/Portals", name ="Exit", iconName = "GeneratorIcons/PortalOut")] public class MatrixPortalExit : PortalExit<Den.Tools.Matrices.MatrixWorld> { }
	//[GeneratorMenu (menu = "Objects/Portals", name = "Enter", iconName = "GeneratorIcons/PortalIn")] public class ObjectsPortalEnter : PortalEnter<TransitionsList> { }
	//[GeneratorMenu (menu = "Objects/Portals", name ="Exit", iconName = "GeneratorIcons/PortalOut")] public class ObjectsPortalExit : PortalExit<TransitionsList> { }
	//[GeneratorMenu (menu = "Spline/Portals", name = "Enter", iconName = "GeneratorIcons/PortalIn")] public class SplinePortalEnter : PortalEnter<Plugins.Splines.SplineSys> { }
	//[GeneratorMenu (menu = "Spline/Portals", name ="Exit", iconName = "GeneratorIcons/PortalOut")] public class SplinePortalExit : PortalExit<Plugins.Splines.SplineSys> { }


	public interface IPortalEnter<out T> : IInlet<T> where T:class
	{
		string Name {get; set; }
	}


	public interface IPortalExit<out T> :  IOutlet<T> where T:class
	{
		IPortalEnter<T> Enter { get; }
		void AssignEnter (IPortalEnter<object> enter, Graph graph);
	}


	[System.Serializable]
	[GeneratorMenu(name = "Generic Portal Enter")]
	public class PortalEnter<T> : Generator, IInlet<T>, IPortalEnter<T>  where T: class, ICloneable
	{
		public string name = "Portal";
		public string Name { get{ return name; } set{ name = value; } }

		public override void Generate (TileData data, StopToken stop) { }
	}


	[Serializable]
	[GeneratorMenu (name ="Generic Portal Exit")]
	public class PortalExit<T> : Generator, IOutlet<T>, IPortalExit<T>, ICustomDependence   where T: class, ICloneable 
	{
		public PortalEnter<T> enter; //TODO: don't serialize, keep name only for serialization simplicity
		public IPortalEnter<T> Enter => enter;

		public void AssignEnter (IPortalEnter<object> ienter, Graph graph)
		{
			if (!(ienter is PortalEnter<T> enter)) return;
			//TODO: other validity check
			this.enter = enter;
		}

		public override void Generate (TileData data, StopToken stop) 
		{ 
			if (enter != null   &&  !stop.stop) 
			{
				data.products[this] = data.products.ReadInlet(enter);
				//TODO: clone?
			}
		
		}

		public IEnumerable<Generator> PriorGens () 
		{
			if (enter != null)
				yield return enter;
		}
	}
}