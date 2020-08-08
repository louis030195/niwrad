using UnityEngine;

namespace Utils
{
    /// <summary>
    /// This static class should offers the possibility to generate with a high-level API terrain procedurally
    /// using wide range of different algorithms
    /// </summary>
    public static class ProceduralTerrain
    {
        public static GameObject Generate(int n, int height, int seed, float spread, float spreadReductionRate) 
        {
            var terrain = new GameObject("Terrain");
            var t = terrain.AddComponent<Terrain>();
            t.materialTemplate = MaterialHelper.RandomMaterial("Nature/Terrain/Standard");
            
            var td = 
                t.terrainData = 
                    terrain.AddComponent<TerrainCollider>().terrainData = new TerrainData();
            td.heightmapResolution = (int)Mathf.Pow(2, n);
            td.alphamapResolution = (int)Mathf.Pow(2, n);
            t.heightmapPixelError = 0;
            td.SetHeights(0, 0, MidpointDisplacement.CreateHeightmap(n, seed, spread, spreadReductionRate));
            td.size = new Vector3(td.heightmapResolution, height, td.heightmapResolution);
            return terrain;
        }

        /// <summary>
        /// Returns the center of the terrain (including center in Y axis yes)
        /// </summary>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static Vector3 GetCenter(this Terrain terrain)
        {
            return Vector3.Lerp(terrain.terrainData.size, terrain.transform.position, 0.5f);
        }
    }
}
