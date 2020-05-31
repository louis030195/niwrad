using System;
using System.Collections;
using StateMachine;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class Generate : MonoBehaviour
{
	[Header("Animals configuration")]
	[Range(0, 20)]
	[SerializeField]
	private int spawnDelay = 5; // Ugly hack to wait for navmesh baking (no clean async ...)
	[SerializeField]
	private GameObject animalPrefab;
	[Range(0, 100_000)]
	[SerializeField]
	private int animalAmount = 5;
	[SerializeField]
	private Vector2 animalSpawnCenter = Vector2.zero;
	[Range(0, 100_000)]
	[SerializeField]
	private int animalSpawnCircleRadius = 10;

	[Header("Vegetation configuration")]
	[SerializeField]
	private GameObject vegetationPrefab;
	[Range(0, 100_000)]
	[SerializeField]
	private int vegetationAmount = 5;
	[SerializeField]
	private Vector2 vegetationSpawnCenter = Vector2.one * 10;
	[Range(0, 100_000)]
	[SerializeField]
	private int vegetationSpawnCircleRadius = 10;

	private void Awake()
	{
		Pool.Preload(vegetationPrefab, vegetationAmount);
		Pool.Preload(animalPrefab, animalAmount);
	}

	private void Start()
	{

		for (var i = 0; i < vegetationAmount; i++)
		{
			var p = Random.insideUnitCircle * vegetationSpawnCircleRadius;
			Pool.Spawn(vegetationPrefab, new Vector3(p.x+vegetationSpawnCenter.x,
					1000,
					p.y+vegetationSpawnCenter.y).AboveGround(),
				Quaternion.identity);
		}

		StartCoroutine(Spawn());
	}

	private IEnumerator Spawn()
	{
		yield return new WaitForSeconds(spawnDelay);
		for (var i = 0; i < animalAmount; i++)
		{
			var p = Random.insideUnitCircle * animalSpawnCircleRadius;
			Pool.Spawn(animalPrefab, new Vector3(p.x+animalSpawnCenter.x,
					1000,
					p.y+animalSpawnCenter.y).AboveGround(),
				Quaternion.identity);
		}
	}
}
