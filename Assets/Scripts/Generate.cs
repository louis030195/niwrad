using System.Collections;
using Gameplay;
using Net.Match;
using Net.Realtime;
using Net.Session;
using Net.Utils;
using ProceduralTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Quaternion = UnityEngine.Quaternion;

public class Generate : MonoBehaviour
{
	[SerializeField] private Terrain map;
	[SerializeField] private GameObject sessionManagerPrefab;
	[SerializeField] private Slider timescaleSlider;
	[SerializeField] private TextMeshProUGUI timescaleText;

	[Header("Animals configuration")]
	[SerializeField]
	private GameObject animalPrefab;
	[Range(0, 100_000), SerializeField]
	private int animalAmount = 5;
	[Tooltip("Percentage position on the diagonal"), Range(0.1f, 0.9f), SerializeField]
	private float animalSpawnCenter = 0.5f;

	[Header("Vegetation configuration")]
	[Range(0, 100_000), SerializeField]
	private int vegetationAmount = 5;
	[Range(1, 100_000), SerializeField]
	private int vegetationMaxAmount = 1000;
	[Tooltip("Percentage position on the diagonal"), Range(0.1f, 0.9f), SerializeField]
	private float vegetationSpawnCenter = 0.5f;

	private void Awake()
	{
		// TODO: maybe move whole class to host manager or other ... or change name
		Pool.Preload(animalPrefab, animalAmount*100); // TODO: move to hm

		// If there is already a session manager, it's a client
		if (FindObjectOfType<SessionManager>() == null)
		{
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
	}

	private async void InitializeNet()
	{
		// Server account !
		await SessionManager.instance.ConnectAsync("bbbb@bbbb.com", "bbbbbbbb");
		await SessionManager.instance.ConnectSocketAsync();
		// Join match with null id = create
		await MatchCommunicationManager.instance.JoinMatchAsync();
		SessionManager.instance.isServer = true;
	}

	private void Start()
	{
		// Ugly as hell hack to initialize mtd in main thread
		// var i = MainThreadDispatcher.instance;
		StartCoroutine(InitializeGameplay());
	}

	private IEnumerator InitializeGameplay()
	{
		// Seems to be best to wait a bit before spawning things as there is navmesh baking
		// Camera stuff, opengl thing
		Debug.Log($"Initializing gameplay ...");
		yield return new WaitUntil(() => MatchCommunicationManager.instance.seed != -1);
		Random.InitState(MatchCommunicationManager.instance.seed);
		Debug.Log($"Seed loaded value: {MatchCommunicationManager.instance.seed}");

		// Once the seed is loaded, we can generate the map to have a deterministically same map than others
		var diamondSquare = map.GetComponent<DiamondSquareTerrain>();
		Debug.Log($"Generating map and navmesh");
		diamondSquare.ExecuteDiamondSquare();

		// Wait until it's generated and baked
		yield return new WaitUntil(() => diamondSquare.navMeshBaked);
		Debug.Log($"Navmesh baked, ready for gameplay");
		// Notifying self and others that we can handle game play
		MatchCommunicationManager.instance.Rpc(new Packet
			{
				Initialized = new Packet.Types.InitializedPacket()
			}
			.Basic(), Recipient.All);

		// Start filling the pool
		TreePool.instance.FillSlowly(vegetationMaxAmount);

		// From now the server handle the spawning
		if (!SessionManager.instance.isServer)
		{
			// gameObject.SetActive(false);
			Destroy(this);
		}

		var s = map.terrainData.size;
		for (var i = 0; i < vegetationAmount; i++)
		{
			var p = (s * vegetationSpawnCenter)
				.RandomPositionAroundAboveGroundWithDistance((1 - vegetationSpawnCenter) * s.x,
					LayerMask.GetMask("Vegetation"),
					5f);
			HostManager.instance.SpawnTree(p, Quaternion.identity);
		}
		for (var i = 0; i < animalAmount; i++)
		{
			var p = (s * animalSpawnCenter)
				.RandomPositionAroundAboveGroundWithDistance((1 - animalSpawnCenter) * s.x,
					LayerMask.GetMask("Animal"),
					5f);
			HostManager.instance.SpawnAnimal(p, Quaternion.identity);
		}
	}
}
