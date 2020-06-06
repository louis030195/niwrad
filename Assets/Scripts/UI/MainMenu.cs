using System;
using System.Collections;
using System.Threading.Tasks;
using Nakama;
using Net.Match;
using Net.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class MainMenu : MonoBehaviour
	{
		[Tooltip("Prefab containing a match UI block"), SerializeField] private GameObject matchPrefab;
		[SerializeField] private GameObject scrollViewContent;
		[SerializeField] private Button join;

		private string m_MatchId = string.Empty;

		private void Start()
		{
			Connect();
		}

		private void Update()
		{
			// Can't join until a match has been selected
			join.interactable = m_MatchId != string.Empty;
		}

		public async void JoinMatch()
		{
			await MatchCommunicationManager.instance.JoinMatchAsync(m_MatchId);
			StartCoroutine(LoadGame());
		}

		private async void Connect()
		{
			if (!await SessionManager.instance.ConnectSocketAsync())
			{
				Debug.LogError($"Failed to open socket");
				return;
			}
			// var res = await MatchCommunicationManager.instance.CreateMatchAsync();
			// Debug.Log($"Match created : {res}");
			// await Task.Delay(2000);
			while (true)
			{
				await RefreshList();
				await Task.Delay(2000);
			}
		}

		private async Task RefreshList()
		{
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
	}
}
