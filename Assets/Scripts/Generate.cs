using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay;
using Net.Match;
using Net.Realtime;
using Net.Rpc;
using Net.Session;
using Net.Utils;
using ProceduralTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;

public class Generate : MonoBehaviour
{
	private const string CliTerrainSize = "--terrainSize";
	private const string CliInitialAnimals = "--initialAnimals"; // TODO: maybe just pick protobuf prop names
	private const string CliInitialPlants = "--initialPlants";

	[SerializeField] private Terrain map;
	[SerializeField] private GameObject sessionManagerPrefab;
	[SerializeField] private Slider timescaleSlider;
	[SerializeField] private TextMeshProUGUI timescaleText;

	// [Range(0, 100_000), SerializeField]
	private int m_TerrainSize = 100;

	[Header("Animals configuration")]
	[SerializeField]
	private GameObject animalPrefab;
	// [Range(0, 100_000), SerializeField]
	private int m_AnimalAmount = 5;
	[Tooltip("Percentage position on the diagonal"), Range(0.1f, 0.9f), SerializeField]
	private float animalSpawnCenter = 0.5f;

	[Header("Vegetation configuration")]
	// [Range(0, 100_000), SerializeField]
	private int m_VegetationAmount = 5;
	[Range(1, 100_000), SerializeField]
	private int vegetationMaxAmount = 1000;
	[Tooltip("Percentage position on the diagonal"), Range(0.1f, 0.9f), SerializeField]
	private float vegetationSpawnCenter = 0.5f;

	private void Awake()
	{
		// TODO: maybe move whole class to host manager or other ... or change name

		// If there is already a session manager, it's a client
		if (FindObjectOfType<SessionManager>() == null)
		{
			// Helper function for getting the command line arguments
			var args = Environment.GetCommandLineArgs();
			for (var i = 0; i < args.Length; i++)
			{
				// Got the cli param and it's value ?
				if (args[i] == CliTerrainSize && args.Length > i + 1)
				{
					m_TerrainSize = int.Parse(args[i + 1]); // We trust nakama for giving parseable args
				}
				if (args[i] == CliInitialAnimals && args.Length > i + 1)
				{
					m_AnimalAmount = int.Parse(args[i + 1]);
				}
				if (args[i] == CliInitialPlants && args.Length > i + 1)
				{
					m_VegetationAmount = int.Parse(args[i + 1]);
				}

				var ar = args.Length > i + 1 ? args[i + 1] : string.Empty;
				// Debug.Log($"args: {args[i]}: {ar}");
			}


			// Only server can change timescale
			// TODO: break everything ?
			timescaleSlider.onValueChanged.AddListener(value =>
			{
				// Debug.Log($"Timescale changed: {value}");
				Time.timeScale = value;
				timescaleText.text = $"{value}";
			});
			Instantiate(sessionManagerPrefab);
			InitializeNet();
		}
		else
		{
			// Clients can't tweak timescale
			timescaleSlider.gameObject.SetActive(false);
			timescaleText.gameObject.SetActive(false);
		}
		InitializeGameplay();
	}

	private async void InitializeNet()
	{
		// Server account !
		await SessionManager.instance.ConnectAsync("bbbb@bbbb.com", "bbbbbbbb"/*, ip:"192.168.1.20"*/);
		await SessionManager.instance.ConnectSocketAsync();
		// Join match with null id = create
		await MatchCommunicationManager.instance.JoinMatchAsync();
		SessionManager.instance.isServer = true;
	}

	private async void InitializeGameplay()
	{
		// Seems to be best to wait a bit before spawning things as there is navmesh baking
		// Camera stuff, opengl thing
		Debug.Log($"Initializing gameplay ...");
		await UniTask.WaitUntil(() => MatchCommunicationManager.instance.seed != -1);
		Random.InitState(MatchCommunicationManager.instance.seed);
		Debug.Log($"Seed loaded value: {MatchCommunicationManager.instance.seed}");

		// Once the seed is loaded, we can generate the map to have a deterministically same map than others
		var diamondSquare = map.GetComponent<DiamondSquareTerrain>();
		Debug.Log($"Generating map and navmesh");
		diamondSquare.ExecuteDiamondSquare(m_TerrainSize);

		// Wait until it's generated and baked
		await UniTask.WaitUntil(() => diamondSquare.navMeshBaked);
		Debug.Log($"Navmesh baked, ready for gameplay");
		// Notifying self and others that we can handle game play
		var msg = new Packet {Initialized = new Initialized()};
		foreach (var instancePlayer in MatchCommunicationManager.instance.players)
		{
			msg.Recipients.Add(instancePlayer.UserId);
		}
		MatchCommunicationManager.instance.RpcAsync(msg);

		// Start filling the pool
		TreePool.instance.FillSlowly(vegetationMaxAmount);

		// From now the server handle the spawning
		if (!SessionManager.instance.isServer)
		{
			return;
		}

		// ?
		await UniTask.Delay(1000);

		var s = map.terrainData.size;
		for (var i = 0; i < m_VegetationAmount; i++)
		{
			var p = (s * vegetationSpawnCenter)
				.RandomPositionAroundAboveGroundWithDistance((1 - vegetationSpawnCenter) * s.x,
					LayerMask.GetMask("Vegetation"),
					5f);
			HostManager.instance.SpawnTree(p, Quaternion.identity);
		}

		for (var i = 0; i < m_AnimalAmount; i++)
		{
			var p = (s * animalSpawnCenter)
				.RandomPositionAroundAboveGroundWithDistance((1 - animalSpawnCenter) * s.x,
					LayerMask.GetMask("Animal"),
					5f);
			HostManager.instance.SpawnAnimal(p, Quaternion.identity);
		}
	}
}
