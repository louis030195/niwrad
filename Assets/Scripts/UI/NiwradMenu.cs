using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Api.Session;
using Gameplay;
using Input;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class StackL<T> : Stack<T>
    {

        public event Action<T> OnPush;
        public event Action<T> OnPop;

        public new void Push(T item)
        {
            OnPush?.Invoke(item);
            base.Push(item);
        }
        
        public new T Pop()
        {
            var item = base.Pop();
            OnPop?.Invoke(item);
            return item;
        }
    }
    
	public class NiwradMenu : Singleton<NiwradMenu>
	{
        public UnitSelection unitSelection;
        public CameraController cameraController;
        public Menu hud;
        public Menu settings;

        [SerializeField] private TextMeshProUGUI toast;
        [SerializeField] private Graphic toastBackground;
        [SerializeField] private Menu background;

        [Header("First Menu")] 
        
        public TMP_InputField username;
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
        
        private Rts _rtsControls;
        private readonly StackL<Menu> _stack = new StackL<Menu>();

        
		private void Start()
		{
            
            settings.Hide();
            
			_serverIp = serverIpGameObject.GetComponent<TMP_InputField>();
			_serverPort = serverPortGameObject.GetComponent<TMP_InputField>();
            playButton.onClick.AddListener(Connect);
            debugToggle.onValueChanged.AddListener(Debug);
            username.text = PlayerPrefs.GetString("username");
            _serverIp.text = PlayerPrefs.GetString("serverIp");
			_serverPort.text = PlayerPrefs.GetString("serverPort");
            
            singlePlayerButton.onClick.AddListener(() =>
            {
                secondMenu.Hide();
                background.Hide();
                Gm.instance.state = GameState.Play;
                settings.Show();
            });
            multiplayerButton.onClick.AddListener(() => throw new NotImplementedException("Online mode in maintenance")); // TODO next
            
            _rtsControls = new Rts();
#if UNITY_STANDALONE
            // Lock cursor withing window in standalone
            Cursor.lockState = CursorLockMode.Confined;

            void ReLock(InputAction.CallbackContext _)
            {
                Cursor.lockState = CursorLockMode.Confined;
                ShowToast("Cursor locked again").Forget();
                _rtsControls.Player.Fire.performed -= ReLock;
            }
            // Can unlock cursor from the window in standalone by pressing escape
            _rtsControls.Player.DoubleCancel.performed += ctx =>
            {
                Cursor.lockState = CursorLockMode.None;
                ShowToast("Cursor unlocked from the window, click inside the window to re-lock the cursor").Forget();
                // Can re-lock cursor from the window in standalone by pressing escape
                _rtsControls.Player.Fire.performed += ReLock;
            };
#endif
#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
#endif
        }

        private async void Connect()
        {
            // TODO username validation
            var res = await Sm.instance.ConnectAsync(username.text != "" ? username.text : null);
            
            switch (res)
            {
                case AuthenticationResponse.Authenticated:
                    ShowToast($"Authenticated as {Sm.instance.Account.User.Username}").Forget();
                    break;
                case AuthenticationResponse.Error:
                    ShowToast("Failed to reach server, mode offline ...").Forget();
                    break;
                case AuthenticationResponse.NewAccountCreated:
                    ShowToast($"New account created as {Sm.instance.Account.User.Username}").Forget();
                    break;
                case AuthenticationResponse.UserInfoUpdated:
                    ShowToast($"User information updated, {Sm.instance.Account.User.Username}").Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            PlayerPrefs.SetString("username", username.text);
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
            toast.FadeInAndOut(true, 0.5f).Forget();
            toastBackground.FadeInAndOut(true, 0.5f).Forget();
            
            //Wait for the duration
            float counter = 0;
            while (counter < duration)
            {
                counter += Time.deltaTime;
                await UniTask.Yield();
            }
            
            //Fade out
            toast.FadeInAndOut(false, 0.5f).Forget();
            toastBackground.FadeInAndOut(false, 0.5f).Forget();
            
            toast.enabled = false;
            toast.color = originalColor;
        }
        

        
        private void OnEscapeMenu(bool push = false)
        {
            var isEmpty = IsEmpty();
            // Can't select anything while scrolling a menu
            unitSelection.disable = !isEmpty;
            cameraController.disable = !isEmpty;
            // Hide hud when showing any menu (ignore if no experience is set)
            if (Gm.instance.Experience != null) EnableHud(isEmpty && !push);
        }

        public void Push(Menu menu)
        {
            OnEscapeMenu(true);
            if (_stack.Count > 0) _stack.Peek().Hide(); // TODO: By default hide current but maybe in some cases could want to literally stack UIs ?
            _stack.Push(menu);
            menu.Show();
        }

        public Menu Pop()
        {
            var ret = _stack.Pop();
            ret.Hide();
            if (_stack.Count > 0) _stack.Peek().Show();
            OnEscapeMenu();
            return ret;
        }

        /// <summary>
        /// Pop all elements until reaching the given one which is included
        /// If given an in-existing menu, will pop all the stack
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public List<Menu> PopTo(Menu menu)
        {
            var ret = new List<Menu>();
            while (_stack.Count > 0)
            {
                ret.Add(Pop());
                
                // Break the loop once we've popped up to the given menu
                if (ret.Last().Equals(menu)) break;
            }
            return ret;
        }

        public void PopAll()
        {
            while(_stack.Count > 0) Pop();
            settings.gameObject.SetActive(true);
        }

        public bool IsEmpty()
        {
            return _stack.Count == 0;
        }

        /// <summary>
        /// Show or hide all hud menus
        /// </summary>
        /// <param name="enable"></param>
        public void EnableHud(bool enable)
        {
            if (enable)
            {
                PopAll();
                hud.Show();
            }
            else
            {
                hud.Hide();
            }
        }
	}
}
