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
	public class GraphWindow : EditorWindow
	{
		public static GraphWindow current;  //assigned each gui draw (and nulled after)

		//public List<Graph> graphs = new List<Graph>();
		//public Graph CurrentGraph { get{ if (graphs.Count==0) return null; else return graphs[graphs.Count-1]; }}
		//public Graph RootGraph { get{ if (graphs.Count==0) return null; else return graphs[0]; }}

		public Graph graph;
		public List<Graph> parentGraphs;   //the ones on pressing "up level" button
											//we can have the same function in two biomes. Where should we exit on pressing "up level"?
											//automatically created on opening window, though

		private bool drawAddRemoveButton = true;  //turning off Add/Remove on opening popup with it, and re-enabling once the graph window is focused again

		public static Dictionary<Graph,Vector3> graphsScrollZooms = new Dictionary<Graph, Vector3>();
		//to remember the stored graphs scroll/zoom to switch between graphs
		//public for snapshots

		public MapMagicObject mapMagic;

		public static MapMagicObject RelatedMapMagic 
		{get{
			if (current == null  ||  current.mapMagic == null  ||  current.mapMagic.graph == null) return null;
			if (current.mapMagic.graph != current.graph  &&  !current.mapMagic.graph.ContainsSubGraph(current.graph)) return null;
			return current.mapMagic;
		}}

		public static void RefreshMapMagic () => RefreshMapMagic(true, null, null); 
		public static void RefreshMapMagic (Generator gen) => RefreshMapMagic(false, gen, null);
		public static void RefreshMapMagic (Generator gen1, Generator gen2) => RefreshMapMagic(false, gen1, gen2);
		private static void RefreshMapMagic (bool all, Generator gen1, Generator gen2)
		/// makes current mapMagic to generate
		/// if gen not specified forcing re-generate
		{
			GraphWindow.current.graph.changeVersion++;

			if (all) RelatedMapMagic?.Refresh();
			else RelatedMapMagic?.Refresh(gen1, gen2);

			EditorUtility.SetDirty(current.graph);
		}

		public static void RecordCompleteUndo ()
		{
			current.graphUI.undo.Record(completeUndo:true);
		}
		//the usual undo is recorded on valChange via gui

		const int toolbarSize = 20;

		public UI graphUI = UI.ScrolledUI(); 
		UI toolbarUI = new UI();
		UI dragUI = new UI();

		bool wasGenerating = false; //to update final frame when generate is finished
		
		private static Vector2 addDragTo = new Vector2(Screen.width-50,20);
		private static Vector2 AddDragDefault {get{ return new Vector2(Screen.width-50,20); }}
		private const int addDragSize = 34;
		private const int addDragOffset = 20; //the offset from screen corner
		private static readonly object addDragId = new object();

		private Vector2 addButtonDragOffset;

		public HashSet<Generator> selected = new HashSet<Generator>();

		private long lastFrameTime;


		public static MapMagicObject GetRelatedMapMagic (Graph graph)
		{
			MapMagicObject[] allMM = GameObject.FindObjectsOfType<MapMagicObject>();
			for (int m=0; m<allMM.Length; m++)
				if (allMM[m].ContainsGraph(graph)) return allMM[m];
			return null;
		}

		public void OnEnable () 
		{
			//redrawing previews
			//Preview.OnRefreshed += p => Repaint();

			#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnSceneGUI;
			SceneView.duringSceneGui += OnSceneGUI;
			#else
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			#endif

			ScrollZoomOnOpen(); //focusing after script re-compile
		}

		public void OnDisable () 
		{
			#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui -= OnSceneGUI;
			#else
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			#endif

			 UnityEditor.Tools.hidden = false; //in case gizmo node is turned on
		}

		public void OnInspectorUpdate () 
		{
			current = this;

			//updating gauge
			if (mapMagic == null) return;
			bool isGenerating = mapMagic.IsGenerating()  &&  mapMagic.ContainsGraph(graph);
			if (wasGenerating) { Repaint(); wasGenerating=false; } //1 frame delay after generate is finished
			if (isGenerating) { Repaint(); wasGenerating=true; }
		}


		private void OnGUI()
		{
			current = this;
			mapMagic = GetRelatedMapMagic(graph);

			if (graph==null || graph.generators==null) return;

			if (graphUI.undo == null) 
			{
				graphUI.undo = new Den.Tools.GUI.Undo() { undoObject = graph , undoName = "MapMagic Graph Change" };
				graphUI.undo.undoAction = GraphWindow.RefreshMapMagic;
			}
			graphUI.undo.undoObject = graph;

			BeginWindows();  
			//using (Timer.Start("UI_Graph"))
				graphUI.DrawInSubWindow(DrawGraph, 1, new Rect(0, toolbarSize, position.width, position.height-toolbarSize) );
			//using (Timer.Start("UI_Toolbar"))
				toolbarUI.DrawInSubWindow(DrawToolbar, 2, new Rect(0, 0, position.width, toolbarSize));
			EndWindows();

			//graphUI.Draw(DrawGraph);
			//toolbarUI.Draw(DrawToolbar);

			//storing graph pivot to focus it on load
			Vector3 scrollZoom = graphUI.scrollZoom.GetWindowCenter(position.size);
			scrollZoom.z = graphUI.scrollZoom.zoom;
			if (graphsScrollZooms.ContainsKey(graph)) graphsScrollZooms[graph] = scrollZoom;
			else graphsScrollZooms.Add(graph, scrollZoom);

			//debug mouse pos
			//Draw.DebugMousePos();

			//switching to main on field drag release
			//mapMagic.guiDraggingField = DragDrop.obj!=null && (DragDrop.group=="DragField" || DragDrop.group=="DragCurve" || DragDrop.group=="DragLevels");
			if (mapMagic != null)
			{
				bool newForceDrafts = DragDrop.obj!=null && (DragDrop.group=="DragField" || DragDrop.group=="DragCurve" || DragDrop.group=="DragLevels"); 
				if (!newForceDrafts  &&  mapMagic.guiDraggingField)
				{
					mapMagic.guiDraggingField = newForceDrafts;
					mapMagic.SwitchLods();
				}
				mapMagic.guiDraggingField = newForceDrafts;
			}
			

			//showing fps
			/*long currentFrameTime = System.Diagnostics.Stopwatch.GetTimestamp();
			float timeDelta = 1f * (currentFrameTime-lastFrameTime) / System.Diagnostics.Stopwatch.Frequency;
			lastFrameTime = currentFrameTime;
			float fps = 1 / timeDelta;
			EditorGUI.LabelField(new Rect(position.x+position.width-70, 0, 70, 18), "FPS:" + fps.ToString("0.0"));*/
		}


		private void DrawGraph () 
//			{using (Timer.Start("DrawGraph"))
		{
				//background
				#if MM_DOC
					float gridColor = 0.25f;
					float gridBackgroundColor = 0.25f;
				#else
					float gridColor = !StylesCache.isPro ? 0.45f : 0.14f; //0.135f; 
					float gridBackgroundColor = !StylesCache.isPro ? 0.5f : 0.16f; //0.16f;
				#endif

				Draw.StaticGrid(
					displayRect: new Rect(0, 0, Screen.width, Screen.height-toolbarSize),
					cellSize:32,
					color:new Color(gridColor,gridColor,gridColor), 
					background:new Color(gridBackgroundColor,gridBackgroundColor,gridBackgroundColor),
					fadeWithZoom:true);


				//drawing groups
				foreach (Group group in graph.groups)
					using (Cell.Custom(group.guiPos.x, group.guiPos.y, group.guiSize.x, group.guiSize.y))
					{
						GroupDraw.DragGroup(group, graph.generators);
						GroupDraw.DrawGroup(group);
					}


				//dragging nodes
				foreach (Generator gen in graph.generators)
					GeneratorDraw.DragGenerator(gen, selected);


				//drawing links
				//using (Timer.Start("Links"))
				if (!UI.current.layout)
				{
					List<(IInlet<object> inlet, IOutlet<object> outlet)> linksToRemove = null;
					foreach (var kvp in graph.links)
					{
						IInlet<object> inlet = kvp.Key;
						IOutlet<object> outlet = kvp.Value;

						Cell outletCell = UI.current.cellObjs.GetCell(outlet, "Outlet");
						Cell inletCell = UI.current.cellObjs.GetCell(inlet, "Inlet");

						if (outletCell == null || inletCell == null)
						{
							Debug.LogError("Could not find a cell for inlet/outlet. Removing link");
							if (linksToRemove == null) linksToRemove = new List<(IInlet<object> inlet, IOutlet<object> outlet)>();
							linksToRemove.Add((inlet,outlet));
							continue;
						}

						GeneratorDraw.DrawLink(
							GeneratorDraw.StartCellLinkpos(outletCell),
							GeneratorDraw.EndCellLinkpos(inletCell), 
							GeneratorDraw.GetLinkColor(inlet) );
					}

					if (linksToRemove != null)
						foreach ((IInlet<object> inlet, IOutlet<object> outlet) in linksToRemove)
						{
							graph.UnlinkInlet(inlet);
							graph.UnlinkOutlet(outlet);
						}
				}

				//removing null generators (for test purpose)
				for (int n=graph.generators.Length-1; n>=0; n--)
				{
					if (graph.generators[n] == null)
						ArrayTools.RemoveAt(ref graph.generators, n);
				}

				//drawing generators
				//using (Timer.Start("Generators"))
				foreach (Generator gen in graph.generators)
					using (Cell.Custom(gen.guiPosition.x, gen.guiPosition.y, GeneratorDraw.nodeWidth, 0))
					{
						if (gen is IPortalEnter<object> || gen is IPortalExit<object> || gen is IFunctionInput<object> || gen is IFunctionOutput<object>) 
							GeneratorDraw.DrawPortal(gen, graph, selected:selected.Contains(gen));

						else
						{
							try { GeneratorDraw.DrawGenerator(gen, graph, selected:selected.Contains(gen)); }
							catch (ExitGUIException)
								{ } //ignoring
							catch (Exception e) 
								{ Debug.LogError("Draw Graph Window failed: " + e); }
						}
					}

				//de-selecting nodes (after dragging and drawing since using drag obj)
				if (!UI.current.layout)
				{
					GeneratorDraw.SelectGenerators(selected);
					GeneratorDraw.DeselectGenerators(selected);
				}
				
				//add/remove button
				//using (Timer.Start("AddRemove"))
				using (Cell.Full)
					DragDrawAddRemove();

				//right click menu (should have access to cellObjs)
				if (!UI.current.layout  &&  Event.current.type == EventType.MouseDown  &&  Event.current.button == 1)
					RightClick.DrawRightClickItems(graphUI, graphUI.mousePos, graph);

				//create menu on space
				if (!UI.current.layout  &&  Event.current.type == EventType.KeyDown  &&  Event.current.keyCode == KeyCode.Space  && !Event.current.shift)
					CreateRightClick.DrawCreateItems(graphUI.mousePos, graph);

				//delete selected generators
				if (selected!=null  &&  selected.Count!=0  &&  Event.current.type==EventType.KeyDown  &&  Event.current.keyCode==KeyCode.Delete)
					GraphEditorActions.RemoveGenerators(graph, selected);
		}



		private void DrawToolbar () 
		{ 
			//using (Timer.Start("DrawToolbar"))

			using (Cell.LinePx(toolbarSize))
			{
				//Graph graph = CurrentGraph;
				//Graph rootGraph = mapMagic.graph;

				//if (mapMagic != null  &&  mapMagic.graph!=graph  &&  mapMagic.graph!=rootGraph) mapMagic = null;

				UI.current.styles.Resize(0.9f);  //shrinking all font sizes

				Draw.Element(UI.current.styles.toolbar);

	
				//undefined graph
				if (graph==null)
				{
					using (Cell.RowPx(200)) Draw.Label("No graph selected to display. Select:");
					using (Cell.RowPx(100)) Draw.ObjectField(ref graph);
					return;
				}

				//if graph loaded corrupted
				if (graph.generators==null) 
				{
					using (Cell.RowPx(300)) Draw.Label("Graph is null. Check the console for the error on load.");

					using (Cell.RowPx(100))
						if (Draw.Button("Reload", style:UI.current.styles.toolbarButton)) graph.OnAfterDeserialize();

					using (Cell.RowPx(100))
					{
						if (Draw.Button("Reset", style:UI.current.styles.toolbarButton)) graph.generators = new Generator[0];
					}
					
					Cell.EmptyRowRel(1);

					return;
				}

				//root graph
				Graph rootGraph = null;
				if (parentGraphs != null  &&  parentGraphs.Count != 0) 
					rootGraph = parentGraphs[0];
					//this has nothing to do with currently assigned mm graph - we can view subGraphs with no mm in scene at all

				if (rootGraph != null)
				{
					Vector2 rootBtnSize = UnityEngine.GUI.skin.label.CalcSize( new GUIContent(rootGraph.name) );
					using (Cell.RowPx(rootBtnSize.x))
					{
						//Draw.Button(graph.name, style:UI.current.styles.toolbarButton, cell:rootBtnCell);
						Draw.Label(rootGraph.name);
							if (Draw.Button("", visible:false))
								EditorGUIUtility.PingObject(rootGraph);
					}
				
					using (Cell.RowPx(20)) Draw.Label(">>"); 
				}

				//this graph
				Vector2 graphBtnSize = UnityEngine.GUI.skin.label.CalcSize( new GUIContent(graph.name) );
				using (Cell.RowPx(graphBtnSize.x))
				{
					Draw.Label(graph.name);
					if (Draw.Button("", visible:false))
						EditorGUIUtility.PingObject(graph);
				}

				//up-level and tree
				using (Cell.RowPx(20))
				{
					if (Draw.Button(null, icon:UI.current.textures.GetTexture("DPUI/Icons/FolderTree"), iconScale:0.5f, visible:false))
						GraphTreePopup.DrawGraphTree(rootGraph!=null ? rootGraph : graph);
				}

				using (Cell.RowPx(20))
				{
					if (parentGraphs != null  &&  parentGraphs.Count != 0  && 
						Draw.Button(null, icon:UI.current.textures.GetTexture("DPUI/Icons/FolderUp"), iconScale:0.5f, visible:false))
					{
						graph = parentGraphs[parentGraphs.Count-1];
						parentGraphs.RemoveAt(parentGraphs.Count-1);
						ScrollZoomOnOpen();
						Repaint();
					}
				}

				Cell.EmptyRowRel(1); //switching to right corner

				//seed
				Cell.EmptyRowPx(5);
				using (Cell.RowPx(1)) Draw.ToolbarSeparator();

				using (Cell.RowPx(90))
				//	using (Cell.LinePx(toolbarSize-1))  //-1 just to place it nicely
				{
					#if UNITY_2019_1_OR_NEWER
					int newSeed;
					using (Cell.RowRel(0.4f)) Draw.Label("Seed:");
					using (Cell.RowRel(0.6f))
						using (Cell.Padded(1))
							newSeed = (int)Draw.Field(graph.random.Seed, style:UI.current.styles.toolbarField);
					#else
					Cell.current.fieldWidth = 0.6f;
					int newSeed = Draw.Field(graph.random.Seed, "Seed:");
					#endif
					if (newSeed != graph.random.Seed)
					{
						GraphWindow.RecordCompleteUndo();
						graph.random.Seed = newSeed;
						GraphWindow.RefreshMapMagic();
					}
				}

				Cell.EmptyRowPx(2);


				//gauge
				using (Cell.RowPx(1)) Draw.ToolbarSeparator();

				using (Cell.RowPx(200))
					using (Cell.LinePx(toolbarSize-1)) //-1 to leave underscore under gauge
				{
					if (mapMagic != null)
					{
						if (!mapMagic.IsGenerating())
						{
							Cell.EmptyRow();
							using (Cell.RowPx(40)) Draw.Label("Ready");
						}

						else
						{
							float progress = mapMagic.GetProgress();

							if (progress < 1 && progress != 0)
							{
								Texture2D backgroundTex = UI.current.textures.GetTexture("DPUI/ProgressBar/BackgroundBorderless");
								mapMagic.GetProgress();
								Draw.Texture(backgroundTex);

								Texture2D fillTex = UI.current.textures.GetBlankTexture(StylesCache.isPro ? Color.grey : Color.white);
								Color color = StylesCache.isPro ? new Color(0.24f, 0.37f, 0.58f) : new Color(0.44f, 0.574f, 0.773f);
								Draw.ProgressBarGauge(progress, fillTex, color);
							}

							//Repaint(); //doing it in OnInspectorUpdate
						}

						using (Cell.RowPx(20))
							if (Draw.Button(null, icon:UI.current.textures.GetTexture("DPUI/Icons/RefreshAll"), iconScale:0.5f, visible:false))
							{
								//graphUI.undo.Record(completeUndo:true); //won't record changed terrain data
								foreach (Terrain terrain in mapMagic.tiles.AllActiveTerrains())
									UnityEditor.Undo.RegisterFullObjectHierarchyUndo(terrain.terrainData, "RefreshAll");
								EditorUtility.SetDirty(mapMagic);

								GraphWindow.current.mapMagic.ClearAll();
								GraphWindow.current.mapMagic.StartGenerate();
							}

						using (Cell.RowPx(20))
							if (Draw.Button(null, icon:UI.current.textures.GetTexture("DPUI/Icons/Refresh"), iconScale:0.5f, visible:false))
							{
								GraphWindow.current.mapMagic.StartGenerate();
							}
					}

					else
						Draw.Label("Not Assigned to MapMagic Object");
				}

				using (Cell.RowPx(1)) Draw.ToolbarSeparator();

				//focus
				using (Cell.RowPx(20))
					if (Draw.Button(null, icon:UI.current.textures.GetTexture("DPUI/Icons/FocusSmall"), iconScale:0.5f, visible:false))
					{
						graphUI.scrollZoom.FocusWindowOn(GetNodesCenter(graph), position.size);
					}

				using (Cell.RowPx(20))
				{
					if (graphUI.scrollZoom.zoom < 0.999f)
					{
						if (Draw.Button(null, icon:UI.current.textures.GetTexture("DPUI/Icons/ZoomSmallPlus"), iconScale:0.5f, visible:false))
							graphUI.scrollZoom.Zoom(1f, position.size/2);
					}
					else
					{
						if (Draw.Button(null, icon:UI.current.textures.GetTexture("DPUI/Icons/ZoomSmallMinus"), iconScale:0.5f, visible:false))
							graphUI.scrollZoom.Zoom(0.5f, position.size/2); 
					}
				}
			}
		}


		private void OnHierarchyChange ()
		{
			//TODO: change selected graph here
		}


		private static Vector2 GetNodesCenter (Graph graph)
		{
			//Graph graph = CurrentGraph;
			if (graph.generators.Length==0) return new Vector2(0,0);

			Vector2 min = graph.generators[0].guiPosition;
			Vector2 max = min + graph.generators[0].guiSize;

			for (int g=1; g<graph.generators.Length; g++)
			{
				Vector2 pos = graph.generators[g].guiPosition;
				min = Vector2.Min(pos, min);
				max = Vector2.Max(pos + graph.generators[g].guiSize, max);
			}

			return (min + max)/2;
		}


		public void OnSceneGUI (SceneView sceneview)
		{
			if (graph==null || graph.generators==null) return; //if graph loaded corrupted

			bool hideDefaultToolGizmo = false; //if any of the nodes has it's gizmo enabled (to hide the default tool)

			for (int n=0; n<graph.generators.Length; n++)
				if (graph.generators[n] is ISceneGizmo)
				{
					ISceneGizmo gizmoNode = (ISceneGizmo)graph.generators[n];
					gizmoNode.DrawGizmo();
					if (gizmoNode.hideDefaultToolGizmo) hideDefaultToolGizmo = true;
				}
			
			if (hideDefaultToolGizmo) UnityEditor.Tools.hidden = true;
			else UnityEditor.Tools.hidden = false;
		}


		private void DragDrawAddRemove ()
		{
			int origButtonSize = 34; int origButtonOffset = 20;

			Vector2 buttonPos = new Vector2(
				UI.current.editorWindow.position.width - (origButtonSize + origButtonOffset)*UI.current.DpiScaleFactor,
				20*UI.current.DpiScaleFactor);
			Vector2 buttonSize = new Vector2(origButtonSize,origButtonSize) * UI.current.DpiScaleFactor;

			using (Cell.Custom(buttonPos,buttonSize))
			//later button pos could be overriden if dragging it
			{
				Cell.current.MakeStatic();


				//if dragging generator
				if (DragDrop.IsDragging()  &&  !DragDrop.IsStarted()  &&  DragDrop.obj is Cell  &&  UI.current.cellObjs.TryGetObject((Cell)DragDrop.obj, "Generator", out Generator draggedGen) )
				
				{
					if (Cell.current.Contains(UI.current.mousePos))
						Draw.Texture(UI.current.textures.GetTexture("MapMagic/Icons/NodeRemoveActive"));
					else
						Draw.Texture(UI.current.textures.GetTexture("MapMagic/Icons/NodeRemove"));
				}


				//if released generator on remove icon
				else if (DragDrop.IsReleased()  &&  
					DragDrop.releasedObj is Cell  &&  
					UI.current.cellObjs.TryGetObject((Cell)DragDrop.releasedObj, "Generator", out Generator releasedGen)  &&  
					Cell.current.Contains(UI.current.mousePos))
				{
					GraphEditorActions.RemoveGenerator(graph, releasedGen, selected);
					GraphWindow.RefreshMapMagic();
				}


				//if not dragging generator
				else
				{
					if (focusedWindow==this) drawAddRemoveButton = true;   //re-enabling when window is focused again after popup
					bool drawFrame = false;
					Color frameColor = new Color();

					//dragging button
					if (DragDrop.TryDrag(addDragId, UI.current.mousePos))
					{
						Cell.current.pixelOffset += DragDrop.totalDelta; //offsetting cell position with the mouse

						Draw.Texture(UI.current.textures.GetTexture("MapMagic/Icons/NodeAdd"));

						//if dragging near link, output or node
						Vector2 mousePos = graphUI.mousePos;
						//Vector2 mousePos = graphUI.scrollZoom.ToInternal(addDragTo + new Vector2(addDragSize/2,addDragSize/2)); //add button center

						object clickedObj = RightClick.ClickedOn(graphUI, mousePos);
				
						if (clickedObj != null  &&  !(clickedObj is Group))
						{
							drawFrame = true;
							frameColor = GeneratorDraw.GetLinkColor(Generator.GetGenericType(clickedObj.GetType()));
						}
					}

					//releasing button
					if (DragDrop.TryRelease(addDragId))
					{
						drawAddRemoveButton = false;

						Vector2 mousePos = graphUI.mousePos;
						//Vector2 mousePos = graphUI.scrollZoom.ToInternal(addDragTo + new Vector2(addDragSize/2,addDragSize/2)); //add button center

						RightClick.ClickedNear (graphUI, mousePos, 
							out Group clickedGroup, out Generator clickedGen, out IOutlet<object> clickedLayer, out IInlet<object> clickedLink, out IInlet<object> clickedInlet, out IOutlet<object> clickedOutlet, out FieldInfo clickedField);

						if (clickedOutlet != null)
							CreateRightClick.DrawAppendItems(mousePos, graph, clickedOutlet);

						else if (clickedLayer != null)
							CreateRightClick.DrawAppendItems(mousePos, graph, clickedLayer);

						else if (clickedLink != null)
							CreateRightClick.DrawInsertItems(mousePos, graph, clickedLink);

						else
							CreateRightClick.DrawCreateItems(mousePos, graph);
					}

					//starting button drag
					DragDrop.TryStart(addDragId, UI.current.mousePos, Cell.current.InternalRect);

					//drawing button
					#if !MM_DOC
					if (drawAddRemoveButton) //don't show this button if right-click items are shown
						Draw.Texture(UI.current.textures.GetTexture("MapMagic/Icons/NodeAdd")); //using Texture since Icon is scaled with scrollzoom
					#endif

					if (drawFrame)
					{
						Texture2D frameTex = UI.current.textures.GetColorizedTexture("MapMagic/Icons/NodeAddRemoveFrame", frameColor);
						Draw.Texture(frameTex);
					}
				}
			}
		}

		#region Showing Window

			public static GraphWindow ShowInNewTab (Graph graph)
			{
				GraphWindow window = CreateInstance<GraphWindow>();

				window.OpenRoot(graph);

				ShowWindow(window, inTab:true);
				return window;
			}

			public static GraphWindow Show (Graph graph)
			{
				GraphWindow window = null;
				GraphWindow[] allWindows = Resources.FindObjectsOfTypeAll<GraphWindow>();

				//if opened as biome via focused graph window - opening as biome
				if (focusedWindow is GraphWindow focWin  &&  focWin.graph.ContainsSubGraph(graph))
				{
					focWin.OpenBiome(graph);
					return focWin;
				}

				//if opened only one window - using it (and trying to load mm biomes)
				if (window == null)
				{
					if (allWindows.Length == 1)  
					{
						window = allWindows[0];
						if (!window.TryOpenMapMagicBiome(graph))
							window.OpenRoot(graph);
					}
				}

				//if window with this graph currently opened - just focusing it
				if (window == null)
				{
					for (int w=0; w<allWindows.Length; w++)
						if (allWindows[w].graph == graph)
							window = allWindows[w];
				}

				//if the window with parent graph currently opened
				if (window == null)
				{
					for (int w=0; w<allWindows.Length; w++)
						if (allWindows[w].graph.ContainsSubGraph(graph))
						{
							window = allWindows[w];
							window.OpenBiome(graph);
						}
				}

				//if no window found after all - creating new tab (and trying to load mm biomes)
				if (window == null)
				{
					window = CreateInstance<GraphWindow>();
					if (!window.TryOpenMapMagicBiome(graph))
						window.OpenRoot(graph);
				}
					
				ShowWindow(window, inTab:false);
				return window;
			}


			public void OpenBiome (Graph graph)
			/// In this case we know for sure what window should be opened. No internal checks
			{
				if (parentGraphs == null) parentGraphs = new List<Graph>();
				parentGraphs.Add(this.graph);
				this.graph = graph;
				ScrollZoomOnOpen();
			}


			public void OpenBiome (Graph graph, Graph root)
			/// Opens graph as sub-sub-sub biome to root
			{
				parentGraphs = GetStepsToSubGraph(root, graph);
				this.graph = graph;
				ScrollZoomOnOpen();
			}


			private bool TryOpenMapMagicBiome (Graph graph)
			/// Finds MapMagic object in scene and opens graph as mm biome with mm graph as a root
			/// Return false if it's wrong mm (or no mm at all)
			{
				MapMagicObject mapMagic = GetRelatedMapMagic(graph);
				if (mapMagic == null) return false;

				parentGraphs = GetStepsToSubGraph(mapMagic.graph, graph);
				this.graph = graph;

				ScrollZoomOnOpen();

				return true;
			}


			private void OpenRoot (Graph graph)
			{
				this.graph = graph;
				parentGraphs = null;

				ScrollZoomOnOpen();
			}


			private static void ShowWindow (GraphWindow window, bool inTab=false)
			/// Opens the graph window. But it should be created and graph assigned first.
			{
				Texture2D icon = TexturesCache.LoadTextureAtPath("MapMagic/Icons/Window"); 
				window.titleContent = new GUIContent("MapMagic Graph", icon);

				if (inTab) window.ShowTab();
				else window.Show();
				window.Focus();
				window.Repaint();

				window.ScrollZoomOnOpen(); //focusing after window has shown (since it needs window size)
			}


			private static GraphWindow FindReusableWindow (Graph graph)
			/// Finds the most appropriate window among all of all currently opened
			{
				GraphWindow[] allWindows = Resources.FindObjectsOfTypeAll<GraphWindow>();

				//if opened only one window - using it
				if (allWindows.Length == 1)  
					return allWindows[0];

				//if opening from currently active window
				if (focusedWindow is GraphWindow focWin)
					if (focWin.graph.ContainsSubGraph(graph))
						return focWin;
						
				//if window with this graph currently opened
				for (int w=0; w<allWindows.Length; w++)
					if (allWindows[w].graph == graph)
						return allWindows[w];

				//if the window with parent graph currently opened
				for (int w=0; w<allWindows.Length; w++)
					if (allWindows[w].graph.ContainsSubGraph(graph))
						return allWindows[w];

				return null;
			}


			private void ScrollZoomOnOpen ()
			///Finds a graph scroll and zoom from graphsScrollZooms and focuses on them. To switch between graphs
			///should be called each time new graph assigned
			{
				if (graph == null) return; 

				if (graphsScrollZooms.TryGetValue(graph, out Vector3 scrollZoom))
				{
					graphUI.scrollZoom.FocusWindowOn(new Vector2(scrollZoom.x, scrollZoom.y), position.size);
					graphUI.scrollZoom.zoom = scrollZoom.z;
				}

				else
					graphUI.scrollZoom.FocusWindowOn(GetNodesCenter(graph), position.size);
			}


			public static List<Graph> GetStepsToSubGraph (Graph rootGraph, Graph subGraph)
			/// returns List(this > biome > innerBiome)
			/// doesn't include the subgraph itself
			/// doesn't perform check if subGraph is contained within graph at all
			{
				List<Graph> steps = new List<Graph>();
				ContainsSubGraphSteps(rootGraph, subGraph, steps);
				steps.Reverse();
				return steps;
			}


			private static bool ContainsSubGraphSteps (Graph thisGraph, Graph subGraph, List<Graph> steps)
			/// Same as ContainsSubGraph, but using track list for GetStepsToSubGraph
			{
				if (thisGraph == subGraph)
					return true;

				foreach (Graph biomeSubGraph in thisGraph.SubGraphs())
					if (ContainsSubGraphSteps(biomeSubGraph, subGraph, steps))
					{
						steps.Add(thisGraph);
						return true;
					}
				
				return false;
			}


			[MenuItem ("Window/MapMagic/Editor")]
			public static void ShowEditor ()
			{
				MapMagicObject mm = FindObjectOfType<MapMagicObject>();
				Graph gens = mm!=null? mm.graph : null;
				GraphWindow.Show(mm?.graph);
			}

			[UnityEditor.Callbacks.OnOpenAsset(0)]
			public static bool ShowEditor (int instanceID, int line)
			{
				UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
				if (obj is Nodes.Graph graph) 
				{ 
					if (UI.current != null) UI.current.DrawAfter( new Action( ()=>GraphWindow.Show(graph) ) ); //if opened via graph while drawing it - opening after draw
					else Show(graph); 
					return true; 
				}
				if (obj is MapMagicObject) { GraphWindow.Show(((MapMagicObject)obj).graph); return true; }
				return false;
			}

		#endregion
	}

}//namespace