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
	public static class ValueRightClick
	{
		public static Item ValueItems (FieldInfo valField, IOutlet<object> layer, Generator gen, Graph graph, int priority=2)
		{
			Item valItems = new Item("Value");
			valItems.onDraw = RightClick.DrawItem;
			valItems.icon = RightClick.texturesCache.GetTexture("MapMagic/Popup/Value");
			valItems.color = Color.gray;
			valItems.subItems = new List<Item>();
			valItems.priority = priority;

			valItems.disabled = valField==null || gen==null;

			Item exposeItem = new Item("Expose", onDraw:RightClick.DrawItem, priority:6); 
			exposeItem.icon = RightClick.texturesCache.GetTexture("MapMagic/Popup/Expose");
			if (valField != null) exposeItem.onClick = // () => { if (gen.exposed==null) gen.exposed=new Exposed(); gen.exposed.ExposeField(valField); };
				() => ExposeWindow.ShowWindow(graph, gen, layer, valField, Event.current.mousePosition+EditorWindow.focusedWindow.position.position);  
				// graph.exposed.Expose(gen, valField);
			valItems.subItems.Add(exposeItem);

			Item unExposeItem = new Item("UnExpose", onDraw:RightClick.DrawItem, priority:6); 
			unExposeItem.icon = RightClick.texturesCache.GetTexture("MapMagic/Popup/UnExpose");
			if (valField != null) unExposeItem.onClick = //() => { if (gen.exposed==null) gen.exposed=new Exposed(); gen.exposed.ExposeField(valField); };
				() => 
				{
					graph.exposed.Unexpose(gen, valField);
					GraphWindow.current.Focus();
					GraphWindow.current.Repaint();
				};
			valItems.subItems.Add(unExposeItem);

			return valItems;
		}
	}
}