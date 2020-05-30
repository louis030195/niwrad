using System;
using StateMachine;
using UnityEngine;
using Random = UnityEngine.Random;


public class Generate : MonoBehaviour
{

	[Header("Animals configuration")]
	[SerializeField]
	private GameObject animalPrefab;
	[SerializeField]
	private int animalAmount = 5;
	[SerializeField]
	private Vector2 animalSpawnCenter = Vector2.zero;
	[SerializeField]
	private int animalSpawnCircleRadius = 10;

	[Header("Vegetation configuration")]
	[SerializeField]
	private GameObject vegetationPrefab;
	[SerializeField]
	private int vegetationAmount = 5;
	[SerializeField]
	private Vector2 vegetationSpawnCenter = Vector2.one * 10;
	[SerializeField]
	private int vegetationSpawnCircleRadius = 10;

	private void Start()
	{
		for (var i = 0; i < animalAmount; i++)
		{
			var p = Random.insideUnitCircle * animalSpawnCircleRadius;
			Instantiate(animalPrefab, new Vector3(p.x+animalSpawnCenter.x, 0.5f, p.y+animalSpawnCenter.y),
				Quaternion.identity).GetComponent<StateController>().SetupAi(true);
		}
		for (var i = 0; i < vegetationAmount; i++)
		{
			var p = Random.insideUnitCircle * animalSpawnCircleRadius;
			Instantiate(vegetationPrefab, new Vector3(p.x+vegetationSpawnCenter.x, 0.5f, p.y+vegetationSpawnCenter.y),
				Quaternion.identity);
		}
	}
}
