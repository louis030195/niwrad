using UnityEngine;

namespace Utils
{
    public static class MaterialHelper
    {
        /// <summary>
        /// https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
        /// </summary>
        /// <returns></returns>
        public static Material RandomMaterial(string shaderName, int resolution=256)
        {
            var noiseTex = CalcNoise(resolution);
            var mat = new Material(Shader.Find(shaderName)) {mainTexture = noiseTex};
            return mat;
        }
        private static Texture2D CalcNoise(int resolution)
        {
            var noiseTex = new Texture2D(resolution, resolution);
            var pix = new Color[noiseTex.width * noiseTex.height];
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
                    pix[(int) y * noiseTex.width + (int) x] = new Color(sample, sample, sample);
                    x++;
                }

                y++;
            }

            // Copy the pixel data to the texture and load it into the GPU.
            noiseTex.SetPixels(pix);
            noiseTex.Apply();
            return noiseTex;
        }

        // TODO: super ugly code !
        public static Texture2D RandomTexture(Transform transform, int resolution = 256, 
            FilterMode filterMode = FilterMode.Trilinear, int anisolevel = 9,
            NoiseMethodType type = NoiseMethodType.Perlin, int dimensions = 3, float frequency = 1f, int octaves = 1, 
            float lacunarity = 2f, float persistence = 0.5f, Gradient gradient = null)
        {
            var texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true)
            {
                name = "Procedural Texture",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = filterMode,
                anisoLevel = anisolevel
            };
            texture.FillTexture(transform, resolution, type, dimensions, frequency, octaves, lacunarity, persistence, gradient);
            return texture;
        }

        public static void FillTexture (this Texture2D texture, Transform transform, int resolution = 256, 
            NoiseMethodType type = NoiseMethodType.Perlin, int dimensions = 3, float frequency = 1f, int octaves = 1, 
            float lacunarity = 2f, float persistence = 0.5f, Gradient gradient = null)
        {
            if (gradient == null)
            {
                gradient = new Gradient();

                // Populate the color keys at the relative time 0 and 1 (0 and 100%)
                var colorKey = new GradientColorKey[3];
                colorKey[0].color = Color.red;
                colorKey[0].time = 0.0f;
                colorKey[1].color = Color.blue;
                colorKey[1].time = 0.5f;
                colorKey[2].color = Color.green;
                colorKey[2].time = 1.0f;

                // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                var alphaKey = new GradientAlphaKey[3];
                alphaKey[0].alpha = 1.0f;
                alphaKey[0].time = 0.0f;
                alphaKey[1].alpha = 1.0f;
                alphaKey[1].time = 0.5f;
                alphaKey[2].alpha = 1.0f;
                alphaKey[2].time = 1.0f;

                gradient.SetKeys(colorKey, alphaKey);
            }

            if (texture.width != resolution) {
                texture.Resize(resolution, resolution);
            }
		
            Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f));
            Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f));
            Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
            Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f));

            NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
            float stepSize = 1f / resolution;
            for (int y = 0; y < resolution; y++) {
                Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
                Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
                for (int x = 0; x < resolution; x++) {
                    Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                    float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
                    if (type != NoiseMethodType.Value) {
                        sample = sample * 0.5f + 0.5f;
                    }
                    texture.SetPixel(x, y, gradient.Evaluate(sample));
                }
            }
            texture.Apply();
        }
    }
}
