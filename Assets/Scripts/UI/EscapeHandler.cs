using System;
using Gameplay;
using Input;
using Lean.Gui;
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
        [SerializeField] private Menu audioMenu;
        [SerializeField] private Menu leaderboardMenu;
        
        // Choices you see when reaching escape menu
        [SerializeField] private LeanButton resumeButton;
        [SerializeField] private LeanButton inputsButton;
        [SerializeField] private LeanButton audioButton;
        [SerializeField] private LeanButton experienceButton;
        [SerializeField] private LeanButton quitButton;
        
        // Stuff in corners
        [SerializeField] private LeanButton leaderboardButton;
        [SerializeField] private LeanButton settingsButton;

        private Rts _rtsControls;
        private Menu _settingsButtonMenu;
        
        private void Awake()
        {
            _rtsControls = new Rts();
            Gm.instance.PlayStateStarted += () => _rtsControls.Player.Cancel.started += _ => SubShow();
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
            resumeButton.OnClick.AddListener(() =>
            {
                Gm.instance.State = GameState.Play;
                NiwradMenu.instance.PopAll();
            });
            inputsButton.OnClick.AddListener(inputsMenu.Push);
            audioButton.OnClick.AddListener(audioMenu.Push);
            experienceButton.OnClick.AddListener(experienceMenu.Push);
            quitButton.OnClick.AddListener(Quit);
            
            settingsButton.OnClick.AddListener(SubShow);
            leaderboardButton.OnClick.AddListener(leaderboardMenu.Push);
            _settingsButtonMenu = settingsButton.GetComponent<Menu>();
            Gm.instance.ExperienceStateStarted += _settingsButtonMenu.Show;
            Gm.instance.PlayStateStarted += _settingsButtonMenu.Show;
            Gm.instance.MenuStateStarted += _settingsButtonMenu.Hide;
        }

        private void Quit()
        {
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(2);
#endif
        }

        private void SubShow()
        {
            if (NiwradMenu.instance.IsEmpty())
            {
                Gm.instance.State = GameState.Menu;
                NiwradMenu.instance.Push(escapeScrollView);
            }
            else
            {
                Gm.instance.State = GameState.Play;
                NiwradMenu.instance.PopAll();
            }
        }
    }
}
