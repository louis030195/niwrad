using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using Den.Tools;

namespace MapMagic.Nodes 
{
	[Serializable]
	public class Exposed
	{
		public struct Entry
		{
			public Guid guid;
			public Type type;
			public string fieldName;

			public string guiName;
		}

		public Entry[] entries;


		public Exposed () { } //for serializer
		public Exposed (Exposed src)
		{
			entries = (Entry[])src.entries.Clone();
		}


		public bool IsExposed (IExposedGuid obj, FieldInfo field)
		{
			if (entries == null) return false;

			Guid objGuid = obj.Guid;
			Type type = field.FieldType;
			string fieldName = field.Name;
			return entries.Find( e => e.guid==objGuid && e.fieldName==fieldName && e.type==type) >= 0;
		}


		public void Expose (IExposedGuid obj, FieldInfo field, string name=null)
		{
			if (entries == null) entries = new Entry[0];

			if (IsExposed(obj,field))
				Unexpose(obj,field);

			Entry entry = new Entry() {
				guid = obj.Guid,
				type = field.FieldType,
				fieldName = field.Name,
				guiName = name!=null ? name : field.Name.Nicify() };

			ArrayTools.Add(ref entries, entry);
		}


		public void Unexpose (IExposedGuid obj, FieldInfo field)
		{
			if (entries == null) 
				return; 

			for (int i=0; i<entries.Length; i++) //trying to remove all instances
			{
				if (!IsExposed(obj,field)) return;

				Guid objGuid = obj.Guid;
				Type type = field.FieldType;
				string fieldName = field.Name;
				int index = entries.Find( e => e.guid==objGuid && e.fieldName==fieldName && e.type==type );

				ArrayTools.RemoveAt(ref entries, index);
				if (entries.Length == 0) entries = null;
				if (entries == null) return; //if just removed last entry
			}
		}


		public void ReadOverride (Graph graph, Dictionary<Entry,object> ovd)
		///Reads the graph's exposed values defaults, storing them to ovd
		///If ovd already contains exposed value, it won't be overwritten
		{
			Dictionary<Guid,IExposedGuid> guidLut = graph.CompileGraphGuidLut();

			ClearObsoleteEntries(graph); //required to avoid errors on getting fields
			ClearObsoleteOverride(ovd); //just in case
			
			if (entries == null || entries.Length==0) return;
			for (int i=0; i<entries.Length; i++)
			{
				if (ovd.ContainsKey(entries[i])) continue; //already in list

				IExposedGuid gen = guidLut[entries[i].guid];

				Type genType = gen.GetType();
				FieldInfo field = genType.GetField(entries[i].fieldName);

				object val = field.GetValue(gen);
				ovd.Add(entries[i], val);
			}
		}


		public void ApplyOverride (Graph graph, Dictionary<Entry,object> ovd)
		///Writes overridden value (ovd) to used graph
		{
			Dictionary<Guid,IExposedGuid> guidLut = graph.CompileGraphGuidLut();

			ClearObsoleteEntries(graph); //required to avoid errors on getting fields
			
			if (entries==null || entries.Length==0 || ovd==null || ovd.Count==0) return;
			for (int i=0; i<entries.Length; i++)
			{
				if (!ovd.ContainsKey(entries[i])) continue; //exposed but not not overrided

				IExposedGuid gen = guidLut[entries[i].guid];

				Type genType = gen.GetType();
				FieldInfo field = genType.GetField(entries[i].fieldName);

				object val = ovd[entries[i]];
				field.SetValue(gen, val);
			}
		}


		public void ClearObsoleteEntries (Graph graph)
		/// Removes THIS entries containing generators or fields that are not in graph anymore
		{
			if (entries == null || entries.Length==0) return;
			for (int i=entries.Length-1; i>=0; i--)
			{
				//generator removed
				Guid guid = entries[i].guid;
				if (!graph.ContainsGenGuid(guid))
				{
					ArrayTools.RemoveAt(ref entries, i);
					continue;
				}

				//field removed (or target like adjust changed)
				IExposedGuid gen = graph.FindGenByGuid(guid);
				if (gen == null)
				{
					ArrayTools.RemoveAt(ref entries, i);
					continue;
				}
			}
		}


		public void ClearObsoleteOverride (Dictionary<Entry,object> ovd)
		/// Does not change THIS entries, but removes unused entries from the ovd dictionary (the ones that do not exist in THIS)
		{
			if (entries == null || entries.Length==0) { ovd.Clear(); return; }

			HashSet<Entry> entHash = new HashSet<Entry>();
			for (int i=0; i<entries.Length; i++)
				entHash.Add(entries[i]);

			List<Entry> removedEntries = new List<Entry>();

			foreach (var kvp in ovd)
			{
				Entry entry = kvp.Key;
				if (!entHash.Contains(entry))
					removedEntries.Add(entry);
			}

			foreach (Entry entry in removedEntries)
				ovd.Remove(entry);
		}
	}

	public static class GraphExtensions
	{
		public static IExposedGuid FindGenByGuid (this Graph graph, Guid guid)
		///Finds generator or layer in graph by guid number
		{
			bool MatchGuid (object o)
			{
				if (o is IExposedGuid eo)
					return eo.Guid == guid;

				return false;
			}

			foreach (object obj in graph.GetGeneratorsOrLayers(MatchGuid))
				return (IExposedGuid)obj;

foreach (object obj in graph.GetGeneratorsOrLayers(MatchGuid))
	return (IExposedGuid)obj;

			return null;
		}


		public static bool ContainsGenGuid (this Graph graph, Guid guid)
		{
			return FindGenByGuid(graph, guid) != null;
		}


		public static Dictionary<Guid,IExposedGuid> CompileGraphGuidLut (this Graph graph)
		{
			Dictionary<Guid,IExposedGuid> lut = new Dictionary<Guid, IExposedGuid>();

			foreach (IExposedGuid obj in graph.GeneratorsOrLayersOfType<IExposedGuid>())
				lut.Add(obj.Guid, obj);

			return lut;
		}
	}
}
