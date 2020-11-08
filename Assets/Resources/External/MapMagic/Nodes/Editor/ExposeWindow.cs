using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Profiling;

using Den.Tools;
using Den.Tools.GUI;

using MapMagic.Core;
using MapMagic.Nodes;
using MapMagic.Products;
using MapMagic.Previews;

namespace MapMagic.Nodes.GUI
{
	//[EditoWindowTitle(title = "MapMagic Graph")]  //it's internal Unity stuff
	public class ExposeWindow : EditorWindow
	{
		public Graph graph;
		public IExposedGuid obj;
		public FieldInfo field;

		public string customName;

		private UI ui = new UI();

		//public override void OnGUI(Rect rect) => ui.Draw(DrawParams);
		//public override Vector2 GetWindowSize() =>  new Vector2(200, 150);
		public void OnGUI () => ui.Draw(DrawParams);

		private void DrawParams () 
		{
			using (Cell.Padded(5,5,5,5))
			{
				using (Cell.LineStd) Draw.Field(ref customName, "Name");

				Cell.EmptyLinePx(10);

				using (Cell.LinePx(22)) 
				{
					Cell.EmptyRow();

					using (Cell.RowPx(70)) 
						if (Draw.Button("OK"))
						{
							graph.exposed.Expose(obj, field, customName);
							Close();
						}

					Cell.EmptyRowPx(10);

					using (Cell.RowPx(70)) 
						if (Draw.Button("Cancel"))
							Close();
				}
			}
		}

		public static void ShowWindow (Graph graph, IExposedGuid gen, IOutlet<object> layer, FieldInfo field, Vector2 pos)
		{
			ExposeWindow window = ScriptableObject.CreateInstance(typeof(ExposeWindow)) as ExposeWindow;

			if (layer != null  &&  layer is IExposedGuid expLayer) window.obj = expLayer;
			else window.obj = gen;

			window.graph = graph;
			window.field = field;
			window.customName = field.Name.Nicify();
			window.ShowUtility();

			Vector2 windowSize = new Vector2(300, 100);
			window.position = new Rect(
				pos - windowSize/2,
				windowSize);
		}
	}

}//namespace