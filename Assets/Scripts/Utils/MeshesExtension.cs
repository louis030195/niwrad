using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
	public static class MeshesExtension
	{
		public static Mesh Mutation(Mesh originalMesh)
		{
			var clonedMesh = new Mesh
			{
				name = "clone",
				vertices = originalMesh.vertices,
				triangles = originalMesh.triangles,
				normals = originalMesh.normals,
				uv = originalMesh.uv
			};

			for (var i = 0; i < 10; i++)
			{
				var r = clonedMesh.vertices.AnyItem();
				clonedMesh.PullSimilarVertices(r, r+Random.insideUnitSphere*0.1f);
			}

			return clonedMesh;
		}

		/// returns List of int that is related to the targetPt.
		private static List<int> FindRelatedVertices(this Mesh mesh, Vector3 targetPt, bool findConnected)
		{
			var relatedVertices = new List<int>();

			int idx;
			Vector3 pos;

			// loop through triangle array of indices
			for (int t = 0; t < mesh.triangles.Length; t++)
			{
				// current idx return from tris
				idx = mesh.triangles[t];
				// current pos of the vertex
				pos = mesh.vertices[idx];
				// if current pos is same as targetPt
				if (pos == targetPt)
				{
					// add to list
					relatedVertices.Add(idx);
					// if find connected vertices
					if (findConnected)
					{
						// min
						// - prevent running out of count
						if (t == 0)
						{
							relatedVertices.Add(mesh.triangles[t + 1]);
						}
						// max
						// - prevent runnign out of count
						if (t == mesh.triangles.Length - 1)
						{
							relatedVertices.Add(mesh.triangles[t - 1]);
						}
						// between 1 ~ max-1
						// - add idx from triangles before t and after t
						if (t > 0 && t < mesh.triangles.Length - 1)
						{
							relatedVertices.Add(mesh.triangles[t - 1]);
							relatedVertices.Add(mesh.triangles[t + 1]);
						}
					}
				}
			}
			// return compiled list of int
			return relatedVertices;
		}
		private static void PullSimilarVertices(this Mesh mesh, Vector3 targetVertexPos, Vector3 newPos)
		{
			var relatedVertices = mesh.FindRelatedVertices(targetVertexPos, false);
			var nv = new Vector3[mesh.vertices.Length];
			foreach (var i in relatedVertices)
			{
				nv[i] = newPos;
			}

			mesh.vertices = nv;
			mesh.RecalculateNormals();
		}
	}
}
