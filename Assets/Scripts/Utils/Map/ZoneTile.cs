using System.Collections.Generic;
using UnityEngine;

namespace Utils.Map
{
    [ExecuteInEditMode]
    public class ZoneTile : MonoBehaviour {

        public List<ZoneInfo> ZoneInfoList;
        public int resolution = 0;
        protected Terrain terrain;
        protected float terrainDataSizeX = 0f;
        protected float terrainDataSizeZ = 0f;
        protected int alphamapWidth = 0;
        protected int alphamapHeight = 0;

        void Awake () {
            this.terrain = this.GetComponent<Terrain>();
            if ( this.terrain != null ) {
                TerrainData terrainData = this.terrain.terrainData;
                if ( terrainData != null ) {
                    Vector3 terrainDataSize = terrainData.size;
                    this.terrainDataSizeX = terrainDataSize.x;
                    this.terrainDataSizeZ = terrainDataSize.z;
                    this.alphamapWidth = terrainData.alphamapWidth;
                    this.alphamapHeight = terrainData.alphamapHeight;
                }
            }
        }

        void OnEnable () {
            ZoneManager.RegisterZoneTile(this);
        }

        void OnDisable () {
            ZoneManager.UnregisterZoneTile(this);
        }

        public ZoneInfo GetActiveZoneInfo ( Transform transform ) {
            ZoneInfo activeZoneInfo = null;
            if ( this.ZoneInfoList != null && this.ZoneInfoList.Count > 0 && transform != null && ZoneManager.Contains(this.terrain, transform.position) ) {
                Vector3 positionRelativeToTerrain = ZoneManager.getRelativePosition(this.terrain, transform.position);
                int textureMaskPosition = getTextureMapPosition(positionRelativeToTerrain);
                float maxStrength = 0f;
                foreach ( ZoneInfo zoneInfo in this.ZoneInfoList ) {
                    float strength = getZoneStrength(zoneInfo.textureMask, textureMaskPosition);
                    if ( strength > maxStrength ) {
                        maxStrength = strength;
                        activeZoneInfo = zoneInfo;
                    }
                }
            }
            return activeZoneInfo;
        }

        protected bool Contains ( ZoneInfo zoneInfo, Vector3 positionRelativeToTerrain ) {
            bool contains = false;
            if ( zoneInfo != null ) {
                contains = Contains(zoneInfo.textureMask, this.getTextureMapPosition(positionRelativeToTerrain));
            }
            return contains;
        }

        public static bool Contains ( float[] textureMask, int textureMaskPosition ) {
            return getZoneStrength(textureMask, textureMaskPosition) > 0f;
        }

        protected float getZoneStrength ( ZoneInfo zoneInfo, Vector3 positionRelativeToTerrain ) {
            float strength = 0f;
            if ( zoneInfo != null ) {
                strength = getZoneStrength(zoneInfo.textureMask, this.getTextureMapPosition(positionRelativeToTerrain));
            }
            return strength;
        }

        public static float getZoneStrength ( float[] textureMask, int textureMaskPosition ) {
            return textureMaskPosition >= 0 && textureMaskPosition < textureMask.Length ? textureMask[textureMaskPosition] : 0f;
        }


        public int getTextureMapPosition ( Vector3 positionRelativeToTerrain ) {
            return getTextureMapPosition(this.resolution, positionRelativeToTerrain, this.terrainDataSizeX, this.terrainDataSizeZ, this.alphamapWidth, this.alphamapHeight);
        }

        public static int getTextureMapPosition ( int resolution, Vector3 positionRelativeToTerrain, float terrainDataSizeX,
            float terrainDataSizeZ, int alphamapWidth, int alphamapHeight ) {
            Vector3 mapPosition = new Vector3(positionRelativeToTerrain.x / terrainDataSizeX, 0f, positionRelativeToTerrain.z / terrainDataSizeZ);
            int x = (int) ( mapPosition.x * alphamapWidth );
            int z = (int) ( mapPosition.z * alphamapHeight );
            return z * resolution + x;
        }
    }
}
