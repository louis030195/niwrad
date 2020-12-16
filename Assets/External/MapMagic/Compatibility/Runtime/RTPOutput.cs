#if MAPMAGIC2 //shouldn't work if MM assembly not compiled

using UnityEngine;

using Den.Tools;
using Den.Tools.Matrices;
using MapMagic.Products;

namespace MapMagic.Nodes.MatrixGenerators 
{

	[System.Serializable]
	[GeneratorMenu( 
		menu = "Map/Output", 
		name = "RTP", 
		section =2,
		drawButtons = false,
		colorType = typeof(MatrixWorld), 
		iconName = "GeneratorIcons/TexturesOut",
		helpLink = "https://gitlab.com/denispahunov/mapmagic/wikis/output_generators/Textures")]
	public class RTPOutput200 : BaseTexturesOutput<RTPOutput200.RTPLayer>
	{
		#if RTP
		public ReliefTerrain rtp = null;
		#endif

		public class RTPLayer : BaseTextureLayer { }

		public override FinalizeAction FinalizeAction => finalizeAction; //should return variable, not create new
		public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
		public static void Finalize (TileData data, StopToken stop) 
		{
			//purging if no outputs
			if (data.finalize.GetTypeCount(Finalize) == 0)
			{
				if (stop!=null && stop.stop) return;
				data.apply.Add(CustomShaderOutput200.ApplyData.Empty);
				return;
			}

			//creating control textures contents
			Color[][] colors = null; //TODO: re-use colors array
//			CustomShaderOutput200.BlendControlTextures(ref colors, typeof(RTPOutput200), data);

			//pushing to apply
			if (stop!=null && stop.stop) return;
			var controlTexturesData = new CustomShaderOutput200.ApplyData() {
				textureColors = colors,
				textureFormat = TextureFormat.RGBA32,
				textureBaseMapDistance = 10000000, //no base map
				textureNames = new string[colors!=null ? colors.Length : 0] };

			for (int t=0; t<controlTexturesData.textureNames.Length; t++)
				controlTexturesData.textureNames[t] = "_Control" + (t+1);

			Graph.OnBeforeOutputFinalize?.Invoke(typeof(RTPOutput200), data, controlTexturesData, stop);
			data.apply.Add(controlTexturesData);
		}


		public override void Purge (TileData data, Terrain terrain)
		{

		}
	}

}
#endif