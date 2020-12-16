using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Den.Tools.GUI.Popup
{
	public class SingleWindow : PopupWindowContent 
	{
		public bool sortItems = true;
		public int width = 170;
		public int height = 320;

		private UI ui = UI.ScrolledUI();

		public Item rootItem;
		public List<int> path = new List<int>() {0};  //like 1>>3>>2
		public int deepness = 0; //at which path level we are now (instead path.Count-1, since we've got to have something at the right when scrolling back)

		public static readonly Color highlightColor = new Color(0.6f, 0.7f, 0.9f);
		public static readonly Color backgroundColor =  new Color(0.9f, 0.9f, 0.9f);
		public static readonly Color highlightColorPro = new Color(0.219f, 0.387f, 0.629f);
		public static readonly Color backgroundColorPro =  new Color(0.33f, 0.33f, 0.33f);

		public static readonly float scrollSpeed = 5f;
		private static DateTime lastFrameTime = DateTime.Now;

		public Item GetItemAtDepth (int p)
		{
			Item item = rootItem;
			for (int i=1; i<=p; i++)
				item = item.subItems[ path[i] ];
			return item;
		}


		public override void OnGUI (Rect rect) 
		{
			ui.Draw(DrawGUI, UI.RectSelector.Full);
		}

		public void DrawGUI ()
		{
			ui.scrollZoom.allowScroll = false;
			ui.scrollZoom.allowZoom = false;

			Draw.Rect(StylesCache.isPro ? backgroundColorPro : backgroundColor);

			//smoothly scrolling
			float deltaTime = (float)(DateTime.Now-lastFrameTime).TotalSeconds;
			lastFrameTime = DateTime.Now;

			float targetScroll = -deepness * width;
			if (ui.scrollZoom.scroll.x < targetScroll)
			{
				ui.scrollZoom.scroll.x += scrollSpeed * width * deltaTime;
				if (ui.scrollZoom.scroll.x > targetScroll) ui.scrollZoom.scroll.x = targetScroll;
			}
			if (ui.scrollZoom.scroll.x > targetScroll)
			{
				ui.scrollZoom.scroll.x -= scrollSpeed * width * deltaTime;
				if (ui.scrollZoom.scroll.x < targetScroll) ui.scrollZoom.scroll.x = targetScroll;
			}


			for (int p=0; p<path.Count; p++)
			{
				Item currItem = GetItemAtDepth(p);
				using (Cell.RowPx(width))
					DrawMenu(currItem, p);
			}

			//refreshing selection frame
			this.editorWindow.Repaint();
		}

		public void DrawMenu(Item item, int itemDeepness)
		{
			using (Cell.LinePx(25))
			{
				Texture2D headerTex = UI.current.textures.GetTexture("DPUI/Backgrounds/Popup");
				Draw.ColorizedTexture(headerTex, item.color);

				if (itemDeepness != 0)
				{
					Texture2D shveronTex = UI.current.textures.GetTexture("DPUI/Chevrons/TickLeft"); 
					using (Cell.RowPx(20)) Draw.Icon(shveronTex);
				}

				Draw.Label(item.name, style:UI.current.styles.boldMiddleCenterLabel);

				bool clicked = Cell.current.Contains(ui.mousePos) && Event.current.rawType == EventType.MouseDown && Event.current.button == 0 && !UI.current.layout;
				if (clicked && itemDeepness != 0) 
					deepness = itemDeepness-1;
			}

			using (Cell.LinePx(0))
			{
				for (int n=0; n<item.subItems.Count; n++)
				{
					using (Cell.LinePx(0)) 
					{
						Item currItem = item.subItems[n];

						//drawing
						bool highlighted = Cell.current.Contains(ui.mousePos);
						
						if (!currItem.isSeparator)
							using (Cell.LinePx(22)) DefaultItemDraw(currItem, n, highlighted);
						else
							using (Cell.LinePx(Item.separatorHeight)) DrawSeparator();

						//clicking
						bool clicked = highlighted && 
							!currItem.disabled &&
							Event.current.type == EventType.MouseDown && 
							Event.current.button == 0 && 
							!UI.current.layout;

						if (clicked && currItem.subItems != null) 
						{
							if (path.Count-1 > itemDeepness)
								path.RemoveRange(itemDeepness+1, path.Count-itemDeepness-1);
							path.Add(n);
							deepness = itemDeepness + 1;
						}
						if (clicked && currItem.onClick != null)
							currItem.onClick();
					}
				}
			}
		}

		public void DefaultItemDraw (Item item, int num, bool selected)
		{
			if (selected && !item.disabled)
				Draw.Rect(StylesCache.isPro ? highlightColorPro : highlightColor);	

			Cell.current.disabled = item.disabled;

			//icon
			using (Cell.RowPx(35))
			{
				//Draw.Rect(item.color);
				if (item.icon!=null) 
					Draw.Icon(item.icon, scale:0.5f);
			}

			//label
			using (Cell.Row) Draw.Label(item.name);

			//chevron
			if (item.subItems != null)
			{
				Texture2D chevronTex = UI.current.textures.GetTexture("DPUI/Chevrons/TickRight");
				using (Cell.RowPx(20)) Draw.Icon(chevronTex);
			}
		}

		public void DrawSeparator ()
		{
			Cell.EmptyRowPx(20);
			using (Cell.Row)
			{
				Cell.EmptyLine();
				using (Cell.LinePx(1)) Draw.Rect(Color.gray);
				Cell.EmptyLine();
			}
			Cell.EmptyRowPx(20);
		}


		public override Vector2 GetWindowSize() 
		{
			return new Vector2(width, height);
		}

		public void Show (Vector2 pos)
		{
			rootItem.SortSubItems();
			PopupWindow.Show(new Rect(pos.x-width/2,pos.y-10,width,0), this);
		}
	}

}