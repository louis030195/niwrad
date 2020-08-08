using UnityEngine;

namespace Utils
{
    public static class MaterialHelper
    {
        private static void CalcNoise(Texture2D noiseTex, Color[] pix)
        {
            // For each pixel in the texture...
            var y = 0.0F;

            while (y < noiseTex.height)
            {
                var x = 0.0F;
                while (x < noiseTex.width)
                {
                    var xCoords = x / noiseTex.width;
                    var yCoords = y / noiseTex.height;
                    var sample = Mathf.PerlinNoise(xCoords, yCoords);
                    pix[(int)y * noiseTex.width + (int)x] = new Color(sample, sample, sample);
                    x++;
                }
                y++;
            }

            // Copy the pixel data to the texture and load it into the GPU.
            noiseTex.SetPixels(pix);
            noiseTex.Apply();
        }
        /// <summary>
        /// https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
        /// </summary>
        /// <returns></returns>
        public static Material RandomMaterial(string shaderName)
        {
            var noiseTex = new Texture2D(1, 1);
            var colTex = new Color[noiseTex.width * noiseTex.height];
            var mat = new Material(Shader.Find(shaderName)) {color = Color.green, mainTexture = noiseTex};
            CalcNoise(noiseTex, colTex);
            return mat;
        }
    }
}
