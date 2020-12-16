
//using a different assembly to make it compile independently from MapMagic or other assets
#if UNITY_EDITOR

using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
//using System.Reflection;
using UnityEngine.Profiling;
using UnityEditor.Compilation;

//using Plugins;
//using Plugins.GUI;

//using MapMagic.Core;
//using MapMagic.Nodes;
//using MapMagic.Products;
//using MapMagic.Previews;

namespace MapMagic.GUI
{
	

	//[EditoWindowTitle(title = "MapMagic Settings")]  //it's internal Unity stuff
	public class SettingsWindow : EditorWindow
	{
		//UI ui = new UI();

		#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		#endif
		static void InitializeSettings ()
		/// Initializes settings on first MM import
		{
			BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
			string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
			
			//removing beta marks
			if (symbols.Contains("_MAPMAGIC_BETA"))
				DisableKeyword("_MAPMAGIC_BETA", ref symbols);
			if (symbols.Contains("_MMNATIVE"))
				DisableKeyword("_MMNATIVE", ref symbols);

			if (!symbols.Contains("MAPMAGIC2"))
			{
				EnableKeyword("MAPMAGIC2", ref symbols); //for voxeland and other plugins compatibility

				#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
				EnableKeyword("MM_NATIVE", ref symbols);
				#endif

				PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);
			}

			//ShowNet20Notification();
		}

		public void OnGUI ()
		{
			//ui.Draw(DrawGUI);
			DrawGUI();
		}

		public void DrawGUI ()
		{
			if (EditorApplication.isCompiling)
				EditorGUILayout.HelpBox("Compiling scripts. Please wait until compilation is finished", MessageType.None);

			//using (Cell.LineStd)
			EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.Space();
				using (new EditorGUILayout.VerticalScope())
				{
					BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
					string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);

					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Scripting Define Symbols"); //Draw.Label(
					DrawKeyword("MM_NATIVE", "C++ Native Code", ref symbols);
					DrawKeyword("MM_DEBUG", "Debug Mode", ref symbols);
					DrawKeyword("MM_DOC", "Documentation Screenshots Mode", ref symbols);
					DrawKeyword("MM_EXP", "Experimental Features", ref symbols);

					//Cell.EmptyLinePx(5);
					EditorGUILayout.Space();

					EditorGUILayout.LabelField("Compatibility");
					DrawKeyword("CTS_PRESENT", "CTS 2019", ref symbols);
					DrawKeyword("__MEGASPLAT__", "MegaSplat", ref symbols);
					DrawKeyword("__MICROSPLAT__", "MicroSplat", ref symbols);
					DrawKeyword("RTP", "Relief Terrain Pack", ref symbols);
					DrawKeyword("VEGETATION_STUDIO_PRO", "Vegetation Studio Pro", ref symbols);

					//Cell.EmptyLinePx(4);
					EditorGUILayout.Space();

					#if UNITY_2019_2_OR_NEWER
					bool autoRef = GetMMAutoRef();
					bool newAutoRef = EditorGUILayout.ToggleLeft("Assemblies Auto Ref", autoRef);
					if (autoRef != newAutoRef)
						SetMMAutoRef(newAutoRef);
				
					if (!newAutoRef)
						EditorGUILayout.HelpBox("Enable MM assemblies Auto Reference for compatibility with these or custom scripts", MessageType.None);
					#endif

					EditorGUILayout.Space();
				}
				EditorGUILayout.Space();
			}
			EditorGUI.EndDisabledGroup();
		}

		static void DrawKeyword (string symbol, string label, ref string projectSymbols)
		{
			bool enabled = projectSymbols.Contains(symbol);

			//Draw.ToggleLeft(ref enabled, label);
			bool newEnabled = EditorGUILayout.ToggleLeft(label, enabled);

			if (newEnabled != enabled)
			{
				if (newEnabled) EnableKeyword(symbol, ref projectSymbols);
				else DisableKeyword(symbol, ref projectSymbols);
			}
		}


		static void EnableKeyword (string keyword, ref string symbols)
		{	
			if (!symbols.Contains(keyword+";") && !symbols.EndsWith(keyword)) 
			{
				symbols += (symbols.Length!=0? ";" : "") + keyword;

				Debug.Log(keyword + " Enabled");

				BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
				PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);
			}

			//EditorWindow.focusedWindow.Repaint();
			//AssetDatabase.Refresh();
		}


		static void DisableKeyword (string keyword, ref string symbols)
		{
			if (symbols.Contains(keyword+";") || symbols.EndsWith(keyword)) 
			{
				symbols = symbols.Replace(keyword,""); 
				symbols = symbols.Replace(";;", ";"); 

				Debug.Log(keyword + " Disabled");

				BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
				PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbols);
			}

			//EditorWindow.focusedWindow.Repaint();
			//AssetDatabase.Refresh();
		}



		static bool GetMMAutoRef ()
		{
			return GetAutoRef("MapMagic") &&
				GetAutoRef("MapMagic.Editor") &&
				GetAutoRef("Tools") &&
				GetAutoRef("Tools.Editor");
		}

		static void SetMMAutoRef (bool val)
		{
			SetAutoRef("MapMagic", val);
			SetAutoRef("MapMagic.Editor", val);
			SetAutoRef("Tools", val);
			SetAutoRef("Tools.Editor", val);
		}

		static bool GetAutoRef (string assName)
		{
			string asmdef = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assName);
			using (StreamReader reader = new StreamReader(asmdef))
			{
				string asmdefText = reader.ReadToEnd();
				if (asmdefText.Contains("\"autoReferenced\": false"))
					return false;
				else return true;
			}
		}

		static void SetAutoRef (string assName, bool val)
		{
			string asmdef = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assName);
			string asmdefText;
			using (StreamReader reader = new StreamReader(asmdef))
				asmdefText = reader.ReadToEnd();

			string newText = "\"autoReferenced\": " + (val? "true":"false");
			if (asmdefText.Contains("\"autoReferenced\": false"))
				asmdefText = asmdefText.Replace("\"autoReferenced\": false", newText);
			else if (asmdefText.Contains("\"autoReferenced\": true"))
				asmdefText = asmdefText.Replace("\"autoReferenced\": true", newText);
			else 
				asmdefText = asmdefText.Replace("}", newText + "\n}");

			using (StreamWriter writer = new StreamWriter(asmdef))
				writer.Write(asmdefText);

			AssetDatabase.Refresh();
		}


		[MenuItem ("Window/MapMagic/Settings")]
		public static void ShowWindow ()
		{
			SettingsWindow window = (SettingsWindow)GetWindow(typeof (SettingsWindow));

			Texture2D icon = Resources.Load("MapMagic/Icons/Window") as Texture2D; 
			window.titleContent = new GUIContent("MapMagic Settings", icon);

			window.position = new Rect(100,100,300,250);
		}

		public static void ShowNet20Notification ()
		{
			//#if !NET_STANDARD_2_0 won't work since editor is always NET_4
			if (PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup) != ApiCompatibilityLevel.NET_4_6  &&
				EditorUtility.DisplayDialog("MapMagic API Compatibility Warning", "MapMagic requires .NET 4.x API Compatibility level. \n"+
					"Do you want to switch compatibility level now? \n\n"+
					"You can switch compatibility level manually in Project Settings -> Player -> Api Compatibility Level",
					"Switch to .NET 4.x",
					"Cancel"))
						PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup, ApiCompatibilityLevel.NET_4_6);
			
		}
	}
}

#endif