using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Profiling;
using UnityEditor;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.GUI.Popup;
using MapMagic.Core;
using MapMagic.Nodes;
using MapMagic.Nodes.GUI;

namespace MapMagic.Nodes.GUI
{
	public static class RightClick
	{
		private static GUIStyle itemTextStyle;
		public static TexturesCache texturesCache = new TexturesCache(); //to store icons


		public static void DrawRightClickItems (UI ui, Vector2 mousePos, Graph graph)
		{ 
			Item item = RightClickItems(ui, mousePos, graph);

			#if MM_EXP || UNITY_2020_1_OR_NEWER || UNITY_EDITOR_LINUX
			SingleWindow menu = new SingleWindow() {rootItem=item};
			#else
			PopupMenu menu = new PopupMenu() {items=item.subItems, minWidth=150};
			#endif

			menu.Show(Event.current.mousePosition);
		}


		public static Item RightClickItems (UI ui, Vector2 mousePos, Graph graph)
		{
			ClickedNear (ui, mousePos, 
				out Group clickedGroup, 
				out Generator clickedGen, 
				out IOutlet<object> clickedLayer, 
				out IInlet<object> clickedLink, 
				out IInlet<object> clickedInlet, 
				out IOutlet<object> clickedOutlet, 
				out FieldInfo clickedField);

			Item menu = new Item("Menu");
			menu.subItems = new List<Item>();

			if (clickedOutlet != null)
				menu.subItems.Add( CreateRightClick.AppendItems(mousePos, graph, clickedOutlet, priority:5) );

			else if (clickedLink != null)
				menu.subItems.Add( CreateRightClick.InsertItems(mousePos, graph, clickedLink, priority:5) );

			else
				menu.subItems.Add( CreateRightClick.CreateItems(mousePos, graph, priority:5) );

			menu.subItems.Add( GeneratorRightClick.GeneratorItems(clickedGen, graph, priority:4) );
			menu.subItems.Add( GroupRightClick.GroupItems(mousePos, clickedGroup, graph, priority:3) );
			menu.subItems.Add( ValueRightClick.ValueItems(clickedField, clickedLayer, clickedGen, graph, priority:2) );
			menu.subItems.Add( GraphPopup.GraphItems(graph, priority:1) );

			return menu;
		}


		public static bool ClickedNear (UI ui, Vector2 mousePos, 
			out Group clickedGroup, 
			out Generator clickedGen, 
			out IOutlet<object> clickedLayer,
			out IInlet<object> clickedLink,
			out IInlet<object> clickedInlet, 
			out IOutlet<object> clickedOutlet, 
			out FieldInfo clickedField)
		/// Returns the top clicked object (or null) in clickedGroup-to-clickedField priority
		{
			clickedGroup = null;
			clickedGen = null;
			clickedLayer = null;
			clickedLink = null;
			clickedInlet = null; 
			clickedOutlet = null; 
			clickedField = null;

			List<Cell> cellsUnderCursor = new List<Cell>();
			ui.rootCell.FillCellsUnderCursor(cellsUnderCursor, mousePos);

			//checking cells
			for (int i=0; i<cellsUnderCursor.Count; i++)
			{
				Cell cell = cellsUnderCursor[i];

				//GeneratorDraw.genCellLut.TryGetValue(cell, out clickedGen);  //TryGet will overwrite to null if not found

				if (ui.cellObjs.TryGetObject(cell, "Generator", out Generator gen)) clickedGen = gen;
				if (ui.cellObjs.TryGetObject(cell, "Group", out Group group)) clickedGroup = group;
				if (ui.cellObjs.TryGetObject(cell, "Layer", out IOutlet<object> layer)) clickedLayer = layer;
				if (ui.cellObjs.TryGetObject(cell, "Inlet", out IInlet<object> inlet)) clickedInlet = inlet;
				if (ui.cellObjs.TryGetObject(cell, "Outlet", out IOutlet<object> outlet)) clickedOutlet = outlet;
				if (ui.cellObjs.TryGetObject(cell, "Field", out FieldInfo field)) clickedField = field;

				if (clickedGen != null  &&  clickedOutlet == null  &&  clickedGen is IOutlet<object> o)
					clickedOutlet = o; 
					//assigning outlet if clicked on single-outlet gen 
			}

			//checking links
			float minDist = 10/ui.scrollZoom.zoom; //10 pixels is max dist to link
			foreach (var kvp in GraphWindow.current.graph.links)
			{
				float dist = GeneratorDraw.DistToLink(mousePos, kvp.Value, kvp.Key);
				if (dist < minDist) 
					{minDist = dist; clickedLink=kvp.Key;} 
			}

			return clickedGroup != null ||  clickedGen != null || clickedLayer != null || clickedLink != null || clickedInlet != null || clickedOutlet != null || clickedField != null;
		}


		public static object ClickedOn (UI ui, Vector2 mousePos)
		/// Returns the top clicked object (or null) in clickedGroup-to-clickedField priority
		{
			ClickedNear (ui, mousePos, 
				out Group clickedGroup, out Generator clickedGen, out IOutlet<object> clickedLayer, out IInlet<object> clickedLink, out IInlet<object> clickedInlet, out IOutlet<object> clickedOutlet, out FieldInfo clickedField);

			if (clickedField != null) return clickedField;
			if (clickedOutlet != null) return clickedOutlet;
			if (clickedInlet != null) return clickedInlet;
			if (clickedLink != null) return clickedLink;
			if (clickedLayer != null) return clickedLayer;
			if (clickedGen != null) return clickedGen;
			if (clickedGroup != null) return clickedGroup;

			return null;
		}


		public static void DrawItem (Item item, Rect rect)
		{
			Rect leftRect = new Rect(rect.x, rect.y, 28, rect.height);
			leftRect.x -= 1; leftRect.height += 2;
			item.color.a = 0.25f;
			EditorGUI.DrawRect(leftRect, item.color);

			Rect labelRect = new Rect(rect.x+leftRect.width+3, rect.y, rect.width-leftRect.width-3, rect.height);

			if (itemTextStyle == null)
			{
				itemTextStyle = new GUIStyle(UnityEditor.EditorStyles.label); 
				itemTextStyle.normal.textColor = itemTextStyle.focused.textColor = itemTextStyle.active.textColor = Color.black;
			}

			EditorGUI.LabelField(labelRect, item.name, itemTextStyle);

			if (item.icon!=null) 
			{
				Rect iconRect = new Rect(leftRect.center.x-6, leftRect.center.y-6, 12,12);
				iconRect.y -= 2;
				UnityEngine.GUI.DrawTexture(iconRect, item.icon);
			}
		}


		public static void DrawSeparator (Item item, Rect rect)
		{
			Rect leftRect = new Rect(rect.x, rect.y, 28, rect.height);
			leftRect.x -= 1; leftRect.height += 2;
			item.color.a = 0.125f;
			EditorGUI.DrawRect(leftRect, item.color);

			Rect labelRect = new Rect(rect.x+leftRect.width+3, rect.y, rect.width-leftRect.width-3, rect.height);
			Rect separatorRect = new Rect(labelRect.x, labelRect.y+2, labelRect.width-6, 1);
			EditorGUI.DrawRect(separatorRect, new Color(0.3f, 0.3f, 0.3f, 1));
		}
	}
}