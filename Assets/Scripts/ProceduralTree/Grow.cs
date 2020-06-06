using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralTree {

	[RequireComponent (typeof(MeshRenderer))]
	public class Grow : MonoBehaviour
	{


		[Range(1f, 100_000f)] public float timeToFullSize = 5;
		Material material;

		const string kGrowingKey = "_T";

		void OnEnable () {
			material = GetComponent<MeshRenderer>().material;
			material.SetFloat(kGrowingKey, 0f);
		}

		void Start () {
			StartCoroutine(IGrowing(timeToFullSize));
		}

		IEnumerator IGrowing(float duration) {
			yield return 0;
			var time = 0f;
			while(time < duration) {
				yield return 0;
				material.SetFloat(kGrowingKey, time / duration);
				time += Time.deltaTime;
			}
			material.SetFloat(kGrowingKey, 1f);
		}

		void OnDestroy() {
			if(material != null) {
				Destroy(material);
				material = null;
			}
		}

	}

}

