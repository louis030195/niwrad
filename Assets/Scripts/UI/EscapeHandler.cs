using System;
using Gameplay;
using Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class EscapeHandler : MonoBehaviour
    {
        [SerializeField] private Menu escapeScrollView;
        [SerializeField] private Menu experienceMenu;
        [SerializeField] private Menu inputsMenu;
        [SerializeField] private Menu leaderboardMenu;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button inputsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button experienceButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button leaderboardButton;

        private Rts _rtsControls;
        private void Awake()
        {
            _rtsControls = new Rts();
            _rtsControls.Player.Cancel.started += ShowEscapeMenu;
        }
        
        private void OnEnable()
        {
            _rtsControls.Enable();
        }

        private void OnDisable()
        {
            _rtsControls.Disable();
        }

        private void Start()
        {
            settingsButton.onClick.AddListener(SubShow);
            resumeButton.onClick.AddListener(NiwradMenu.instance.PopAll);
            inputsButton.onClick.AddListener(inputsMenu.Push);
            experienceButton.onClick.AddListener(experienceMenu.Push);
            quitButton.onClick.AddListener(Quit);
            leaderboardButton.onClick.AddListener(leaderboardMenu.Push);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowEscapeMenu(InputAction.CallbackContext ctx)
        {
            if (Gm.instance.state == GameState.Menu) return;
            SubShow();
        }

        private void SubShow()
        {
            if (NiwradMenu.instance.IsEmpty())
            {
                settingsButton.gameObject.SetActive(false);
                NiwradMenu.instance.Push(escapeScrollView);
            }
            else
            {
                settingsButton.gameObject.SetActive(true);
                NiwradMenu.instance.PopAll();
            }
        }
    }
}
