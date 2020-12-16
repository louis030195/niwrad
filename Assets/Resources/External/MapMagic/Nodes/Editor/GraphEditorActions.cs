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
	public static class GraphEditorActions
	/// Editor implementation (with undo, messages, mouse pos, stuff) of the graph actions
	{
		public static void RemoveGenerator (this Graph graph, Generator gen, HashSet<Generator> selected)
		{
			if (selected==null || selected.Count==0)
				RemoveGenerator(graph, gen);

			else if (selected.Contains(gen)) //removing selected only if gen is amoung them
				RemoveGenerators(graph, selected);
		}


		public static void RemoveGenerator (this Graph graph, Generator gen)
		{
			if (gen is IOutputGenerator  &&  GraphWindow.current.mapMagic != null)
			{
				if (EditorUtility.DisplayDialog("Purge Output", "Would you like to clear generated results on the terrain as well?", "Clear", "Keep"))
					GraphWindow.current.mapMagic.Purge((IOutputGenerator)gen);
			}

			GraphWindow.RecordCompleteUndo();

			if (graph.ContainsGenerator(gen))
			{
				graph.ThroughLink(gen); //trying to maintain connections between generators
				graph.Remove(gen);
			}

			GraphWindow.current.Focus();
			GraphWindow.current.Repaint();

			GraphWindow.RefreshMapMagic();
		}


		public static void RemoveGenerators (this Graph graph, HashSet<Generator> selected)
		{
			bool isOutput = false;
			foreach (Generator gen in selected)
				if (gen is IOutputGenerator)
					{ isOutput=true; break; }

			if (isOutput  &&  
				GraphWindow.current.mapMagic != null  &&
				EditorUtility.DisplayDialog("Purge Output", "Would you like to clear generated results on the terrain as well?", "Clear", "Keep"))
			{
				foreach (Generator gen in selected)
					if (gen is IOutputGenerator outGen)
						GraphWindow.current.mapMagic.Purge(outGen);
			}

			GraphWindow.RecordCompleteUndo();

			foreach (Generator sgen in selected)
			{
				if (graph.ContainsGenerator(sgen))
					graph.Remove(sgen);
			}

			GraphWindow.current.Focus();
			GraphWindow.current.Repaint();

			GraphWindow.RefreshMapMagic();
		}


		public static void CreateGenerator (this Graph graph, Type type, Vector2 mousePos)
		{
			GraphWindow.RecordCompleteUndo();

			Generator gen = Generator.Create(type, graph);
			gen.guiPosition = new Vector2(mousePos.x-GeneratorDraw.nodeWidth/2, mousePos.y-GeneratorDraw.headerHeight/2);
			graph.Add(gen);

			GraphWindow.current.Focus(); //to return focus from right-click menu
			GraphWindow.current.Repaint();

			//GraphWindow.RefreshMapMagic();
			//no need to refresh since added node isn't linked anywhere
		}


		public static void DuplicateGenerator (this Graph graph, Generator gen, ref HashSet<Generator> selected)
		{
			if (selected==null || selected.Count==0)
				DuplicateGenerator(graph, gen);

			else if (selected.Contains(gen)) //removing selected only if gen is among them
				DuplicateGenerators(graph, ref selected);
		}

		public static void DuplicateGenerator (this Graph graph, Generator gen)
		{
			HashSet<Generator> gens = new HashSet<Generator>();
			gens.Add(gen);
			DuplicateGenerators(graph, ref gens);
		}

		public static void DuplicateGenerators (this Graph graph, ref HashSet<Generator> gens)
		/// Changes the selected hashset
		{
			GraphWindow.RecordCompleteUndo();

			if (gens.Count == 0) return;

			Generator[] duplicatedGens = graph.Duplicate(gens);

			//placing under source generators
			Rect selectionRect = new Rect(duplicatedGens[0].guiPosition, Vector2.zero);
			foreach (Generator gen in duplicatedGens)
				selectionRect = selectionRect.Encapsulate( new Rect(gen.guiPosition, gen.guiSize) );
			foreach (Generator gen in duplicatedGens)
				gen.guiPosition.y += selectionRect.size.y + 20;

			gens.Clear();
			gens.AddRange(duplicatedGens);
			
			GraphWindow.current.Focus(); //to return focus from right-click menu
			GraphWindow.current.Repaint();

			GraphWindow.RefreshMapMagic();
		}
	}
}