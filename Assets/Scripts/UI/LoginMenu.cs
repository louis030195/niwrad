using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Api.Session;
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
		private TMP_InputField m_ServerPort;
		private void Start()
		{
			m_ServerIp = serverIpGameObject.GetComponent<TMP_InputField>();
			m_ServerPort = serverPortGameObject.GetComponent<TMP_InputField>();
			username.text = PlayerPrefs.GetString("username");
			password.text = PlayerPrefs.GetString("password");
			m_ServerIp.text = PlayerPrefs.GetString("serverIp");
			m_ServerPort.text = PlayerPrefs.GetString("serverPort");
		}

		private async void Connect(string u, string p, string ip, int port, bool create = false)
		{
			var (success, message) = await SessionManager.instance
				.ConnectAsync(u, p, create, ip, port);
			response.text = message;
			await ClearResponse();
			if (success) LoadMainMenu();
		}

		private async UniTask ClearResponse()
		{
			await UniTask.Delay(5000);
			response.text = $"";
		}

		/// <summary>
		/// Starts the game scene and joins the main menu
		/// </summary>
		private async void LoadMainMenu()
		{
			PlayerPrefs.SetString("username", username.text);
			PlayerPrefs.SetString("password", password.text); // TODO: "save password" checkbox
			PlayerPrefs.SetString("serverIp", m_ServerIp.text);
			PlayerPrefs.SetString("serverPort", m_ServerPort.text);
			PlayerPrefs.Save();
			// Wait a few second to let the user see authentication result
			await UniTask.Delay(500);
			await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("SecondMenu");
		}

		public void Login()
		{
			Connect(username.text, password.text, m_ServerIp.text, int.Parse(m_ServerPort.text));
		}

		public void Register()
		{
			Connect(username.text, password.text, m_ServerIp.text, int.Parse(m_ServerPort.text), true);
		}

		public void Debug(bool value)
		{
			serverIpGameObject.SetActive(value);
			serverPortGameObject.SetActive(value);
		}
	}
}
