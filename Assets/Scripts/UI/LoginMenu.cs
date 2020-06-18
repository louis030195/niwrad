using System;
using System.Collections;
using Net.Session;
using TMPro;
using UnityEngine;

namespace UI
{
	public class LoginMenu : MonoBehaviour
	{
		[SerializeField] private TMP_InputField username;
		[SerializeField] private TMP_InputField password;
		[SerializeField] private TextMeshProUGUI response;
		[SerializeField] private GameObject serverIpGameObject;


		private TMP_InputField m_ServerIp;
		private void Start()
		{
			m_ServerIp = serverIpGameObject.GetComponent<TMP_InputField>();
			if (SessionManager.instance.debug) Connect("aaaa@aaaa.com", "aaaaaaaa");
		}

		private async void Connect(string u, string p, bool create = false)
		{
			var result = await SessionManager.instance.ConnectAsync(u, p, create, m_ServerIp.text);
			response.text = result.message;
			StartCoroutine(ClearResponse());
			if (result.success) StartCoroutine(LoadMainMenu());
		}

		private IEnumerator ClearResponse()
		{
			yield return new WaitForSeconds(5f);
			response.text = $"";
		}

		/// <summary>
		/// Starts the game scene and joins the main menu
		/// </summary>
		private static IEnumerator LoadMainMenu()
		{
			// Wait a few second to let the user see authentication result
			yield return new WaitForSeconds(2f);

			var asyncLoad = UnityEngine.SceneManagement.SceneManager.
				LoadSceneAsync("SecondMenu", UnityEngine.SceneManagement.LoadSceneMode.Additive);

			while (!asyncLoad.isDone)
			{
				yield return null;
			}

			UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("LoginMenu");
		}

		public void Login()
		{
			Connect(username.text, password.text);
		}

		public void Register()
		{
			Connect(username.text, password.text, true);
		}

		public void Debug(bool value)
		{
			serverIpGameObject.SetActive(value);
		}
	}
}
