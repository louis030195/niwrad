using System;
using System.Reflection;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

using Den.Tools;
using MapMagic.Products;
using MapMagic.Core; //version number
using Den.Tools.Matrices; //get generic type

namespace MapMagic.Nodes
{
	[System.Serializable]
	[HelpURL("https://gitlab.com/denispahunov/mapmagic/wikis/home")]
	[CreateAssetMenu(menuName = "MapMagic/Empty Graph", fileName = "Graph.asset", order = 101)]
	public class Graph : ScriptableObject , ISerializationCallbackReceiver
	{
		public Generator[] generators = new Generator[0];
		public Dictionary<IInlet<object>, IOutlet<object>> links = new Dictionary<IInlet<object>, IOutlet<object>>();
		public Group[] groups = new Group[0];
		public Noise random = new Noise(12345, 32768);
		public Exposed exposed = new Exposed();

		public ulong changeVersion; //increases each time after change from gui. Used to copy function userGraph only when changed.

		public static Action<Generator, TileData> OnBeforeNodeCleared;
		public static Action<Generator, TileData> OnAfterNodeGenerated;
		public static Action<Type, TileData, IApplyData, StopToken> OnBeforeOutputFinalize; //TODO: rename onAfterFinalize? onBeforeApplyAssign?
		public static Action<Generator> OnInletsOutletsChanged; //TODO: in generator?
		//public static Action<Graph> On


		public static Graph Create (Graph src=null, bool inThread=false)
		{
			Graph graph;

			if (inThread)
				graph = (Graph)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Graph));  //not stable, better create in prepare
			else 
				graph = ScriptableObject.CreateInstance<Graph>();

			//copy source graph
			if (src==null) graph.generators = new Generator[0];
			else 
			{
				graph.generators = (Generator[])Serializer.DeepCopy(src.generators);
				graph.random = src.random;
			}

			return graph;
		}


		#region Node Operations

			public void Add (Generator node)
			{
				if (ArrayTools.Contains(generators,node))
						throw new Exception("Could not add generator " + node + " since it is already in graph");

				ArrayTools.Add(ref generators, node);
				//cachedGuidLut = null;
			}


			public void Add (Group grp)
			{
				if (ArrayTools.Contains(groups, grp))
						throw new Exception("Could not add group " + grp + " since it is already in graph");

				ArrayTools.Add(ref groups, grp);
				//cachedGuidLut = null;
			}


			public void Remove (Generator node)
			{
				if (!ArrayTools.Contains(generators,node))
					throw new Exception("Could not remove generator " + node + " since it is not in graph");

				UnlinkGenerator(node);

				ArrayTools.Remove(ref generators, node);
				//cachedGuidLut = null;
			}


			public void Remove (Group grp)
			{
				if (!ArrayTools.Contains(groups, grp))
					throw new Exception("Could not remove group " + grp + " since it is not in graph");

				ArrayTools.Remove(ref groups, grp);
				//cachedGuidLut = null;
			}

			
			public void Import (Graph other)
			{
				ArrayTools.AddRange(ref generators, other.generators);

				foreach (var kvp in other.links)
					links.Add(kvp.Key, kvp.Value);

				ArrayTools.AddRange(ref groups, other.groups);
			}


			public Graph Export (HashSet<Generator> gensHash)
			{
				Graph exported = ScriptableObject.CreateInstance<Graph>();
				exported.generators = gensHash.ToArray();

				foreach (var kvp in links)
				{
					if (gensHash.Contains(kvp.Key.Gen) && gensHash.Contains(kvp.Value.Gen))
						exported.links.Add(kvp.Key, kvp.Value);
				}

				Graph copied = ScriptableObject.CreateInstance<Graph>();
				DeepCopy(exported, copied);

				return copied;
			}

			public Generator[] Duplicate (HashSet<Generator> gens)
			/// Returns the list of duplicated generators
			{
				Graph exported = Export(gens);
				Generator[] expGens = exported.generators;
				Import(exported);

				//changing guids
				foreach (Generator gen in expGens)
					if (gen is IExposedGuid guidGen) gen.guid = Guid.NewGuid();

				return expGens;
			}

		#endregion

		#region Linking

			public void Link (IOutlet<object> outlet, IInlet<object> inlet)
			{
				//unlinking
				if (outlet == null  &&  links.ContainsKey(inlet))
					links.Remove(inlet);

				//linking
				else //if (CheckLinkValidity(outlet, inlet)) 
				{
					if (links.ContainsKey(inlet)) links[inlet] = outlet;
					else links.Add(inlet, outlet);
				}

				//cachedBackwardLinks = null;
			}


			public void Link (IInlet<object> inlet, IOutlet<object> outlet)
			/// The same in case this order is more convenient
				{ Link(outlet, inlet); }


			public bool CheckLinkValidity (IOutlet<object> outlet, IInlet<object> inlet)
			{
				if (Generator.GetGenericType(outlet) != Generator.GetGenericType(inlet))
					return false;

				if (AreDependent(inlet.Gen, outlet.Gen)) //in this order
					return false;

				return true;
			}

			public bool AreDependent (Generator prevGen, Generator nextGen)
			{
				if (prevGen == nextGen)
					return true;

				if (nextGen is IInlet<object> nextInlet  &&  
					links.TryGetValue(nextInlet, out IOutlet<object> nextInletLink)  &&
					AreDependent(prevGen, nextInletLink.Gen) ) 
						return true;

				if (nextGen is IMultiInlet nextMulIn)
				{
					foreach (IInlet<object> nextIn in nextMulIn.Inlets())
					{
						if (links.TryGetValue(nextIn, out IOutlet<object> nextInLink)  &&
							AreDependent(prevGen, nextInLink.Gen) )
								return true;
					}
				}

				if (nextGen is ICustomDependence cusDepGen)
				{
					foreach (Generator priorGen in cusDepGen.PriorGens())
					{
						if (AreDependent(prevGen, priorGen))
							return true;
					}
				}

				return false;
			}


			public bool IsLinked (IInlet<object> inlet) => links.ContainsKey(inlet);
			/// Is this inlet linked to anything


			public IOutlet<object> GetLink (IInlet<object> inlet) => links.TryGetValue(inlet, out IOutlet<object> outlet) ? outlet : null;
			/// Simply gets inlet's link


			public void UnlinkInlet (IInlet<object> inlet)
			{
				if (links.ContainsKey(inlet))
					links.Remove(inlet);
				
				//cachedBackwardLinks = null;
			}


			public void UnlinkOutlet (IOutlet<object> outlet)
			/// Removes any links to this outlet
			{
				List<IInlet<object>> linkedInlets = new List<IInlet<object>>();

				foreach (IInlet<object> inlet in linkedInlets)
					links.Remove(inlet);

				//cachedBackwardLinks = null;
			}


			public void UnlinkGenerator (Generator gen)
			/// Removes all links from and to this generator
			{
				List<IInlet<object>> genLinks = new List<IInlet<object>>(); //both that connected to this gen inlets and outlets

				foreach (var kvp in links)
				{
					IInlet<object> inlet = kvp.Key;
					IOutlet<object> outlet = kvp.Value;

					//unlinking this from others (not needed on remove, but Unlink could be called not only on remove)
					if (inlet.Gen == gen)
						genLinks.Add(inlet);

					//unlinking others from this
					if (outlet.Gen == gen)
						genLinks.Add(inlet);
				}

				foreach (IInlet<object> inlet in genLinks)
					links.Remove(inlet);

				//cachedBackwardLinks = null;
			}


			private List<IInlet<object>> LinkedInlets (IOutlet<object> outlet)
			/// Isn't fast, so using for internal purpose only. For other cases use cachedBackwardsLinks
			{
				List<IInlet<object>> linkedInlets = new List<IInlet<object>>();
				
				foreach (var kvp in links)
					if (kvp.Value == outlet)
						linkedInlets.Add(kvp.Key);

				return linkedInlets;
			}


			public void ThroughLink (Generator gen)
			/// Connects previous gen outlet with next gen inlet maintaining link before removing this gen
			/// This will not unlink generator completely - other inlets may remain
			{
				//choosing the proper inlets and outlets for re-link
				IInlet<object> inlet = null;
				IOutlet<object> outlet = null;

				if (gen is IInlet<object>  &&  gen is IOutlet<object>)
					{ inlet = (IInlet<object>)gen; outlet = (IOutlet<object>)gen; }

				if (gen is IMultiInlet multInGen  &&  gen is IOutlet<object> outletGen)
				{
					Type genericType = Generator.GetGenericType(gen);
					foreach (IInlet<object> genInlet in multInGen.Inlets())
					{
						if (!IsLinked(genInlet)) continue;
						if (Generator.GetGenericType(genInlet) == genericType) inlet = genInlet; //the first inlet of gen type
					}
				}

				if (gen is IInlet<object> inletGen  &&  gen is IMultiOutlet multOutGen)
				{
					Type genericType = Generator.GetGenericType(gen);
					foreach (IOutlet<object> genOutlet in multOutGen.Outlets())
					{
						if (Generator.GetGenericType(genOutlet) == genericType) outlet = genOutlet; //the first outlet of gen type
					}
				}

				if (inlet == null || outlet == null) return;
					
				// re-linking
				List<IInlet<object>> linkedInlets = LinkedInlets(outlet); //other generator's inlet connected to this gen
				if (linkedInlets.Count == 0)
					return;
				
				IOutlet<object> linkedOutlet;
				if (!links.TryGetValue(inlet, out linkedOutlet))
					return;
				
				foreach (IInlet<object> linkedInlet in linkedInlets)
					Link(linkedOutlet, linkedInlet);
			}


			public void AutoLink (Generator gen, IOutlet<object> outlet)
			/// Links with first gen's inlet of the same type as outlet
			{
				Type outletType = Generator.GetGenericType(outlet);

				if (gen is IInlet<object> inletGen)
				{
					if (Generator.GetGenericType(inletGen) == outletType)
						Link(outlet, inletGen);
				}

				else if (gen is IMultiInlet multInGen)
				{
					foreach (IInlet<object> inlet in multInGen.Inlets())
						if (Generator.GetGenericType(inlet) == outletType)
							{ Link(outlet,inlet); break; }
				}
			}


			private void CacheBackwardLinks ()
			{
				Dictionary<IOutlet<object>, HashSet<IInlet<object>>> bck = new Dictionary<IOutlet<object>, HashSet<IInlet<object>>>();

				foreach (var kvp in links)
				{
					IInlet<object> inlet = kvp.Key;
					IOutlet<object> outlet = kvp.Value;

					HashSet<IInlet<object>> linkedInlets;
					if (!bck.TryGetValue(outlet, out linkedInlets))
					{
						linkedInlets = new HashSet<IInlet<object>>();
						bck.Add(outlet, linkedInlets);
					}

					if (!linkedInlets.Contains(inlet))
						linkedInlets.Add(inlet);
						//TODO: check if we really need to check if it's added (maybe use list)
				}

				Dictionary<IOutlet<object>, IInlet<object>[]> newBck = new Dictionary<IOutlet<object>, IInlet<object>[]>();
				foreach (var kvp in bck)
					newBck.Add(kvp.Key, kvp.Value.ToArray());

				//cachedBackwardLinks = newBck;
			}

			public void ResetCachedLinks () { }// cachedBackwardLinks = null; } 

		#endregion


		#region Iterating Nodes

			// Not iteration nodes in subGraphs
			// Using recursive fn calls instead (with Graph in SubGraphs)

			public IEnumerable<Generator> GetGenerators (Predicate<Generator> predicate)
			/// Iterates in all generators that match predicate condition
			{
				int i = -1;
				for (int g=0; g<generators.Length; g++)
				{
					i = Array.FindIndex(generators, i+1, predicate);
					if (i>=0) yield return generators[i];
					else break;
				}
			}


			public Generator GetGenerator (Predicate<Generator> predicate)
			/// Finds first generator that matches condition
			/// Returns null if nothing found (no need to use TryGet)
			{
				int i = Array.FindIndex(generators, predicate);
				if (i>=0) return generators[i]; 
				else return null;
			}


			public IEnumerable<T> GeneratorsOfType<T> ()
			/// Iterates all generators of given type
			{
				for (int g=0; g<generators.Length; g++)
				{
					if (generators[g] is T tGen)
						yield return tGen;
				}
			}


			public int GeneratorsCount (Predicate<Generator> predicate)
			/// Finds the number of generators that match given condition
			{
				int count = 0;

				int i = -1;
				for (int g=0; g<generators.Length; g++)
				{
					i = Array.FindIndex(generators, i+1, predicate);
					if (i>=0) count++;
				}

				return count;
			}


			public IEnumerable<object> GetGeneratorsOrLayers (Predicate<object> predicate)
			/// Iterates in all generators that match predicate condition
			{
				foreach (Generator gen in generators)
				{
					if (predicate(gen))
						yield return gen;

					if (gen is ILayered<object> layered)
					{
						object[] layers = layered.Layers;
						foreach (object layer in layers)
						{
							if (predicate(layer))
								yield return layer;
						}
					}
				}
			}


			public IEnumerable<T> GeneratorsOrLayersOfType<T> ()
			/// Iterates all generators of given type
			{
				foreach (Generator gen in generators)
				{
					if (gen is T tGen)
						yield return tGen;

					if (gen is ILayered<object> layered)
					{
						object[] layers = layered.Layers;
						foreach (object layer in layers)
						{
							if (layer is T tLayer)
								yield return tLayer;
						}
					}
				}
			}


			public bool ContainsGenerator (Generator gen)
			{
				return GetGenerator(g => g==gen) != null;
			}


			public int GeneratorsCount<T> () where T: class
			{
				bool findByType (Generator g) => g is T;
				return GeneratorsCount(findByType);
			}


			public IEnumerable<Graph> SubGraphs (bool recursively=false)
			/// Enumerates in all child graphs recursively
			{
				foreach (IBiome biome in GeneratorsOfType<IBiome>())
				{
					Graph subGraph = biome.SubGraph;
					if (subGraph == null) continue;

					yield return biome.SubGraph;

					if (recursively)
						foreach (Graph subSubGraph in subGraph.SubGraphs(recursively:true))
							yield return subSubGraph;
				}

				foreach (IMultiBiome mbiome in GeneratorsOfType<IMultiBiome>())
					foreach (IBiome biome in mbiome.Biomes())
					{
						Graph subGraph = biome.SubGraph;
						if (subGraph == null) continue;

						yield return biome.SubGraph;

						if (recursively)
							foreach (Graph subSubGraph in subGraph.SubGraphs(recursively:true))
								yield return subSubGraph;
					}
			}


			public bool ContainsSubGraph (Graph subGraph, bool recursively=false)
			{
				for (int g=0; g<generators.Length; g++)
				{
					if (generators[g] is IBiome biome)
					{
						Graph biomeSubGraph = biome.AssignedGraph;
						if (biomeSubGraph == null) continue;
						if (biomeSubGraph == subGraph) return true;
						if (recursively && biomeSubGraph.ContainsSubGraph(subGraph, recursively:true)) return true;
					}
				
					if (generators[g] is IMultiBiome multiBiome)
						foreach (IBiome layer in multiBiome.Biomes())
					{
						Graph biomeSubGraph = layer.AssignedGraph;
						if (biomeSubGraph == null) continue;
						if (biomeSubGraph == subGraph) return true;
						if (recursively && biomeSubGraph.ContainsSubGraph(subGraph, recursively:true)) return true;
					}
				}

				return false;
			}



			public IEnumerable<Generator> RelevantGenerators (bool isDraft)
			/// All nodes that end chains, have previes, etc - all that should be generated
			{
				for (int g=0; g<generators.Length; g++)
					if (IsRelevant(generators[g], isDraft))
						yield return generators[g];
			}


			public bool IsRelevant (Generator gen, bool isDraft)
			{
				if (gen is IOutputGenerator outGen)
				{
					if (isDraft  &&  outGen.OutputLevel.HasFlag(OutputLevel.Draft)) return true;
					if (!isDraft  && outGen.OutputLevel.HasFlag(OutputLevel.Main)) return true;
				}

				else if (gen is IRelevant)
					return true;

				else if (gen is IBiome biomeGen && biomeGen.SubGraph!=null)
					return true;

				else if (gen is IMultiBiome multiBiomeGen) //TODO: make IMultiBiome and IFunctionOutput relevant
					return true;

				else if (gen.guiPreview)
					return true;

				else if (gen is IFunctionOutput<object>)
					return true;

				return false;
			}


			public IEnumerable<(Graph,TileData)> SubGraphsDatas (TileData data)
			{
				for (int g=0; g<generators.Length; g++)
				{
					if (generators[g] is IBiome biome)
					{
						Graph subGraph = biome.SubGraph;
						TileData subData = data.subDatas[biome];

						if (subGraph != null && subData != null)
							yield return (subGraph, subData);
					}

					if (generators[g] is IMultiBiome multiBiome)
						foreach (IBiome sbiome in multiBiome.Biomes())
						{
							Graph subGraph = sbiome.SubGraph;
							TileData subData = data.subDatas[sbiome];

							if (subGraph != null && subData != null)
								yield return (subGraph, subData);
						}
				}
			}

		#endregion


		/*#region Guid ops

			public Dictionary<Guid,Generator> GuidLut ()  //SOMEDAY: make cache
			{
				Dictionary<Guid,Generator> lut = new Dictionary<Guid, Generator>();
				for (int g=0; g<generators.Length; g++)
					lut.Add(generators[g].guid, generators[g]);
				return lut;
			}

			public bool ContainsGenerator (Guid guid)
			{
				bool findByGuid (Generator g) => g.guid==guid;
				bool tmp = GetGenerator(findByGuid) != null;
				return tmp;
			}

			
			public T GetGenerator<T> (Guid guid) where T: class
			{
				bool findByTypeGuid (Generator g) => g is T  &&  g.guid==guid;
				object tmp = GetGenerator(findByTypeGuid);
				return (T)tmp;
			}

		#endregion*/


		#region Generate

			//And all the stuff that takes data into account

			public void ClearChanged (TileData data)
			/// Removes ready state for all generators dependent from any non-ready gen
			{
				foreach (Generator relGen in RelevantGenerators(data.isDraft))
					ClearChangedRecursive(relGen, data);

				foreach ((Graph subGraph, TileData subData) in SubGraphsDatas(data))
					subGraph.ClearChanged(subData);
			}


			public void ClearChangedRecursive (Generator gen, TileData data)
			{
				//if (!data.ready[gen]) return; 
				//non-ready generators should be cleared too since they might have NonReady-ReadyOutdated-NonReady chanis

				bool resetReadyState = false; //should this gen reset mark be set to false?

				if (gen is ICustomClear customClearGen)
					customClearGen.OnBeforeClear(this, data);

				if (gen is IInlet<object> inletGen)
				{
					if (links.TryGetValue(inletGen, out IOutlet<object> outlet))
					{
						Generator outletGen = outlet.Gen;
						ClearChangedRecursive(outletGen, data);

						if (!data.ready[outletGen]) 
							resetReadyState = true;
					}
				}

				if (gen is IMultiInlet multInGen)
					foreach (IInlet<object> inlet in multInGen.Inlets())
						if (links.TryGetValue(inlet, out IOutlet<object> outlet))
						{
							Generator outletGen = outlet.Gen;
							ClearChangedRecursive(outletGen, data);

							if (!data.ready[outletGen])
								resetReadyState = true; 
								//break; //need to check-clear other layers chains 
						}

				if (gen is ICustomDependence customDepGen)
					foreach (Generator priorGen in customDepGen.PriorGens())
					{
						ClearChangedRecursive(priorGen, data);

						if (!data.ready[priorGen]) 
							{ resetReadyState = true; break; }
					}

				if (resetReadyState)
				{
					OnBeforeNodeCleared?.Invoke(gen, data);
					data.ready[gen] = false;
				}

				if (gen is ICustomClear customClearGenB)
					customClearGenB.OnAfterClear(this, data);
			}


			public void Prepare (TileData data, Terrain terrain)
			{
				foreach (Generator gen in generators)
				{
					if (!(gen is IPrepare prepGen)) continue;
					//if (!IsRelevant(gen, data.isDraft)) continue; //TODO: prepare recursive?
					if (data.ready[gen]) continue;

					prepGen.Prepare(data, terrain);
				}

				foreach ((Graph subGraph, TileData subData) in SubGraphsDatas(data))
					subGraph.Prepare(subData, terrain);
			}


			public void Generate (TileData data, StopToken stop=null)
			{
				data.products.linksLut = links; //assigning links to let data find product by inlet

				//main generate pass - all changed gens recursively
				foreach (Generator relGen in RelevantGenerators(data.isDraft))
				{
					if (stop!=null && stop.stop) return;
					GenerateRecursive(relGen, data, stop); //will not generate if it has not changed
				}

				//generating sub graphs
				foreach ((Graph subGraph, TileData subData) in SubGraphsDatas(data))
					subGraph.Generate(subData, stop);
			}


			public void GenerateRecursive (Generator gen, TileData data, StopToken stop=null)
			{
				if (stop!=null && stop.stop) return;
				if (data.ready[gen]) return;

				//generating inlets recursively
				if (gen is IInlet<object> inletGen)
				{
					if (links.TryGetValue(inletGen, out IOutlet<object> outlet))
						GenerateRecursive(outlet.Gen, data, stop);
				}

				if (gen is IMultiInlet multInGen)
				{
					foreach (IInlet<object> inlet in multInGen.Inlets())
						if (links.TryGetValue(inlet, out IOutlet<object> outlet))
							GenerateRecursive(outlet.Gen, data, stop);
				}

				if (gen is ICustomDependence customDepGen)
				{
					foreach (Generator priorGen in customDepGen.PriorGens())
						GenerateRecursive(priorGen, data, stop);
				}

				//checking for generating twice
				if (stop!=null && stop.stop) return;
				if (data.ready[gen])  
					throw new Exception("Generating twice " + gen + " stop:" + (stop!=null ? stop.stop.ToString() : "null"));

				//before-generated event
				//if (gen is ICustomGenerate customGen)
				//	customGen.OnBeforeGenerated(this, data, stop);

				//main generate fn
				long startTime = System.Diagnostics.Stopwatch.GetTimestamp();
				if (stop!=null && stop.stop) return;
				gen.Generate(data, stop);

				//checking time
				long deltaTime = System.Diagnostics.Stopwatch.GetTimestamp() - startTime;
				if (data.isDraft) gen.draftTime = 1000.0 * deltaTime / System.Diagnostics.Stopwatch.Frequency;
				else gen.mainTime = 1000.0 * deltaTime / System.Diagnostics.Stopwatch.Frequency;

				//marking ready
				if (stop!=null && stop.stop) return;
				data.ready[gen] = true;
				OnAfterNodeGenerated?.Invoke(gen, data);
			}


			public void Finalize (TileData data, StopToken stop=null)
			{
				if (stop!=null && stop.stop) return;
				
				//finalizing heights first (to floor objects)
				if (data.finalize.IsMarked(MatrixGenerators.HeightOutput200.finalizeAction))
				{
					MatrixGenerators.HeightOutput200.finalizeAction(data, stop);
					data.finalize.Mark(false, MatrixGenerators.HeightOutput200.finalizeAction, data.subDatas);

					//re-finalizing objects to changed heights
					//using height finalize event (to detach objects nodes)
					//data.finalize.Mark(true, ObjectsGenerators.ObjectsOutput.finalizeAction, data.subDatas);
					//data.finalize.Mark(true, ObjectsGenerators.TreesOutput.finalizeAction, data.subDatas);
				}

				//finalizing all other
				foreach (FinalizeAction action in data.finalize.MarkedActions(data.subDatas))
				{
					if (stop!=null && stop.stop) return;
					action(data, stop);
				}

				if (stop!=null && stop.stop) return;
				data.finalize.MarkAll(false, data.subDatas); //unmarking
			}


			[Obsolete] private IEnumerator Apply (TileData data, Terrain terrain, StopToken stop=null)
			/// Actually not a graph function. Here for the template. Not used.
			{
				if (stop!=null && stop.stop) yield break;
				while (data.apply.Count != 0)
				{
					IApplyData apply = data.apply.Dequeue(); //this will remove apply from the list
					
					//applying routine
					if (apply is IApplyDataRoutine)
					{
						IEnumerator e = ((IApplyDataRoutine)apply).ApplyRoutine(terrain);
						while (e.MoveNext()) 
						{
							if (stop!=null && stop.stop) yield break;
							yield return null;
						}
					}

					//applying at once
					else
					{
						apply.Apply(terrain);
						yield return null;
					}
				}

				
				#if UNITY_EDITOR
				if (data.isPreview)
					UnityEditor.EditorWindow.GetWindow<UnityEditor.EditorWindow>("MapMagic Graph");
				#endif

				//OnGenerateComplete.Raise(data);
			}


			public void Purge (Type type, TileData data, Terrain terrain)
			/// Purges the results of all output generators of type
			{
				for (int g=0; g<generators.Length; g++)
				{
					if (!(generators[g] is IOutputGenerator outGen)) continue;

					Type genType = outGen.GetType();
					if (genType==type  ||  type.IsAssignableFrom(genType))
						outGen.Purge(data, terrain);
				}

				foreach ((Graph subGraph, TileData subData) in SubGraphsDatas(data))
					subGraph.Purge(type, subData, terrain);
			}


			public bool AllOutputsReady (OutputLevel level, TileData data)
			/// Are all enabled output nodes with level of Level or Both are marked as ready in data
			{
				for (int g=0; g<generators.Length; g++)
				{
					Generator gen = generators[g];

					//if enabled level output is NOT ready
					if (gen.enabled && 
						gen is IOutputGenerator outGen &&
						outGen.OutputLevel.HasFlag(level)  &&
						!data.ready[gen] )
							return false;
				}

				foreach ((Graph subGraph, TileData subData) in SubGraphsDatas(data))
					if (!subGraph.AllOutputsReady(level, subData))
						return false;

				return true;
			}

		
		#endregion


		#region Complexity/Progress

			public float GetGenerateComplexity ()
			/// Gets the total complexity of the graph (including biomes) to evaluate the generate progress
			{
				float complexity = 0;

				for (int g=0; g<generators.Length; g++)
				{
					if (generators[g] is ICustomComplexity)
						complexity += ((ICustomComplexity)generators[g]).Complexity;

					else
						complexity ++;
				}

				return complexity;
			}


			public float GetGenerateProgress (TileData data)
			/// The summary complexity of the nodes Complete (ready) in data (shows only the graph nodes)
			/// No need to combine with GetComplexity since these methods are called separately
			{
				float complete = 0;

				//generate
				for (int g=0; g<generators.Length; g++)
				{
					if (generators[g] is ICustomComplexity)
						complete += ((ICustomComplexity)generators[g]).Progress(data);

					else
					{
						if (data.ready[generators[g]])
							complete ++;
					}
				}

				return complete;
			}


			public float GetApplyComplexity ()
			/// Gets the total complexity of the graph (including biomes) to evaluate the generate progress
			{
				HashSet<Type> allApplyTypes = GetAllOutputTypes();
				return allApplyTypes.Count;
			}


			private HashSet<Type> GetAllOutputTypes (HashSet<Type> outputTypes=null)
			/// Looks in subGraphs recursively
			{
				if (outputTypes == null)
					outputTypes = new HashSet<Type>();

				for (int g=0; g<generators.Length; g++)
					if (generators[g] is IOutputGenerator)
					{
						Type type = generators[g].GetType();
						if (!outputTypes.Contains(type))
							outputTypes.Add(type);
					}

				foreach (Graph subGraph in SubGraphs())
					subGraph.GetAllOutputTypes(outputTypes);

				return outputTypes;
			}


			public float GetApplyProgress (TileData data)
			{
				return data.apply.Count;
			}

		#endregion


		#region Serialization

			public int serializedVersion = 0; //displayed in graph inspector
			[SerializeField] private GraphSerializer200Beta serializer200beta = null;
			//[SerializeField] private GraphSerializer199 serializer199 = null;
			//public Serializer.Object[] serializedNodes = new Serializer.Object[0];


			public void OnBeforeSerialize ()
			{ 
				if (generators == null) 
					OnAfterDeserialize(); //trying to load graph once more

				if (generators == null) 
					throw new Exception("Could not save graph data, node array is null. Check if graph was loaded successfully"); 

				if (serializer200beta == null)
					serializer200beta = new GraphSerializer200Beta();
				serializer200beta.Serialize(this);
			}

			public void OnAfterDeserialize () 
			{
				try 
				{
					if (serializer200beta != null) serializer200beta.Deserialize(this);
					//if (serializer199 != null) serializer199.Deserialize(this);
					//if (serializedNodes!=null && serializedNodes.Length!=0) generators = (Generator[])Serializer.Deserialize(serializedNodes);
				}
				
				catch (Exception e) 
				{ 
					generators=null; 
					//Den.Tools.Tasks.CoroutineManager.Enqueue(()=>Debug.LogError("Could not load graph data: " + name + "\n" + e, this));
					throw new Exception("Could not load graph data:\n" + e); 
				} 
			}

			public static void DeepCopy (Graph src, Graph dst)
			{
				dst.name = src.name;
				if (dst.serializer200beta==null) dst.serializer200beta = new GraphSerializer200Beta();
				dst.serializer200beta.Serialize(src); //using dst's serializer (no need to create new one)
				dst.serializer200beta.Deserialize(dst);
			}


		#endregion
	}
}