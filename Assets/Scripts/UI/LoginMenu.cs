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
		[SerializeField] private GameObject serverPortGameObject; // TODO: unused yet, who cares ?


		private TMP_InputField m_ServerIp;
		private void Start()
		{
			m_ServerIp = serverIpGameObject.GetComponent<TMP_InputField>();
			username.text = PlayerPrefs.GetString("username");
			password.text = PlayerPrefs.GetString("password");
			m_ServerIp.text = PlayerPrefs.GetString("serverIp");
		}

		private async void Connect(string u, string p, string ip, bool create = false)
		{
			var (success, message) = await SessionManager.instance
				.ConnectAsync(u, p, create, ip);
			response.text = message;
			StartCoroutine(ClearResponse());
			if (success) StartCoroutine(LoadMainMenu());
		}

		private IEnumerator ClearResponse()
		{
			yield return new WaitForSeconds(5f);
			response.text = $"";
		}

		/// <summary>
		/// Starts the game scene and joins the main menu
		/// </summary>
		private IEnumerator LoadMainMenu()
		{
			PlayerPrefs.SetString("username", username.text);
			PlayerPrefs.SetString("password", password.text); // TODO: "save password" checkbox
			PlayerPrefs.SetString("serverIp", m_ServerIp.text);
			PlayerPrefs.Save();
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
			Connect(username.text, password.text, m_ServerIp.text);
		}

		public void Register()
		{
			Connect(username.text, password.text, m_ServerIp.text, true);
		}

		public void Debug(bool value)
		{
			serverIpGameObject.SetActive(value);
			serverPortGameObject.SetActive(value);
		}
	}
}
