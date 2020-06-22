using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace ProceduralTree
{
	public class TreePool : Singleton<TreePool>
	{
		public event Action Spawned;
		public event Action Despawned;

		[SerializeField] private GameObject prefab;
		private Stack<GameObject> m_TreePool;
		private int m_TreeCount;

		protected override void Awake()
		{
			base.Awake();
			m_TreePool = new Stack<GameObject>();
		}

		/// <summary>
		/// Slowly generate trees into a pool over time to spray the computation.
		/// </summary>
		/// <param name="maxTrees"></param>
		/// <param name="delayBetweenFills"></param>
		/// <param name="initialTrees"></param>
		public void FillSlowly(int maxTrees, float delayBetweenFills = 100f, int initialTrees = 20)
		{
			// TODO: convert to unitask
			StartCoroutine(FillSlowly(0, initialTrees)); // Fill initial
			StartCoroutine(FillSlowly(delayBetweenFills, maxTrees));
		}

		/// <summary>
		/// Spawn a new tree and returns the object and it's random seed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public (ProceduralTree go, int seed) Spawn(Vector3 position, Quaternion rotation)
		{
			Spawned?.Invoke();
			m_TreeCount++;
			GameObject go;
			if (m_TreePool.Count == 0)
			{
				go = Instantiate(prefab);
			}
			else
			{
				go = m_TreePool.Pop();
				go.SetActive(true);
			}
			go.transform.position = position;
			go.transform.rotation = rotation;
			var tree = go.GetComponent<ProceduralTree>();
			tree.Data.randomSeed = Random.Range(int.MinValue, int.MaxValue);
			return (tree, tree.Data.randomSeed);
		}

		/// <summary>
		/// Despawn a tree into the pool
		/// </summary>
		/// <param name="obj"></param>
		public void Despawn(GameObject obj)
		{
			Despawned?.Invoke();
			obj.SetActive(false);
			m_TreePool.Push(obj);
		}

		private IEnumerator FillSlowly(float delayBetweenFills, int max)
		{
			// TODO: one solution to have same seed sync across net is to make an event
			// triggered when a slow fill is done, the seed should be sync-ed
			while (m_TreeCount < max)
			{
				var go = Instantiate(prefab, Vector3.one*1000, Quaternion.identity);
				var tree = go.GetComponent<ProceduralTree>();
				// TODO: all clients should receive same seed
				tree.Data.randomSeed = Random.Range(int.MinValue, int.MaxValue);
				yield return new WaitForSeconds(delayBetweenFills);
				go.SetActive(false);
				m_TreePool.Push(go);
				m_TreeCount++;
			}
		}
	}
}
