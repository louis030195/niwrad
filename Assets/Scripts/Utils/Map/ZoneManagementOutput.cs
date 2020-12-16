using System;
using System.Collections.Generic;
using Den.Tools.Matrices;
using MapMagic.Nodes;
using MapMagic.Products;
using MapMagic.Terrains;
using UnityEngine;

namespace Utils.Map {
    [System.Serializable]
    [GeneratorMenu(
        menu = "Map/Output",
        name = "Zones",
        section = 2,
        drawButtons = false,
        colorType = typeof(MatrixWorld))]
    public class ZoneManagementOut : Generator, IMultiInlet, IOutputGenerator, IOutput, ILayered<ZoneManagementOut.ZoneLayer> {

        public OutputLevel outputLevel = OutputLevel.Draft | OutputLevel.Main;

        public OutputLevel OutputLevel {
            get {
                return outputLevel;
            }
        }

        public class ZoneLayer : IInlet<MatrixWorld>, IOutput {
            public Generator Gen {
                get {
                    return gen;
                }
                private set {
                    gen = value;
                }
            }
            public Generator gen; //property is not serialized
            public void SetGen ( Generator gen ) => this.gen = gen;

            public readonly Inlet<MatrixWorld> inlet = new Inlet<MatrixWorld>();
            public string zoneName;
        }

        public ZoneLayer[] layers = new ZoneLayer[] { new ZoneLayer(), new ZoneLayer() };
        public ZoneLayer[] Layers => layers;
        public void SetLayers ( object[] ls ) => layers = Array.ConvertAll(ls, i => (ZoneLayer) i);

        public IEnumerable<IInlet<object>> Inlets () {
            for ( int i = 0; i < layers.Length; i++ )
                yield return layers[i].inlet;
        }

        public override void Generate ( TileData data, StopToken stop ) {
            if ( stop != null && stop.stop )
                return;
            if ( !enabled ) {
                data.finalize.Remove(finalizeAction, this);
                return;
            }
            for ( int i = 0; i < layers.Length; i++ ) {
                if ( stop != null && stop.stop )
                    return;
                ZoneLayer layer = layers[i];
                string zoneName = layer.zoneName;
                if ( zoneName != null && layer.inlet != null ) {
                    MatrixWorld matrix = data.products.ReadInlet(layer.inlet);
                    if ( matrix != null ) {
                        data.finalize.Add(finalizeAction, layer, matrix, data.currentBiomeMask);
                    }
                }
            }
        }


        public static FinalizeAction finalizeAction = Finalize; //class identified for FinalizeData
        public static void Finalize ( TileData data, StopToken stop ) {
            //creating splats and prototypes arrays
            int layersCount = data.finalize.GetTypeCount(finalizeAction, data.subDatas);
            int splatsSize = data.area.active.rect.size.x;
            int splatsSizeSquared = splatsSize * splatsSize;


            //preparing texture colors
            IEnumerable<(ZoneLayer output, MatrixWorld matrix, MatrixWorld biomeMask)> productSet =
                data.finalize.ProductSets<ZoneLayer, MatrixWorld, MatrixWorld>(finalizeAction, data.subDatas);

            HashSet<string> addedZoneNames = new HashSet<string>();
            Dictionary<string, ZoneOutputInfo> outputInfoByZoneName = new Dictionary<string, ZoneOutputInfo>();
            foreach ( (ZoneLayer output, MatrixWorld matrix, MatrixWorld biomeMask)
                in productSet ) {
                if ( output == null || matrix == null ) {
                    continue;
                }
                string zoneName = output.zoneName;
                if ( zoneName != null ) {
                    zoneName = zoneName.Trim().ToUpper();
                    if ( !zoneName.Equals("") && !addedZoneNames.Contains(zoneName) ) {
                        outputInfoByZoneName.Add(zoneName, new ZoneOutputInfo() { output = output, matrix = matrix, biomeMask = biomeMask });
                        addedZoneNames.Add(zoneName);
                    }
                }
            }

            // if there are no zones, no need to continue.
            Dictionary<string, float[]> colorsForTextureMaskByZoneName = new Dictionary<string, float[]>();
            Dictionary<string, float> percentCompositionByZoneName = new Dictionary<string, float>();
            if ( addedZoneNames.Count > 0 ) {
                // determine the colors to set for the texture masks.
                foreach ( string zoneName in addedZoneNames ) {
                    ZoneOutputInfo outputInfo = null;
                    outputInfoByZoneName.TryGetValue(zoneName, out outputInfo);
                    if ( outputInfo != null ) {
                        float[] colors = new float[splatsSizeSquared];
                        ZoneLayer output = outputInfo.output;
                        float total = BlendLayer(colors, data.area, outputInfo.matrix, outputInfo.biomeMask, stop);
                        if ( total > 0 ) {
                            float percentComposition = total / splatsSizeSquared;
                            colorsForTextureMaskByZoneName.Add(zoneName, colors);
                            percentCompositionByZoneName.Add(zoneName, percentComposition);
                        }
                    }
                }
            }

            //pushing to apply
            if ( stop != null && stop.stop ) {
                return;
            }
            ApplyData applyData = new ApplyData() { colorsForTextureMaskByZoneName = colorsForTextureMaskByZoneName, percentCompositionByZoneName = percentCompositionByZoneName, resolution = splatsSize };
            Graph.OnBeforeOutputFinalize?.Invoke(typeof(ZoneManagementOut), data, applyData, stop);
            data.apply.Add(applyData);
        }

        /// <summary>
        /// A helper object for holding data for the ApplyData functions.
        /// </summary>
        public class ZoneOutputInfo {
            public ZoneLayer output;
            public MatrixWorld matrix;
            public MatrixWorld biomeMask;
        }

        public static float BlendLayer ( float[] cols, Area area, MatrixWorld matrix, MatrixWorld biomeMask, StopToken stop = null ) {
            int splatsSize = area.active.rect.size.x;
            int fullSize = area.full.rect.size.x;
            int margins = area.Margins;

            float total = 0;
            for ( int x = 0; x < splatsSize; x++ ) {
                for ( int z = 0; z < splatsSize; z++ ) {
                    if ( stop != null && stop.stop )
                        return 0;

                    int matrixPos = ( z + margins ) * fullSize + ( x + margins );

                    float val = matrix.arr[matrixPos];

                    if ( biomeMask != null ) //no empty biomes in list (so no mask == root biome)
                        val *= biomeMask.arr[matrixPos]; //if mask is not assigned biome was ignored, so only main outs with mask==null left here

                    if ( val < 0 )
                        val = 0;
                    if ( val > 1 )
                        val = 1;

                    int colsPos = z * splatsSize + x;
                    cols[colsPos] += val;
                    total += val;
                }
            }
            return total;
        }

        public void Purge ( TileData data, Terrain terrain ) {

        }

        public class ApplyData : IApplyData {
            public Dictionary<string, float[]> colorsForTextureMaskByZoneName;
            public Dictionary<string, float> percentCompositionByZoneName;
            public int resolution;

            public void Read ( Terrain terrain ) {
                throw new System.NotImplementedException();
            }

            public void Apply ( Terrain terrain ) {
                ZoneTile zoneTile = terrain.GetComponent<ZoneTile>();
                if ( this.colorsForTextureMaskByZoneName != null && this.colorsForTextureMaskByZoneName.Count > 0 ) {
                    if ( zoneTile == null ) {
                        zoneTile = terrain.gameObject.AddComponent<ZoneTile>();
                    }
                    zoneTile.resolution = this.resolution;
                    zoneTile.ZoneInfoList = CreateZoneInfoList(this.colorsForTextureMaskByZoneName, this.percentCompositionByZoneName);
                } else if ( zoneTile != null ) {
                    GameObject.Destroy(zoneTile);
                }
            }

            public static ApplyData Empty {
                get {
                    return new ApplyData() {
                        colorsForTextureMaskByZoneName = new Dictionary<string, float[]>(),
                        percentCompositionByZoneName = new Dictionary<string, float>(),
                        resolution = 0
                    };
                }
            }

            public int Resolution {
                get {
                    return this.resolution;
                }
            }
        }

        public static List<ZoneInfo> CreateZoneInfoList ( Dictionary<string, float[]> colorsForTextureMaskByZoneName, Dictionary<string, float> percentCompositionByZoneName ) {
            List<ZoneInfo> zoneInfoList = null;
            if ( colorsForTextureMaskByZoneName != null ) {
                zoneInfoList = new List<ZoneInfo>();
                foreach ( KeyValuePair<string, float[]> entry in colorsForTextureMaskByZoneName ) {
                    string zoneName = entry.Key;
                    float[] colors = entry.Value;
                    float percentComposition = 0f;
                    percentCompositionByZoneName.TryGetValue(zoneName, out percentComposition);
                    if ( zoneName != null && colors != null && colors.Length > 0 && percentComposition > 0f ) {
                        zoneInfoList.Add(new ZoneInfo { zoneName = zoneName, textureMask = colors, percentComposition = percentComposition });
                    }
                }
            }
            return zoneInfoList;
        }

        /*public static List<ZoneInfo> WriteTextures ( int resolution, List<ZoneInfo> oldZoneInfoList, Dictionary<string, Color[]> colorsForTextureMaskByZoneName ) {
            List<ZoneInfo> zoneInfoList = null;
            if ( colorsForTextureMaskByZoneName != null ) {
                Dictionary<string, ZoneInfo> oldZoneInfoByName = null;
                if ( oldZoneInfoList != null ) {
                    oldZoneInfoByName = new Dictionary<string, ZoneInfo>();
                    foreach ( ZoneInfo zoneInfo in oldZoneInfoList ) {
                        if ( zoneInfo != null ) {
                            string zoneName = zoneInfo.zoneName;
                            if ( zoneName != null ) {
                                oldZoneInfoByName.Add(zoneName, zoneInfo);
                            }
                        }
                    }
                }

                zoneInfoList = new List<ZoneInfo>();
                foreach ( KeyValuePair<string, Color[]> entry in colorsForTextureMaskByZoneName ) {
                    string zoneName = entry.Key;
                    Color[] colors = entry.Value;
                    if ( zoneName != null && colors != null && colors.Length > 0 ) {
                        //trying to reuse last used texture
                        Texture2D tex = null;
                        if ( oldZoneInfoByName != null ) {
                            ZoneInfo oldInfo = null;
                            oldZoneInfoByName.TryGetValue(zoneName, out oldInfo);
                            if ( oldInfo != null ) {
                                Texture2D oldTex = oldInfo.textureMask;
                                if ( oldTex != null && oldTex.width == resolution && oldTex.height == resolution ) {
                                    tex = oldTex;
                                }
                            }
                        }
                        if ( tex == null ) {
                            tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, true, true);
                            tex.wrapMode = TextureWrapMode.Mirror; //to avoid border seams
                        }
                        if ( tex != null && colors != null ) {
                            tex.SetPixels(0, 0, tex.width, tex.height, colors);
                            tex.Apply();
                        } else {
                            tex = null;
                        }
                        if ( tex != null ) {
                            zoneInfoList.Add(new ZoneInfo { zoneName = zoneName, textureMask = tex });
                        }
                    }
                }
            }
            return zoneInfoList;
        }*/
    }
}
