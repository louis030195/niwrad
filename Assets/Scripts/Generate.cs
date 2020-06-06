using System.Collections;
using System.Collections.Generic;
using ProceduralTree;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class Generate : Singleton<Generate>
{
	[SerializeField] private Terrain map;

	[Header("Animals configuration"), Range(0, 20), SerializeField]
	private int spawnDelay = 5; // Ugly hack to wait for navmesh baking (no clean async ...)
	[SerializeField]
	private GameObject animalPrefab;
	[Range(0, 100_000), SerializeField]
	private int animalAmount = 5;
	[Tooltip("Percentage position on the diagonal"), Range(0.1f, 0.9f), SerializeField]
	private float animalSpawnCenter = 0.5f;

	[Header("Vegetation configuration"), SerializeField]
	private GameObject vegetationPrefab;
	[Range(0, 100_000), SerializeField]
	private int vegetationAmount = 5;
	[Range(1, 100_000), SerializeField]
	private int vegetationMaxAmount = 1000;
	[Tooltip("Percentage position on the diagonal"), Range(0.1f, 0.9f), SerializeField]
	private float vegetationSpawnCenter = 0.5f;


	private Stack<GameObject> m_TreePool;
	private int m_TreeCount;

	protected override void Awake()
	{
		base.Awake();
		// Pool.Preload(vegetationPrefab, vegetationAmount*10);
		m_TreePool = new Stack<GameObject>();
		Pool.Preload(animalPrefab, animalAmount);
	}

	private IEnumerator FillPoolSlowly()
	{
		// TODO: export to a static "tree pool" or smth like that
		while (m_TreeCount < vegetationMaxAmount)
		{
			var go = Instantiate(vegetationPrefab, Vector3.one*1000, Quaternion.identity);
			var tree = go.GetComponent<ProceduralTree.ProceduralTree>();
			tree.Data.randomSeed = Random.Range(int.MinValue, int.MaxValue);
			yield return new WaitForSeconds(3f);
			m_TreePool.Push(go);
			go.SetActive(false);
			m_TreeCount++;
		}
	}

	private void Start()
	{
		var s = map.terrainData.size;
		for (var i = 0; i < vegetationAmount; i++)
		{
			var p = (s * vegetationSpawnCenter)
				.RandomPositionAroundAboveGroundWithDistance((1 - vegetationSpawnCenter) * s.x,
					LayerMask.GetMask("Vegetation"),
					5f);
			SpawnVegetation(p, Quaternion.identity);
		}
		StartCoroutine(Spawn());
		StartCoroutine(FillPoolSlowly());
	}

	private IEnumerator Spawn()
	{
		var s = map.terrainData.size;
		// animalPrefab.GetComponent<Host>().prefab = animalPrefab;
		yield return new WaitForSeconds(spawnDelay);
		for (var i = 0; i < animalAmount; i++)
		{
			var p = (s * animalSpawnCenter)
				.RandomPositionAroundAboveGroundWithDistance((1 - animalSpawnCenter) * s.x,
					LayerMask.GetMask("Animal"),
					5f);
			Pool.Spawn(animalPrefab, p, Quaternion.identity);
		}
	}

	/// <summary>
	/// Wrapper around spawn
	/// </summary>
	/// <param name="position"></param>
	/// <param name="rotation"></param>
	/// <returns></returns>
	public GameObject SpawnHost(Vector3 position, Quaternion rotation)
	{
		return Pool.Spawn(animalPrefab, position, rotation);
	}

	public GameObject SpawnVegetation(Vector3 position, Quaternion rotation)
	{
		if (m_TreeCount > vegetationMaxAmount) return null;
		m_TreeCount++;
		// var go = Pool.Spawn(vegetationPrefab, position, rotation);
		GameObject go;
		if (m_TreePool.Count == 0)
		{
			go = Instantiate(vegetationPrefab, position, rotation);
		}
		else
		{
			go = m_TreePool.Pop();
			go.transform.position = position;
			go.SetActive(true);
		}

		go.transform.localScale = Vector3.one * Random.Range(1, 2);
		go.transform.localRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);

		var tree = go.GetComponent<ProceduralTree.ProceduralTree>();
		tree.Data.randomSeed = Random.Range(int.MinValue, int.MaxValue);
		return go;
	}
}
