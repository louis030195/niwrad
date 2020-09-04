using System;
using Cysharp.Threading.Tasks;
using Api.Match;
using Api.Session;
using ProceduralTree;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
	public class EvolutionPanel : MonoBehaviour
	{
		[Tooltip("It's used to track the birth and death of animals"), SerializeField]
		private GameObject animalPrefab;
		[SerializeField]
		private TextMeshProUGUI animalBirths;
		[SerializeField]
		private TextMeshProUGUI vegetationBirths;

		[SerializeField]
		private TextMeshProUGUI animalDeaths;
		[SerializeField]
		private TextMeshProUGUI vegetationDeaths;


		[SerializeField]
		private TextMeshProUGUI animalGenerations; // TODO: how to implement this ?
		[SerializeField]
		private TextMeshProUGUI vegetationGenerations;

		private async void Start()
		{
			await UniTask.WaitUntil(() => Mcm.instance != null);
			Mcm.instance.Initialized += OnInitialized;
		}

		private void OnInitialized(string _)
		{
			Pool.Spawned[animalPrefab] += IncrementAnimalBirths;
			TreePool.instance.Spawned += IncrementVegetationBirths;

			Pool.Despawned[animalPrefab] += IncrementAnimalDeaths;
			TreePool.instance.Despawned += IncrementVegetationDeaths;
		}

		private void IncrementAnimalBirths()
		{
			var val = Convert.ToInt32(animalBirths.text);
			animalBirths.text = $"{val+1}";
		}

		private void IncrementVegetationBirths()
		{
			var val = Convert.ToInt32(vegetationBirths.text);
			vegetationBirths.text = $"{val+1}";
		}

		private void IncrementAnimalDeaths()
		{
			var val = Convert.ToInt32(animalDeaths.text);
			animalDeaths.text = $"{val+1}";
		}

		private void IncrementVegetationDeaths()
		{
			var val = Convert.ToInt32(vegetationDeaths.text);
			vegetationDeaths.text = $"{val+1}";
		}
	}
}
