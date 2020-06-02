using System.Collections;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class Generate : Singleton<Generate>
{
	[SerializeField] private Terrain map;

	[Header("Animals configuration")]
	[Range(0, 20)]
	[SerializeField]
	private int spawnDelay = 5; // Ugly hack to wait for navmesh baking (no clean async ...)
	[SerializeField]
	private GameObject animalPrefab;
	[Range(0, 100_000)]
	[SerializeField]
	private int animalAmount = 5;
	[Tooltip("Percentage position on the diagonal")]
	[Range(0.1f, 0.9f)]
	[SerializeField]
	private float animalSpawnCenter = 0.5f;

	[Header("Vegetation configuration")]
	[SerializeField]
	private GameObject vegetationPrefab;
	[Range(0, 100_000)]
	[SerializeField]
	private int vegetationAmount = 5;
	[Tooltip("Percentage position on the diagonal")]
	[Range(0.1f, 0.9f)]
	[SerializeField]
	private float vegetationSpawnCenter = 0.5f;


	protected override void Awake()
	{
		base.Awake();
		Pool.Preload(vegetationPrefab, vegetationAmount);
		Pool.Preload(animalPrefab, animalAmount);
	}

	private void Start()
	{
		var s = map.terrainData.size;
		for (var i = 0; i < vegetationAmount; i++)
		{
			var p = s * vegetationSpawnCenter + Random.insideUnitSphere * (0.1f * map.terrainData.size.x);
			Pool.Spawn(vegetationPrefab, p.PositionAboveGround(), Quaternion.identity);
		}

		StartCoroutine(Spawn());
	}

	private IEnumerator Spawn()
	{
		var s = map.terrainData.size;
		// animalPrefab.GetComponent<Host>().prefab = animalPrefab;
		yield return new WaitForSeconds(spawnDelay);
		for (var i = 0; i < animalAmount; i++)
		{
			var p = s * animalSpawnCenter + Random.insideUnitSphere * (0.1f * map.terrainData.size.x);
			Pool.Spawn(animalPrefab, p.PositionAboveGround(), Quaternion.identity);
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
		return Pool.Spawn(vegetationPrefab, position, rotation);
	}
}
