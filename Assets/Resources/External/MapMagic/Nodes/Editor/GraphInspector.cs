using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using MapMagic.Core;
using MapMagic.Core.GUI;

namespace MapMagic.Nodes.GUI
{
	[CustomEditor(typeof(Graph))]
	//[InitializeOnLoad]  
	public class GraphInspector : Editor
	{
		Graph graph; //aka target
		UI ui = new UI();

		bool showDependent;
		bool showShared;
		bool showExposed;

		/*public void OnEnable () 
		{
			SceneView.onSceneGUIDelegate -= DragGraphToScene;
			SceneView.onSceneGUIDelegate += DragGraphToScene;
		}*/

		static HashSet<string> allGraphsGuids;

		[RuntimeInitializeOnLoadMethod, UnityEditor.InitializeOnLoadMethod] 
		static void Subscribe()
		{
			#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= DragGraphToScene;
			SceneView.duringSceneGui += DragGraphToScene;
			#else
			SceneView.onSceneGUIDelegate -= DragGraphToScene;
			SceneView.onSceneGUIDelegate += DragGraphToScene;
			#endif
			
			allGraphsGuids = new HashSet<string>(AssetDatabase.FindAssets("t:Graph"));   //compiling all graph guids to display graph icons
			EditorHacks.SubscribeToListIconDrawCallback(DrawListIcon);	
			EditorHacks.SubscribeToTreeIconDrawCallback(DrawTreeIcon);		
		}

		static void DragGraphToScene (SceneView sceneView)
		{
			UnityEngine.Object[] draggedObjs = DragAndDrop.objectReferences;
			if (draggedObjs == null || draggedObjs.Length != 1 || !(draggedObjs[0] is Graph)) return;

			Graph graph = (Graph)draggedObjs[0];

			if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // show a drag-add icon on the mouse cursor
				
			}

			if (Event.current.type == EventType.DragPerform)
			{
				DragAndDrop.AcceptDrag();
				MapMagicInspector.CreateMapMagic(graph);
			}
		}

		public override void  OnInspectorGUI ()
		{
			graph = (Graph)target;

			if (ui.undo == null) ui.undo = new Den.Tools.GUI.Undo();
			ui.undo.undoObject = graph;
			ui.undo.undoName = "MapMagic Graph Settings";

			ui.Draw(DrawGUI);	
		}

		public void DrawGUI ()
		{
				using (Cell.LinePx(32))
					Draw.Label("WARNING: Keeping this asset selected in \nInspector can slow down editor GUI performance.", style:UI.current.styles.helpBox);
				Cell.EmptyLinePx(5);

				using (Cell.LinePx(24)) if ( Draw.Button("Open Editor"))
					GraphWindow.Show(graph);
				using (Cell.LinePx(20)) if ( Draw.Button("Open in New Tab"))
					GraphWindow.ShowInNewTab(graph);

				//seed
				Cell.EmptyLinePx(5);
				using (Cell.LineStd)
				{
					int newSeed = Draw.Field(graph.random.Seed, "Seed"); //
					if (newSeed != graph.random.Seed)
					{
						graph.random.Seed = newSeed;
						//Graph.OnChange.Raise(graph);
					} 
				}

				using (Cell.LineStd) Draw.DualLabel("Nodes", graph.generators.Length.ToString());
				using (Cell.LineStd) Draw.DualLabel("MapMagic ver", graph.serializedVersion.ToString());
				Cell.EmptyLinePx(5);


				//global values
				/*using (Cell.LineStd)
					using (new Draw.FoldoutGroup (ref showShared, "Global Values"))
						if (showShared)
					{
						List<string> changedNames = new List<string>();
						List<object> changedVals = new List<object>();

						(Type type, string name)[] typeNames = graph.sharedVals.GetTypeNames();
						for (int i=0; i<typeNames.Length; i++)
							using (Cell.LineStd) GeneratorDraw.DrawGlobalVar(typeNames[i].type, typeNames[i].name);

						if (Cell.current.valChanged)
						{
							GraphWindow.current.mapMagic.ClearAllNodes();
							GraphWindow.current.mapMagic.StartGenerate();
						}	
					}*/

				//exposed values
				using (Cell.LineStd)
					using (new Draw.FoldoutGroup (ref showExposed, "Exposed Values"))
						if (showExposed)
					{
						graph.exposed.ClearObsoleteEntries(graph);

						if (graph.exposed.entries != null)
							for (int e=0; e<graph.exposed.entries.Length; e++)
							{
								Exposed.Entry entry = graph.exposed.entries[e];
								IExposedGuid gen = graph.FindGenByGuid(entry.guid);
								FieldInfo field = gen.GetType().GetField(entry.fieldName);

								using (Cell.LineStd)
								{
									if (field==null)
										Draw.DualLabel(entry.guiName, "unknown");

									else
										Draw.ClassField(
											field: field, 
											type: entry.type, 
											obj: gen,
											name: entry.guiName);
								}

								if (Cell.current.valChanged)
									GraphWindow.RefreshMapMagic();
							}
					}

				//dependent graphs
				using (Cell.LineStd)
					using (new Draw.FoldoutGroup (ref showDependent, "Dependent Graphs"))
						if (showDependent)
						{
							using (Cell.LinePx(0))
								DrawDependentGraphs(graph);
						}
		}

		private void DrawDependentGraphs (Graph graph)
		/// Draws subgraphs recursively
		{
			foreach (Graph subGraph in graph.SubGraphs())
			{
				using (Cell.LineStd)
				{
					using (Cell.Row) Draw.Label(subGraph.name);
					using (Cell.RowPx(100)) Draw.ObjectField(subGraph);
				}

				using (Cell.LinePx(0))
				{
					Cell.EmptyRowPx(10);
					DrawDependentGraphs(subGraph);
				}
			}
		}



		#region Create Template Graph

			//empty graph is created viaCreateAssetMenuAttribute,
			//but unfortunately there's only one attribute per class

			[MenuItem("Assets/Create/MapMagic/Template Graph", priority = 102)]
			static void MenuCreateMapMagicGraph(MenuCommand menuCommand)
			{
				ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
					0, 
					ScriptableObject.CreateInstance<TmpCallbackReciever>(), 
					"MapMagic Graph.asset", 
					TexturesCache.LoadTextureAtPath("MapMagic/Icons/AssetBig"), 
					null);
			}

			class TmpCallbackReciever : UnityEditor.ProjectWindowCallback.EndNameEditAction
			{
				public override void Action(int instanceId, string pathName, string resourceFile)
				{
					Graph graph = CreateTemplate();
					graph.name = System.IO.Path.GetFileName(pathName);
					AssetDatabase.CreateAsset(graph, pathName);

					ProjectWindowUtil.ShowCreatedAsset(graph);

					allGraphsGuids = new HashSet<string>(AssetDatabase.FindAssets("t:Graph"));
				} 
			}

			public static Graph CreateTemplate ()
			{
				Graph graph = CreateInstance<Graph>();

				MatrixGenerators.Noise200 noise = (MatrixGenerators.Noise200)Generator.Create(typeof(MatrixGenerators.Noise200), graph);
				graph.Add(noise);
				noise.guiPosition = new Vector2(-270,-100);

				MatrixGenerators.Erosion200 erosion = (MatrixGenerators.Erosion200)Generator.Create(typeof(MatrixGenerators.Erosion200), graph);
				graph.Add(erosion);
				erosion.guiPosition = new Vector2(-70,-100);
				graph.Link(erosion, noise);

				MatrixGenerators.HeightOutput200 output = (MatrixGenerators.HeightOutput200)Generator.Create(typeof(MatrixGenerators.HeightOutput200), graph);
				graph.Add(output);
				output.guiPosition = new Vector2(130, -100);
				graph.Link(output, erosion);

				return graph;
			}


			/*[MenuItem("Assets/Create/MapMagic/PerfTest Graph", priority = 102)]
			static void MenuCreatePerfTestGraph(MenuCommand menuCommand)
			{
				ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
					0, 
					ScriptableObject.CreateInstance<TmpCallbackRecieverBig>(), 
					"PefrfTest Graph.asset", 
					TexturesCache.LoadTextureAtPath("MapMagic/Icons/AssetBig"), 
					null);
			}

			class TmpCallbackRecieverBig : UnityEditor.ProjectWindowCallback.EndNameEditAction
			{
				public override void Action(int instanceId, string pathName, string resourceFile)
				{
					Graph graph = CreateBig();
					graph.name = System.IO.Path.GetFileName(pathName);
					AssetDatabase.CreateAsset(graph, pathName);

					ProjectWindowUtil.ShowCreatedAsset(graph);

					allGraphsGuids = new HashSet<string>(AssetDatabase.FindAssets("t:Graph"));
				} 
			}

			public static Graph CreateBig ()
			{
				Graph graph = CreateInstance<Graph>();

				for (int j=0; j<10; j++)
				{
					MatrixGenerators.Noise200 noise = (MatrixGenerators.Noise200)Generator.Create(typeof(MatrixGenerators.Noise200), graph);
					graph.Add(noise);
					noise.guiPosition = new Vector2(-270,-100 + j*200);

					MatrixGenerators.Terrace200 terrace = null;
					for (int i=0; i<98; i++)
					{
						MatrixGenerators.Terrace200 newTerrace = (MatrixGenerators.Terrace200)Generator.Create(typeof(MatrixGenerators.Terrace200), graph);
						graph.Add(newTerrace);
						newTerrace.guiPosition = new Vector2(-70 + 200*i,-100 + j*200);
						if (i==0) graph.Link(newTerrace, noise);
						else graph.Link(newTerrace, (IOutlet<object>)terrace);
						terrace = newTerrace;
					}

					MatrixGenerators.HeightOutput200 output = (MatrixGenerators.HeightOutput200)Generator.Create(typeof(MatrixGenerators.HeightOutput200), graph);
					graph.Add(output);
					output.guiPosition = new Vector2(130 + 200*98, -100 + j*200);
					graph.Link(output, terrace);
				}

				return graph;
			}*/

		#endregion


		public static void DrawListIcon (Rect iconRect, string guid, bool isListMode)
		{
			if (!allGraphsGuids.Contains(guid)) return;
			Texture2D icon = TexturesCache.LoadTextureAtPath("MapMagic/Icons/AssetBig");
			UnityEngine.GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
		}


		public static void DrawTreeIcon (Rect iconRect, string guid)
		{
			if (!allGraphsGuids.Contains(guid)) return;

			if (!BuildPipeline.isBuildingPlayer) //otherwise will log an error during build that cannot find AssetSmall icon in built resources
			{
				Texture2D icon = TexturesCache.LoadTextureAtPath("MapMagic/Icons/AssetSmall");
				UnityEngine.GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
			}
		}
	}
}