using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Api.Session;
using Gameplay;
using Input;
using Lean.Gui;
using Lean.Transition.Method;
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
        public LeanButton playButton;
        public LeanPulse notification;
        public Toggle debugToggle;
        public LeanToggle developmentToggle;
        [SerializeField] private GameObject serverIpGameObject;
        [SerializeField] private GameObject serverPortGameObject; // TODO: unused yet, who cares ?
        private TMP_InputField _serverIp;
        private TMP_InputField _serverPort;

        [Header("Second Menu")] public Menu secondMenu;
        public LeanButton singlePlayerButton;
        public LeanButton multiplayerButton;

        private Rts _rtsControls;
        private readonly StackL<Menu> _stack = new StackL<Menu>();
        private LeanJoinDelay _notificationJoinDelay;
        private TextMeshProUGUI _notificationText;

        private void Start()
        {
            Gm.instance.State = GameState.Menu;
            _notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
            _notificationJoinDelay = notification.GetComponentInChildren<LeanJoinDelay>();
            _serverIp = serverIpGameObject.GetComponent<TMP_InputField>();
            _serverPort = serverPortGameObject.GetComponent<TMP_InputField>();
            playButton.OnClick.AddListener(() => Connect(new InputAction.CallbackContext()));
            _rtsControls = new Rts();
            _rtsControls.Enable();
            _rtsControls.UI.Submit.performed += Connect;
            debugToggle.onValueChanged.AddListener(Advanced);
            developmentToggle.On = bool.Parse(PlayerPrefs.GetString("development", "false"));
            developmentToggle.OnOff.AddListener(() =>
            {
                PlayerPrefs.SetString("serverPort", "30020");
            });
            developmentToggle.OnOn.AddListener(() =>
            {
                PlayerPrefs.SetString("serverPort", "30021");
            });
            username.text = PlayerPrefs.GetString("username");
            // _serverIp.text = PlayerPrefs.GetString("serverIp");
            // _serverPort.text = PlayerPrefs.GetString("serverPort");

            singlePlayerButton.OnClick.AddListener(() =>
            {
                secondMenu.Hide();
                background.Hide();
                Gm.instance.State = GameState.Play;
                settings.Show();
            });
            multiplayerButton.OnClick.AddListener(() =>
                throw new NotImplementedException("Online mode in maintenance")); // TODO next
            
            // Pointless for mobile
#if UNITY_STANDALONE || UNITY_EDITOR
            // Lock cursor withing window in standalone
            Cursor.lockState = CursorLockMode.Confined;

            void ReLock(InputAction.CallbackContext _)
            {
                Screen.fullScreen = true;
                Cursor.lockState = CursorLockMode.Confined;
                ShowNotification("Cursor locked again");
                _rtsControls.Player.Fire.performed -= ReLock;
            }

            // Basically copy paste of how Google Stadia handle full screen :D
            _rtsControls.Player.DoubleCancel.performed += ctx =>
            {
                ShowNotification($"Press and hold {_rtsControls.Player.LongCancel.activeControl.displayName}" +
                                 $" to unlock the cursor from the window");
            };
            // Can unlock cursor from the window in standalone by pressing escape
            _rtsControls.Player.LongCancel.performed += ctx =>
            {
                Screen.fullScreen = false;
                Cursor.lockState = CursorLockMode.None;
                ShowNotification("Cursor unlocked from the window," +
                                 " click inside the window to re-lock the cursor");
                // Can re-lock cursor from the window in standalone by pressing escape
                _rtsControls.Player.Fire.performed += ReLock;
            };
#elif UNITY_IOS || UNITY_ANDROID
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
#endif


            Sm.instance.ConnectionSucceed += OnConnectionSucceed;
        }

        private void OnConnectionSucceed()
        {
            PlayerPrefs.SetString("username", username.text);
            PlayerPrefs.SetString("development", developmentToggle.On.ToString());
            // PlayerPrefs.SetString("serverIp", _serverIp.text);
            // PlayerPrefs.SetString("serverPort", _serverPort.text);
            PlayerPrefs.Save();

            // Next menu in any case, if Nakama auth failed => auth-less mode
            firstMenu.Hide();
            secondMenu.Show();

            // Unbind Submit from connect then
            // _submitAction.started -= Connect;
            _rtsControls.UI.Submit.performed -= Connect;

            // Just once (imagine "failed to reach server, going offline then at some point connect during the game")
            Sm.instance.ConnectionSucceed -= OnConnectionSucceed;
        }

        private async void Connect(InputAction.CallbackContext _)
        {
            // TODO username validation
            var res = await Sm.instance.ConnectAsync(username.text != "" ? username.text : null);

            switch (res)
            {
                case AuthenticationResponse.Authenticated:
                    ShowNotification($"Authenticated as {Sm.instance.Account.User.Username}");
                    break;
                case AuthenticationResponse.ErrorInternal:
                    ShowNotification("Failed to reach server, mode offline ...");
                    // Go offline if internal server error
                    OnConnectionSucceed();
                    break;
                case AuthenticationResponse.ErrorUsernameAlreadyExists:
                    ShowNotification("Username already taken !", 3);
                    break;
                case AuthenticationResponse.NewAccountCreated:
                    ShowNotification($"New account created as {Sm.instance.Account.User.Username}");
                    break;
                case AuthenticationResponse.UserInfoUpdated:
                    ShowNotification($"User information updated, {Sm.instance.Account.User.Username}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Advanced(bool value)
        {
            serverIpGameObject.SetActive(value);
            serverPortGameObject.SetActive(value);
        }

        public void ShowNotification(string text, int duration = 2) // TODO: !!!! dismiss swipe / click
        {
            _notificationText.text = text;
            _notificationJoinDelay.Duration = duration;
            notification.Pulse();
        }

        /**
         * Kinda deprecated see -> ShowNotification
         */
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
            if (_stack.Count > 0)
                _stack.Peek()
                    .Hide(); // TODO: By default hide current but maybe in some cases could want to literally stack UIs ?
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
            while (_stack.Count > 0) Pop();
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
