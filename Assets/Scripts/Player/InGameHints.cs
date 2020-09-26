using System;
using Input;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Player
{
    internal enum State
    {
        Wandering,
        MainMenu,
        InputMenu,
        ExperiencesMenu
    }
    public class InGameHints : MonoBehaviour
    {
        [SerializeField] private Menu mainMenu;
        [SerializeField] private Menu inputsMenu;
        [SerializeField] private Menu experiencesMenu;
        [SerializeField] private Button closeHintsButton;
        [SerializeField] private Menu hintsBox;
        public TextMeshProUGUI helpText;
        private Rts _rtsControls;

        private State _currentState;
        private string _defaultHelpText;

        private const string KDefaultHelpTextFormat = "Press {cancel} to start a new experience\n" +
                                                      "You can move around using {move}\n " +
                                                      "You can go up and down using {movey}";
        private const string KMainMenuHelpTextFormat = "Open experiences menu to start experimenting things !";
        private const string KInputsMenuHelpTextFormat = "Arrange the key map to your will ...";
        private const string KExperiencesMenuHelpTextFormat = "Select or create an experience and tweak " +
                                                              "its parameters to your will, don't abuse " +
                                                              "or your computer will have hard times !";

        private void Awake()
        {
            // TODO: hint for drag & dropping animals ...
            // TODO: inside dialog box that can be closed permanently ...
            _rtsControls = new Rts();
            // Hints can be disabled
            // TODO: settings ..
            // TODO: yet always on
            /*if (PlayerPrefs.HasKey("hints") && PlayerPrefs.GetInt("hints") == 0) hintsBox.Hide();
            else*/ _currentState = State.Wandering;
            mainMenu.VisibilityChanged += b =>
            {
                if (b) ChangeState(State.MainMenu);
            };
            inputsMenu.VisibilityChanged += b =>
            {
                if (b) ChangeState(State.InputMenu);
            };
            experiencesMenu.VisibilityChanged += b =>
            {
                if (b) ChangeState(State.ExperiencesMenu);
            };
            closeHintsButton.onClick.AddListener(() =>
            {
                hintsBox.Hide();
                PlayerPrefs.SetInt("hints", 0); // Disabled hints for this player
            });
        }

        private void OnEnable()
        {
            _rtsControls.Enable();
            UpdateUIHints();
        }

        private void OnDisable()
        {
            _rtsControls.Disable();
        }
        
        // This is invoked by PlayerInput when the controls on the player change. If the player switches control
        // schemes or keyboard layouts, we end up here and re-generate our hints.
        public void OnControlsChanged()
        {
            UpdateUIHints(true); // Force re-generation of our cached text strings to pick up new bindings.
        }
        
        private void ChangeState(State s)
        {
            _currentState = s;
            UpdateUIHints();
        }
        

        private void UpdateUIHints(bool regenerate = false)
        {
            if (regenerate)
            {
                _defaultHelpText = default;
            }

            switch (_currentState)
            {
                case State.Wandering:
                    if (_defaultHelpText == null)
                        _defaultHelpText = KDefaultHelpTextFormat.Replace("{cancel}",
                                _rtsControls.Player.Cancel.GetBindingDisplayString())
                            .Replace("{move}", _rtsControls.Player.Move.GetBindingDisplayString())
                            .Replace("{movey}", _rtsControls.Player.MoveY.GetBindingDisplayString());
                    helpText.text = _defaultHelpText;
                    break;
                case State.MainMenu:
                    helpText.text = KMainMenuHelpTextFormat;
                    break;
                case State.InputMenu:
                    helpText.text = KInputsMenuHelpTextFormat;
                    break;
                case State.ExperiencesMenu:
                    helpText.text = KExperiencesMenuHelpTextFormat;
                    break;
            }
        }
    }
}
