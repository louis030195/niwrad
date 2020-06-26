using System;
using System.Collections;
using System.Threading.Tasks;
using Google.Protobuf;
using Nakama;
using Net.Match;
using Net.Rpc;
using Net.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class MainMenu : MonoBehaviour
	{
		[Tooltip("Prefab containing a match UI block"), SerializeField] private GameObject matchPrefab;

		[SerializeField] private GameObject joinOrCreatePanel;
		[SerializeField] private GameObject createServerPanel;
		[SerializeField] private GameObject scrollViewContent;
		[SerializeField] private Button join;

		private string m_MatchId = string.Empty;
		private TMP_InputField m_TerrainSize;
		private TMP_InputField m_InitialAnimals;
		private TMP_InputField m_InitialPlants;

		private void Start()
		{
			Connect();
			var ifs = createServerPanel.GetComponentsInChildren<TMP_InputField>();
			Debug.Assert(ifs.Length==3, "Create server panel should have 3 children input field");
			m_TerrainSize = ifs[0];
			m_InitialAnimals = ifs[1];
			m_InitialPlants = ifs[2];
		}

		private void Update()
		{
			// Can't join until a match has been selected
			join.interactable = m_MatchId != string.Empty;
		}

		private async void Connect()
		{
			if (!await SessionManager.instance.ConnectSocketAsync())
			{
				Debug.LogError($"Failed to open socket");
				return;
			}
			while (true)
			{
				await RefreshList();
				await Task.Delay(2000);
			}
		}

		private async Task RefreshList()
		{
			// TODO: only delete if changed
			// Use case: scene is unloading
			if (scrollViewContent == null) return;

			// Clear the list
			foreach(Transform child in scrollViewContent.transform)
			{
				Destroy(child.gameObject);
			}

			var matches = await MatchCommunicationManager.instance.GetMatchListAsync();
			foreach (var m in matches)
			{
				// Append the matches to the scroll view content
				var go = Instantiate(matchPrefab, scrollViewContent.transform);

				// Set the button text to the match id
				go.GetComponentInChildren<TextMeshProUGUI>().text = $"ID: {m}";

				// On button click, update the currently selected match id
				go.GetComponentInChildren<Button>().onClick.AddListener(() =>
				{
					Debug.Log($"Match {m} selected");
					m_MatchId = m;
				});
			}
		}

		/// <summary>
		/// Starts the game scene and joins the match
		/// </summary>
		private static IEnumerator LoadGame()
		{
			Destroy(Camera.main);
			SessionManager.instance.isServer = false;
			var asyncLoad = UnityEngine.SceneManagement.SceneManager.
				LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Additive);

			while (!asyncLoad.isDone)
			{
				yield return null;
			}

			UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("SecondMenu");
		}

		public async void JoinMatch()
		{
			await MatchCommunicationManager.instance.JoinMatchAsync(m_MatchId);
			StartCoroutine(LoadGame());
		}

		/// <summary>
		/// Deactivate join panel, activate create panel and vis-versa
		/// </summary>
		public void JoinCreate()
		{
			joinOrCreatePanel.SetActive(!joinOrCreatePanel.activeInHierarchy);
			createServerPanel.SetActive(!joinOrCreatePanel.activeInHierarchy);
		}

		public void CreateServer()
		{
			var tsOk = int.TryParse(m_TerrainSize.text, out var ts);
			var iaOk = int.TryParse(m_InitialAnimals.text, out var ia);
			var ipOk = int.TryParse(m_InitialPlants.text, out var ip);
			Debug.Log($"Asking for server creation with config:\n" +
			          $"Terrain size: {ts}, " +
			          $"Initial animals: {ia}, " +
			          $"Initial plants: {ip}");
			var p = new RunServerRequest
			{
				TerrainSize = tsOk ? ts : 0,
				InitialAnimals = iaOk ? ia : 0,
				InitialPlants = ipOk ? ip : 0,
			}.ToByteString().ToStringUtf8();
			SessionManager.instance.socket.RpcAsync("run_unity_server", p);
		}
	}
}
