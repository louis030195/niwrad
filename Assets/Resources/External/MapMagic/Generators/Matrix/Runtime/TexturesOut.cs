using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Den.Tools;
using Den.Tools.GUI;
using Den.Tools.Matrices;
using MapMagic.Products;
using MapMagic.Terrains;

using UnityEngine.Profiling;

namespace MapMagic.Nodes.MatrixGenerators {
	[System.Serializable]
	public class BaseTextureLayer : INormalizableLayer, IInlet<MatrixWorld>, IOutlet<MatrixWorld>, IOutput, IExposedGuid
	{
		public string name = "Layer";
		public int channelNum = 0; //for RTP, CTS and custom

		public float Opacity { get; set; }

		public Generator Gen { get { return gen; } private set { gen = value;} }
		public Generator gen; //property is not serialized
		public void SetGen (Generator gen) => this.gen=gen;

		public Guid guid;  //for value expose. Auto-property won't serialize
		public Guid Guid => guid;

		public BaseTextureLayer () { Opacity=1; guid=Guid.NewGuid(); }
	}


	[System.Serializable]
	public abstract class BaseTexturesOutput<L> : Generator, IMultiInlet, IMultiOutlet, IOutputGenerator, ILayered<L> where L: BaseTextureLayer, new()
	{
		public OutputLevel outputLevel = OutputLevel.Draft | OutputLevel.Main;
		public OutputLevel OutputLevel { get{ return outputLevel; } }


		public L[] layers = new L[0];
		public L[] Layers => layers; 
		public virtual void SetLayers(object[] ls) => layers = Array.ConvertAll(ls, i=>(L)i);

		public IEnumerable<IInlet<object>> Inlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i];
		}

		public IEnumerable<IOutlet<object>> Outlets() 
		{ 
			for (int i=0; i<layers.Length; i++)
				yield return layers[i];
		}

		public override void Generate (TileData data, StopToken stop) 
		{
			if (layers.Length == 0) return;

			//reading + normalizing + writing
			if (stop!=null && stop.stop) return;
			NormalizeLayers(layers, data, stop);

			//adding to finalize
			for (int i=0; i<layers.Length; i++)
			{
				if (stop!=null && stop.stop) return;
				data.finalize.Add(FinalizeAction, layers[i], data.products[layers[i]], data.currentBiomeMask);
			}

			#if MM_DEBUG
			Debug.Log("TexturesOut Generated");
			#endif
		}

		public abstract FinalizeAction FinalizeAction { get; } 

		public abstract void Purge (TileData data, Terrain terrain);
	}


	[System.Serializable]
	[GeneratorMenu(
		menu = "Map/Output", 
		name = "Textures", 
		section =2,
		drawButtons = false,
		colorType = typeof(MatrixWorld), 
		iconName="GeneratorIcons/TexturesOut",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Textures")]
	public class TexturesOutput200 : BaseTexturesOutput<TexturesOutput200.TextureLayer>
	{
		public class TextureLayer : BaseTextureLayer
		{
			[Val("Layer", cat:"Layer")] public TerrainLayer prototype; // = new TerrainLayer() {  tileSize=new Vector2(20,20) };

			public Color color = new Color(0.75f, 0.75f, 0.75f, 1);

			public bool guiProperties;
			public bool guiRemapping;
			public bool guiTileSettings;
		}

		[SerializeField] public int guiExpanded;

		public override FinalizeAction FinalizeAction => finalizeAction; //should return variable, not create new
		public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
		public static void Finalize (TileData data, StopToken stop) 
		{
			//calculating number of layers
			int layersCount = data.finalize.GetTypeCount(finalizeAction, data.subDatas);

			//preparing arrays
			if (stop!=null && stop.stop) return;
			TerrainLayer[] prototypes = new TerrainLayer[layersCount];
			Matrix[] matrices = new Matrix[layersCount];
			Matrix[] masks = new Matrix[layersCount];

			int i=0;
			foreach ((TextureLayer output, MatrixWorld product, MatrixWorld biomeMask) 
				in data.finalize.ProductSets<TextureLayer,MatrixWorld,MatrixWorld>(finalizeAction, data.subDatas))
			{
				prototypes[i] = output.prototype;
				matrices[i] = product;
				masks[i] = biomeMask;
				i++;
			}

			//creating splats and prototypes arrays
			if (stop!=null && stop.stop) return;
			float[,,] splats3D = BlendLayers(matrices, masks, data.area, stop);

			//pushing to apply
			if (stop!=null && stop.stop) return;
			ApplyData applyData = new ApplyData() { splats=splats3D, prototypes=prototypes };
			Graph.OnBeforeOutputFinalize?.Invoke(typeof(TexturesOutput200), data, applyData, stop);
			data.apply.Add(applyData);

			#if MM_DEBUG
			Debug.Log("TexturesOut Finalized");
			#endif
		}

		public static float[,,] BlendLayers (Matrix[] matrices, Matrix[] masks, Area area, StopToken stop=null)
		{
			int fullSize = area.full.rect.size.x;
			int activeSize = area.active.rect.size.x;
			int margins = area.Margins;

			float[,,] splats3D = new float[activeSize, activeSize, matrices.Length];

			for (int x=0; x<activeSize; x++)
			{
				if (stop!=null && stop.stop) return null;
				for (int z=0; z<activeSize; z++)
				{
					//int pos = (z+margins-area.full.rect.offset.z)*area.full.rect.size.x + x+margins - area.full.rect.offset.x;
					int pos = area.full.rect.GetPos(x+area.full.rect.offset.x+margins, z+area.full.rect.offset.z+margins);

					float sum = 0;
					for (int i=0; i<matrices.Length; i++) 
					{
						float val = matrices[i].arr[pos] * (masks[i]==null ? 1 : masks[i].arr[pos]);
						sum += val;
					}

					if (sum != 0)
						for (int i=0; i<matrices.Length; i++) 
					{
						float val = matrices[i].arr[pos] * (masks[i]==null ? 1 : masks[i].arr[pos]);
						val /= sum;

						if (val < 0) val = 0; if (val > 1) val = 1;

						splats3D[z,x,i] = val;
					}
				}
			}

			return splats3D;
		}

		public class ApplyData : IApplyData
		{
			public float[,,] splats;
			public TerrainLayer[] prototypes;

			public void Apply (Terrain terrain)
			{
				Profiler.BeginSample("Apply Textures " + terrain.transform.name);

				if (terrain==null || terrain.Equals(null) || terrain.terrainData==null) return; //chunk removed during apply
				TerrainData data = terrain.terrainData;

				//setting resolution
				int size = splats.GetLength(0);
				if (data.alphamapResolution != size) data.alphamapResolution = size;

				terrain.terrainData.terrainLayers = prototypes; //in 2017 seems that alphamaps should go first
				terrain.terrainData.SetAlphamaps(0,0,splats);

				Profiler.EndSample();

				#if MM_DEBUG
				Debug.Log("TexturesOut Applied");
				#endif
			}

			public static ApplyData Empty
			{get{
				return new ApplyData() { 
					splats = new float[64,64,0],
					prototypes = new TerrainLayer[0] };
			}}

			public int Resolution  
			{get{ 
				if (splats==null) return 0;
				else return splats.GetLength(0); 
			}}
		}

		public override void Purge (TileData data, Terrain terrain)
		{
			TerrainData terrainData = terrain.terrainData;
			terrainData.terrainLayers = new TerrainLayer[0];
			terrainData.alphamapResolution = 32;
		}
	}


	[System.Serializable]
	[GeneratorMenu(
		menu = "Map/Output", 
		name = "Custom", 
		section =2,
		drawButtons = false,
		colorType = typeof(MatrixWorld), 
		iconName="GeneratorIcons/TexturesOut",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Textures")]
	public class CustomShaderOutput200 : BaseTexturesOutput<CustomShaderOutput200.CustomShaderLayer>
	{
		public class CustomShaderLayer : BaseTextureLayer { } //inheriting empty class just to draw it's editor

		public static string[] controlTextureNames = new string[] { "_ControlTexture1", "_ControlTexture2", 
			"_ControlTexture3", "_ControlTexture4", "_ControlTexture5", "_ControlTexture6", "_ControlTexture7",
			"_ControlTexture8", "_ControlTexture9", "_ControlTexture10", "_ControlTexture11", "_ControlTexture12"};

		public override FinalizeAction FinalizeAction => finalizeAction; //should return variable, not create new
		public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
		public static void Finalize (TileData data, StopToken stop) 
		{
			//purging if no outputs
			if (data.finalize.GetTypeCount(finalizeAction) == 0)
			{
				if (stop!=null && stop.stop) return;
				data.apply.Add(ApplyData.Empty);
				return;
			}

			//creating control textures contents
			(CustomShaderLayer[] layers, MatrixWorld[] matrices, MatrixWorld[] masks) = 
				data.finalize.ProductArrays<CustomShaderLayer,MatrixWorld,MatrixWorld>(finalizeAction, data.subDatas);
			CoordRect colorsRect = data.area.active.rect;
			
			Color[][] colors = BlendMatrices(colorsRect, matrices, masks, layers.Select(l=>l.Opacity), layers.Select(l=>l.channelNum));

			//pushing to apply
			if (stop!=null && stop.stop) return;
			var controlTexturesData = new ApplyData() {
				textureColors = colors,
				textureFormat = TextureFormat.RGBA32,
				textureBaseMapDistance = 10000000, //no base map
				textureNames = (string[])controlTextureNames.Clone() };

			Graph.OnBeforeOutputFinalize?.Invoke(typeof(CustomShaderOutput200), data, controlTexturesData, stop);
			data.apply.Add(controlTexturesData);
		}


		public static Color[][] BlendMatrices (CoordRect colorsRect, Matrix[] matrices, Matrix[] biomeMasks, float[] opacities, int[] channelNums)
		/// Reads matrices and fills normalized values to colors using masks
		/// TODO: use raw texture bytes
		/// TODO: bring to matrix
		{
			int maxChannelNum = 0;
			foreach (int chNum in channelNums)
				if (chNum > maxChannelNum) maxChannelNum=chNum;

			Color[][] colors = new Color[maxChannelNum/4 + 1][];

			//getting matrices rect
			CoordRect matrixRect = new CoordRect(0,0,0,0);
			for (int m=0; m<matrices.Length; m++)
				if (matrices[m] != null) matrixRect = matrices[m].rect;

			//checking rect
			for (int m=0; m<matrices.Length; m++)
				if (matrices[m] != null  &&  matrices[m].rect != matrixRect)
					throw new Exception("MapMagic: Matrix rect mismatch");
			for (int b=0; b<biomeMasks.Length; b++)
				if (biomeMasks[b] != null  &&  biomeMasks[b].rect != matrixRect)
					throw new Exception("MapMagic: Biome matrix rect mismatch");

			//preparing row re-use array
			float[] values = new float[maxChannelNum+1];

			//blending
			for (int x=0; x<colorsRect.size.x; x++)
				for (int z=0; z<colorsRect.size.z; z++)
				{
					int matrixPosX = colorsRect.offset.x + x;
					int matrixPosZ = colorsRect.offset.z + z;
					int matrixPos = (matrixPosZ-matrixRect.offset.z)*matrixRect.size.x + matrixPosX - matrixRect.offset.x;

					int colorsPos = z*colorsRect.size.x + x; //(z-colorsRect.offset.z)*colorsRect.size.x + x - colorsRect.offset.x;

					float sum = 0;

					//resetting values
					for (int m=0; m<values.Length; m++)
						values[m] = 0;

					//getting values
					for (int m=0; m<matrices.Length; m++)
					{
						Matrix matrix = matrices[m];
						if (matrix == null) 
						{
							values[m] = 0;
							continue;
						}

						float val = matrix.arr[matrixPos];

						//multiply with biome
						Matrix biomeMask = biomeMasks[m];
						if (biomeMask != null) //no empty biomes in list (so no mask == root biome)
							val *= biomeMask.arr[matrixPos]; //if mask is not assigned biome was ignored, so only main outs with mask==null left here
						
						//clamp
						if (val < 0) val = 0; if (val > 1) val = 1;

						sum += val;
						values[channelNums[m]] += val;
					}

					//normalizing and writing to colors
					for (int m=0; m<values.Length; m++)
					{
						float val = sum!=0 ? values[m]/sum : 0;
						
						int texNum = m / 4;
						int chNum = m % 4;

						if (colors[texNum] == null) colors[texNum] = new Color[colorsRect.size.x*colorsRect.size.z];

						switch (chNum)
						{
							case 0: colors[texNum][colorsPos].r += val; break;
							case 1: colors[texNum][colorsPos].g += val; break;
							case 2: colors[texNum][colorsPos].b += val; break;
							case 3: colors[texNum][colorsPos].a += val; break;
						}
					}
				}
			
			return colors;
		}

		public class ApplyData : IApplyData
		{
			public Color[][] textureColors; // TODO: use raw texture bytes
			public string[] textureNames;
			public TextureFormat textureFormat;
			public float textureBaseMapDistance; //most custom shaders change the base distance using their profile or setting it to extremely high like megasplat


			public virtual void Apply (Terrain terrain)
			{
				if (textureColors==null) return;
				int numTextures = textureColors.Length;
				if (numTextures==0) return;
				int resolution = (int)Mathf.Sqrt(textureColors[0].Length);

				//MaterialPropertyBlock matProps = new MaterialPropertyBlock();

				//assigning material props via MaterialPropertySerializer to make them serializable
				MaterialPropertySerializer matPropSerializer = terrain.GetComponent<MaterialPropertySerializer>();
				if (matPropSerializer == null)
					matPropSerializer = terrain.gameObject.AddComponent<MaterialPropertySerializer>();


				for (int i=0; i<textureColors.Length; i++)
				{
					if (textureColors[i] == null) continue;

					string texName = null;
					if (i<textureNames.Length) texName = textureNames[i];

					Texture2D tex = matPropSerializer.GetTexture(textureNames[i]);
					if (tex==null || tex.width!=resolution || tex.height!=resolution || tex.format!=textureFormat)
					{
						if (tex!=null)
						{
							#if UNITY_EDITOR
							if (!UnityEditor.AssetDatabase.Contains(tex))
							#endif
								GameObject.DestroyImmediate(tex);
						}
							
						tex = new Texture2D(resolution, resolution, textureFormat, false, true);
						tex.wrapMode = TextureWrapMode.Mirror; //to avoid border seams
						//tex.hideFlags = HideFlags.DontSave;
						//tex.filterMode = FilterMode.Point;

						matPropSerializer.SetTexture(textureNames[i], tex);
					}

					tex.SetPixels(0,0,tex.width,tex.height,textureColors[i]);
					tex.Apply();

					//if (texName != null) matPropSerializer.SetTexture(texName, tex);
					if (texName != null) terrain.materialTemplate.SetTexture(texName, tex);
				}
 				
				matPropSerializer.Apply();

				terrain.basemapDistance = textureBaseMapDistance;	
			}

			public static ApplyData Empty
			{get{
				return new ApplyData() { 
					textureColors = new Color[0][],
					textureNames = new string[0]  };
			}}

			public int Resolution
			{get{
				if (textureColors.Length==0) return 0;
				else return (int)Mathf.Sqrt(textureColors[0].Length);
			}}
		}

		public override void Purge (TileData data, Terrain terrain)
		{

		}
	}

}
