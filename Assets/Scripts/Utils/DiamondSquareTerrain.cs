using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Utils
{
	/// <summary>
	/// When attached to a Terrain GameObject, it can be used to randomized the heights
	/// using the Diamond-Square algorithm
	/// </summary>
	[RequireComponent(typeof(Terrain))]
	// [ExecuteInEditMode]
	public class DiamondSquareTerrain : MonoBehaviour {
		// Data container for heights of a terrain
		private TerrainData m_Data;
		// Size of the sides of a terrain
		private int m_Size;
		// 2D array of heights
		private float[,] m_Heights;

		// Control variable to determine smoothness of heights
		[Range(0.001f, 1.999f)]
		[SerializeField]
		private float roughness = 0.8f;
		// Flag to set random corner heights when terrain is reset
		[SerializeField]
		private bool randomizeCornerValues;
		[SerializeField]
		private NavMeshSurface[] surfaces;

		/// <summary>
		/// Whether or not the navmesh has been baked on the generated terrain
		/// </summary>
		public bool navMeshBaked;

		/// <summary>
		/// Used for initialization
		/// </summary>
		private void Awake()
		{
			m_Data = new TerrainData {size = new Vector3(100, 100, 100)};
			GetComponent<Terrain>().terrainData = m_Data;
			GetComponent<TerrainCollider>().terrainData = m_Data;
        	m_Size = m_Data.heightmapResolution;
			Reset();
		}


		/// <summary>
		/// Flips the value of the randomizeCornerValues flag
		/// </summary>
		public void ToggleRandomizeCornerValues() {
			randomizeCornerValues = !randomizeCornerValues;
		}

		/// <summary>
		/// Resets the values of the terrain. If randomizeCornerValues is true then the
		/// corner heights will be randomized, else it will be flat.
		/// </summary>
		public void Reset()
		{
			navMeshBaked = false;
			m_Heights = new float[m_Size, m_Size];

			// If the corners need to be randomized
			if (randomizeCornerValues) {
				m_Heights[0, 0] = Random.value;
				m_Heights[m_Size - 1, 0] = Random.value;
				m_Heights[0, m_Size - 1] = Random.value;
				m_Heights[m_Size - 1, m_Size - 1] = Random.value;
			}

			// Update the terrain data
			m_Data.SetHeights(0, 0, m_Heights);
			foreach (var t in surfaces)
			{
				t.BuildNavMesh ();
			}
		}

		/// <summary>
		/// Executes the DiamondSquare algorithm on the terrain.
		/// </summary>
		public void ExecuteDiamondSquare() {
			m_Heights = new float[m_Size, m_Size];
			var range = 0.5f;
			int sideLength;

			// While the side length is greater than 1
			for (sideLength = m_Size - 1; sideLength > 1; sideLength /= 2) {
				var halfSide = sideLength / 2;

				// Run Diamond Step
				float average;
				int x, y;
				for (x = 0; x < m_Size - 1; x += sideLength) {
					for (y = 0; y < m_Size - 1; y += sideLength) {
						// Get the average of the corners
						average = m_Heights[x, y];
						average += m_Heights[x + sideLength, y];
						average += m_Heights[x, y + sideLength];
						average += m_Heights[x + sideLength, y + sideLength];
						average /= 4.0f;

						// Offset by a random value
						average += (Random.value * (range * 2.0f)) - range;
						m_Heights[x + halfSide, y + halfSide] = average;
					}
				}

				// Run Square Step
				for (x = 0; x < m_Size - 1; x += halfSide) {
					for (y = (x + halfSide) % sideLength; y < m_Size - 1; y += sideLength) {
						// Get the average of the corners
						average = m_Heights[(x - halfSide + m_Size - 1) % (m_Size - 1), y];
						average += m_Heights[(x + halfSide) % (m_Size - 1), y];
						average += m_Heights[x, (y + halfSide) % (m_Size - 1)];
						average += m_Heights[x, (y - halfSide + m_Size - 1) % (m_Size - 1)];
						average /= 4.0f;

						// Offset by a random value
						average += (Random.value * (range * 2.0f)) - range;

						// Set the height value to be the calculated average
						m_Heights[x, y] = average;

						// Set the height on the opposite edge if this is
						// an edge piece
						if (x == 0) {
							m_Heights[m_Size - 1, y] = average;
						}

						if (y == 0) {
							m_Heights[x, m_Size - 1] = average;
						}
					}
				}

				// Lower the random value range
				range -= range * 0.5f * roughness;
			}

			// Update the terrain heights
			m_Data.SetHeights(0, 0, m_Heights);

			// Pain textures
			// Splat-map data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
	        // var splatMapData = new float[m_Data.alphamapWidth, m_Data.alphamapHeight, m_Data.alphamapLayers];
	        //
	        // for (int y = 0; y < m_Data.alphamapHeight; y++)
	        // {
	        //     for (int x = 0; x < m_Data.alphamapWidth; x++)
	        //     {
		       //      // Normalise x/y coordinates to range 0-1
		       //      var y01 = y/(float)m_Data.alphamapHeight;
		       //      var x01 = x/(float)m_Data.alphamapWidth;
	        //
		       //      // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the height-map array)
		       //      float height = m_Data.GetHeight(Mathf.RoundToInt(y01 * m_Data.heightmapResolution),
			      //       Mathf.RoundToInt(x01 * m_Data.heightmapResolution) );
	        //
		       //      // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
		       //      Vector3 normal = m_Data.GetInterpolatedNormal(y01,x01);
	        //
		       //      // Calculate the steepness of the terrain
		       //      float steepness = m_Data.GetSteepness(y01,x01);
	        //
		       //      // Setup an array to record the mix of texture weights at this point
		       //      float[] splatWeights = new float[m_Data.alphamapLayers];
	        //
		       //      // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT
	        //
		       //      // Texture[0] has constant influence
		       //      splatWeights[0] = 0.5f;
	        //
		       //      // Texture[1] is stronger at lower altitudes
		       //      splatWeights[1] = Mathf.Clamp01((m_Data.heightmapResolution - height));
	        //
		       //      // Texture[2] stronger on flatter terrain
		       //      // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of height-map height and scale factor
		       //      // Subtract result from 1.0 to give greater weighting to flat surfaces
		       //      splatWeights[2] = 1.0f - Mathf.Clamp01(steepness*steepness/(m_Data.heightmapResolution/5.0f));
	        //
		       //      // Texture[3] increases with height but only on surfaces facing positive Z axis
		       //      splatWeights[3] = height * Mathf.Clamp01(normal.z);
	        //
		       //      // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
		       //      float z = splatWeights.Sum();
	        //
		       //      // Loop through each terrain texture
		       //      for(int i = 0; i<m_Data.alphamapLayers; i++){
	        //
			      //       // Normalize so that sum of all texture weights = 1
			      //       splatWeights[i] /= z;
	        //
			      //       // Assign this point to the splat-map array
			      //       splatMapData[x, y, i] = splatWeights[i];
		       //      }
	        //     }
	        // }
	        //
	        // // Finally assign the new splat-map to the terrainData:
	        // m_Data.SetAlphamaps(0, 0, splatMapData);

			foreach (var t in surfaces)
			{
				t.BuildNavMesh ();
			}

			navMeshBaked = true;
		}

		/// <summary>
		/// Returns the amount of vertices to skip using the given depth.
		/// </summary>
		/// <param name="depth">The vertice detail depth on the height array</param>
		/// <returns>Amount of vertices to skip</returns>
		public int GetStepSize(int depth) {
			// Return an invalid step size if the depth is invalid
			if (!ValidateDepth(depth)) {
				return -1;
			}

			// Return the amount of vertices to skip
			return (int)((m_Size - 1) / Mathf.Pow(2, (depth - 1)));
		}

		/// <summary>
		/// Returns the maximum depth for this terrain's size.
		/// </summary>
		/// <returns>Maximum depth for this terrain</returns>
		private int MaxDepth() {
			// 0.69314718056f = Natural Log of 2
			return (int)(Mathf.Log(m_Size - 1) / 0.69314718056f + 1);
		}

		/// <summary>
		/// Returns false if the depth is above zero and below maximum depth, true otheriwse
		/// </summary>
		/// <param name="depth">The vertice detail depth on the height array</param>
		/// <returns></returns>
		private bool ValidateDepth(int depth) {
			if (depth > 0 && depth <= MaxDepth()) {
				return true;
			}

			return false;
		}
	}
}
