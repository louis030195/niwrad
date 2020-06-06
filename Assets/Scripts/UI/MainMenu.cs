using System.Collections;
using Nakama;
using Net.Match;
using Net.Session;
using UnityEngine;

namespace UI
{
	public class MainMenu : MonoBehaviour
	{
		[SerializeField]
		private TMPro.TMP_InputField matchId;

		public async void ConnectAsync()
		{
			await SessionManager.instance.socket.JoinMatchAsync(matchId.text);
			StartCoroutine(LoadGame());
		}


		/// <summary>
		/// Starts the game scene and joins the match
		/// </summary>
		private static IEnumerator LoadGame()
		{
			var asyncLoad = UnityEngine.SceneManagement.SceneManager.
				LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Additive);

			while (!asyncLoad.isDone)
			{
				yield return null;
			}

			UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("MainMenu");
		}
	}
}
