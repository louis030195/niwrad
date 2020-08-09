using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Nakama;
using Nakama.TinyJson;
using Api.Match;
using Api.Rpc;
using Api.Session;
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

		private float m_MatchPrefabHeight = 30f;
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
			m_MatchPrefabHeight = matchPrefab.GetComponent<RectTransform>().sizeDelta.y;
			RefreshList();
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
			}
		}

		public async void RefreshList()
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
			for (var i = 0; i < matches.Length; i++)
			{
				var m = matches[i];

				// Append the matches to the scroll view content
				var go = Instantiate(matchPrefab, scrollViewContent.transform);

				var p = go.transform.localPosition;
				// Spacing between matches
				go.transform.localPosition = new Vector3(p.x,
					-i * m_MatchPrefabHeight,
					p.z);

				// Set the button text to the match id
				go.GetComponentInChildren<TextMeshProUGUI>().text = $"ID: {m}";

				// On button click, update the currently selected match id
				go.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
				{
					Debug.Log($"Match {m} selected");
					m_MatchId = m;
				});
                go.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
				{
					StopMatch(m);
				});
			}
		}

		/// <summary>
		/// Starts the game scene and joins the match
		/// </summary>
		private async void LoadGame()
		{
			Destroy(Camera.main);
			SessionManager.instance.isServer = false;
			await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game");
		}

		public async void JoinMatch()
		{
			await MatchCommunicationManager.instance.JoinMatchAsync(m_MatchId);
			LoadGame();
		}

		/// <summary>
		/// Deactivate join panel, activate create panel and vis-versa
		/// </summary>
		public void JoinCreate()
		{
			joinOrCreatePanel.SetActive(!joinOrCreatePanel.activeInHierarchy);
			createServerPanel.SetActive(!joinOrCreatePanel.activeInHierarchy);
		}

		public async void CreateServer()
		{
			var tsOk = int.TryParse(m_TerrainSize.text, out var ts);
			var iaOk = int.TryParse(m_InitialAnimals.text, out var ia);
			var ipOk = int.TryParse(m_InitialPlants.text, out var ip);
			Debug.Log($"Asking for server creation with config:\n" +
			          $"Terrain size: {ts}, " +
			          $"Initial animals: {ia}, " +
			          $"Initial plants: {ip}");
			var p = new CreateMatchRequest().ToByteString().ToStringUtf8();
			var protoResponse = await SessionManager.instance.socket.RpcAsync("create_match", p);
			var response = CreateMatchResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(protoResponse.Payload));
			Debug.Log($"CreateServer Response: {response}");
		}

		private async void StopMatch(string m)
		{
			var p = new StopMatchRequest()
			{
				MatchId = m
			}.ToByteString().ToStringUtf8();
			var protoResponse = await SessionManager.instance.socket.RpcAsync("stop_match", p);
			var response = StopMatchResponse.Parser.ParseFrom(Encoding.UTF8.GetBytes(protoResponse.Payload));
			Debug.Log($"StopServer response: {response}");
		}
	}
}
