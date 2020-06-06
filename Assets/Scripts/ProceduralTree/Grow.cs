using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralTree {

	[RequireComponent (typeof(MeshRenderer))]
	public class Grow : MonoBehaviour
	{


		[Range(1f, 100_000f)] public float timeToFullSize = 5;
		private Material m_Material;
		private static readonly int T = Shader.PropertyToID(KGrowingKey);

		private const string KGrowingKey = "_T";

		private void OnEnable () {
			m_Material = GetComponent<MeshRenderer>().material;
			m_Material.SetFloat(T, 0f);
		}

		private void Start () {
			StartCoroutine(Growing(timeToFullSize));
		}

		private IEnumerator Growing(float duration) {
			yield return 0;
			var time = 0f;
			while(time < duration) {
				yield return 0;
				m_Material.SetFloat(T, time / duration);
				time += Time.deltaTime;
			}
			m_Material.SetFloat(T, 1f);
		}

		private void OnDestroy() {
			if(m_Material != null) {
				Destroy(m_Material);
				m_Material = null;
			}
		}

	}

}

