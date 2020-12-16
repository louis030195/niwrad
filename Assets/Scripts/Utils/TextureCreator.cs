using UnityEngine;
using Utils;

public class TextureCreator : MonoBehaviour {

	[Range(2, 512)]
	public int resolution = 256;

	public float frequency = 1f;

	[Range(1, 8)]
	public int octaves = 1;

	[Range(1f, 4f)]
	public float lacunarity = 2f;

	[Range(0f, 1f)]
	public float persistence = 0.5f;

	[Range(1, 3)]
    public int dimensions = 3;
    [Range(1, 9)]
    public int anisoLevel = 9;

    public Gradient colouring;

    public NoiseMethodType type;
    public FilterMode filterMode;
    
	private Texture2D texture;
	
	private void OnEnable ()
    {
        Fill();
    }

	private void Update () {
		if (transform.hasChanged) {
			transform.hasChanged = false;
            Fill();
        }
	}

    public void Fill()
    {
        texture = MaterialHelper.RandomTexture(transform, resolution, filterMode, anisoLevel, type, dimensions, 
            frequency, octaves, lacunarity, persistence, colouring);
        GetComponent<MeshRenderer>().material.mainTexture = texture;
    }
}
