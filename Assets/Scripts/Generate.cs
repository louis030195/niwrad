using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay;
using Net.Match;
using Net.Realtime;
using Net.Rpc;
using Net.Session;
using ProceduralTree;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;

public class Generate : MonoBehaviour
{
	private static readonly Dictionary<string, string> Envs = new Dictionary<string, string>
	{
		{"TERRAIN_SIZE", "100"},
		{"INITIAL_ANIMALS", "5"},
		{"INITIAL_PLANTS", "5"},
		{"NAKAMA_IP", "127.0.0.1"},
		{"NAKAMA_PORT", "6666"},
		{"WORKER_ID", "unityIDE"},
	};

	[SerializeField] private Terrain map;
	[SerializeField] private GameObject sessionManagerPrefab;
	[SerializeField] private Slider timescaleSlider;
	[SerializeField] private TextMeshProUGUI timescaleText;

	[Header("Animals configuration")]
	[Tooltip("Percentage position on the diagonal"), Range(0.1f, 0.9f), SerializeField]
	private float animalSpawnCenter = 0.5f;

	[Header("Vegetation configuration")]
	[Range(1, 100_000), SerializeField]
	private int vegetationMaxAmount = 1000;
	[Tooltip("Percentage position on the diagonal"), Range(0.1f, 0.9f), SerializeField]
	private float vegetationSpawnCenter = 0.5f;
// TODO: everything to env vars ?


	private async void Awake()
	{
		// TODO: maybe move whole class to host manager or other ... or change name

		// If there is no session manager it's a host
		if (FindObjectOfType<SessionManager>() == null)
		{
			foreach (DictionaryEntry kv in Environment.GetEnvironmentVariables())
			{
				var k = kv.Key as string;
				var v = kv.Value as string;
				if (k != null) Envs[k] = v;
			}
			foreach (var environmentVariable in Envs)
			{
				// Debug.Log($"env var:{environmentVariable.Key}:{environmentVariable.Value}");
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
			await InitializeNet();
		}
		else
		{
			// Clients can't tweak timescale
			timescaleSlider.gameObject.SetActive(false);
			timescaleText.gameObject.SetActive(false);
		}
		InitializeGameplay();
	}

	private async UniTask InitializeNet()
	{
		Debug.Log($"Trying to connect to nakama at {Envs["NAKAMA_IP"]}:{Envs["NAKAMA_PORT"]}");
		// Server account !
		var (res, msg) = await SessionManager.instance.ConnectAsync("bbbb@bbbb.com",
			"bbbbbbbb",
			ip: Envs["NAKAMA_IP"],
			p: int.Parse(Envs["NAKAMA_PORT"]),
			create: true);
		if (!res)
		{
			Debug.LogError($"Failed to connect to Nakama {msg}");
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#endif
			Application.Quit();
		}
		await SessionManager.instance.ConnectSocketAsync();
		// Join match with null id = create
		await MatchCommunicationManager.instance.JoinMatchAsync(workerId: Envs["WORKER_ID"],
			matchConfiguration:
			new MatchConfiguration
		{
			InitialAnimals = int.Parse(Envs["INITIAL_ANIMALS"]),
			InitialPlants = int.Parse(Envs["INITIAL_PLANTS"]),
			TerrainSize = int.Parse(Envs["TERRAIN_SIZE"])
		});
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
		diamondSquare.ExecuteDiamondSquare(int.Parse(Envs["TERRAIN_SIZE"]));

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
		for (var i = 0; i < int.Parse(Envs["INITIAL_PLANTS"]); i++)
		{
			var p = (s * vegetationSpawnCenter)
				.RandomPositionAroundAboveGroundWithDistance((1 - vegetationSpawnCenter) * s.x,
					LayerMask.GetMask("Vegetation"),
					5f);
			HostManager.instance.SpawnTree(p, Quaternion.identity);
		}

		for (var i = 0; i < int.Parse(Envs["INITIAL_ANIMALS"]); i++)
		{
			var p = (s * animalSpawnCenter)
				.RandomPositionAroundAboveGroundWithDistance((1 - animalSpawnCenter) * s.x,
					LayerMask.GetMask("Animal"),
					5f);
			HostManager.instance.SpawnAnimal(p, Quaternion.identity);
		}
	}
}
