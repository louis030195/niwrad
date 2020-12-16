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

namespace MapMagic.GUI
{
	//[EditoWindowTitle(title = "MapMagic Graph")]  //it's internal Unity stuff
	public class AboutWindow : EditorWindow
	{
		UI ui = new UI();

		public void OnGUI ()
		{
			ui.Draw(DrawGUI);
		}


		public void DrawGUI ()
		{
			using (Cell.RowPx(150))
				Draw.Icon(UI.current.textures.GetTexture("MapMagic/About"));

			using (Cell.Row)
			{
				using (Cell.LineStd) Draw.Label("MapMagic 2 Beta");
				using (Cell.LineStd) Draw.Label("by Denis Pahunov");

				Cell.EmptyLinePx(10);

				using (Cell.LineStd) Draw.Label($"Version: {MapMagicObject.version.ToString()}");

				Cell.EmptyLinePx(10);

				//using (Cell.LineStd) Draw.URL(" - Online Documentation", "https://gitlab.com/denispahunov/mapmagic/wikis/home");
				using (Cell.LineStd) Draw.URL(" - Video Tutorials", url:"https://www.youtube.com/playlist?list=PL8fjbXLqBxvbsJ56kskwA2tWziQx3G05m");
				using (Cell.LineStd) Draw.URL(" - Forum Thread", url:"http://forum.unity3d.com/threads/map-magic-a-node-based-procedural-and-infinite-map-generator-for-asset-store.344440/");
				using (Cell.LineStd) Draw.URL(" - Issues / Ideas", url:"http://mm2.idea.informer.com/");
			}
		}


		[MenuItem ("Window/MapMagic/About")]
		public static void ShowWindow ()
		{
			AboutWindow window = (AboutWindow)GetWindow(typeof (AboutWindow));

			Texture2D icon = TexturesCache.LoadTextureAtPath("MapMagic/Icons/Window");
			window.titleContent = new GUIContent("About MapMagic", icon);

			window.position = new Rect(100,100,300,200);
		}
	}
}