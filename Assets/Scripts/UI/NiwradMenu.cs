using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Api.Session;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class NiwradMenu : Menu
	{
        [SerializeField] private TextMeshProUGUI toast;
        [SerializeField] private Graphic toastBackground;
        [SerializeField] private Menu background;
		
        [Header("First Menu")]

        public Menu firstMenu;
        public Button playButton;
        public Toggle debugToggle;
        [SerializeField] private GameObject serverIpGameObject;
		[SerializeField] private GameObject serverPortGameObject; // TODO: unused yet, who cares ?
        private TMP_InputField _serverIp;
		private TMP_InputField _serverPort;
        
        [Header("Second Menu")]

        public Menu secondMenu;
        public Button singlePlayerButton;
        public Button multiplayerButton;

        
		protected override void Start()
		{
            base.Start();
            
            Mm.instance.settings.Hide();
            
			_serverIp = serverIpGameObject.GetComponent<TMP_InputField>();
			_serverPort = serverPortGameObject.GetComponent<TMP_InputField>();
            playButton.onClick.AddListener(Connect);
            debugToggle.onValueChanged.AddListener(Debug);
            _serverIp.text = PlayerPrefs.GetString("serverIp");
			_serverPort.text = PlayerPrefs.GetString("serverPort");
            
            singlePlayerButton.onClick.AddListener(() =>
            {
                secondMenu.Hide();
                background.Hide();
                Gm.instance.State = GameState.Play;
                Mm.instance.settings.Show();
            });
            multiplayerButton.onClick.AddListener(() => throw new NotImplementedException("Online mode in maintenance")); // TODO next
        }

        private async void Connect()
        {
            var res = await Sm.instance.ConnectAsync();
            
            switch (res)
            {
                case AuthenticationResponse.Authenticated:
                    ShowToast("Authenticated").Forget();
                    break;
                case AuthenticationResponse.Error:
                    ShowToast("Failed to reach server, mode offline ...").Forget();
                    break;
                case AuthenticationResponse.NewAccountCreated:
                    ShowToast("NewAccountCreated").Forget();
                    break;
                case AuthenticationResponse.UserInfoUpdated:
                    ShowToast("UserInfoUpdated").Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Save IP:PORT (dev stuff)
            PlayerPrefs.SetString("serverIp", _serverIp.text);
            PlayerPrefs.SetString("serverPort", _serverPort.text);
            PlayerPrefs.Save();
            
            // Next menu in any case, if Nakama auth failed => auth-less mode
            firstMenu.Hide();
            secondMenu.Show();
        }
        
        private void Debug(bool value)
		{
			serverIpGameObject.SetActive(value);
			serverPortGameObject.SetActive(value);
		}
        
        public async UniTaskVoid ShowToast(string text, int duration = 2)
        {
            var originalColor = toast.color;
            
            toast.text = text;
            toast.enabled = true;
            
            //Fade in
            FadeInAndOut(toast, true, 0.5f).Forget();
            FadeInAndOut(toastBackground, true, 0.5f).Forget();
            
            //Wait for the duration
            float counter = 0;
            while (counter < duration)
            {
                counter += Time.deltaTime;
                await UniTask.Yield();
            }
            
            //Fade out
            FadeInAndOut(toast, false, 0.5f).Forget();
            FadeInAndOut(toastBackground, false, 0.5f).Forget();
            
            toast.enabled = false;
            toast.color = originalColor;
        }
        
        private async UniTaskVoid FadeInAndOut(Graphic target, bool fadeIn, float duration)
        {
            //Set Values depending on if fadeIn or fadeOut
            float a, b;
            if (fadeIn)
            {
                a = 0f;
                b = 1f;
            }
            else
            {
                a = 1f;
                b = 0f;
            }

            var currentColor = target.color;
            var counter = 0f;

            while (counter < duration)
            {
                counter += Time.deltaTime;
                var alpha = Mathf.Lerp(a, b, counter / duration);

                target.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                await UniTask.Yield();
            }
        }
	}
}
