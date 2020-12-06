using System;
using Input;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils.Shapes;
using Cysharp.Threading.Tasks;

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
        public NiwradMenu niwradMenu;

        [SerializeField] private Menu mainMenu;
        [SerializeField] private Menu inputsMenu;
        [SerializeField] private Menu experiencesMenu;
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
            // mainMenu.VisibilityChanged += b =>
            // {
            //     if (b) ChangeState(State.MainMenu);
            // };
            void Im(bool b)
            {
                if (!b) return;
                ChangeState(State.InputMenu);
                inputsMenu.VisibilityChanged -= Im;
            } // Just once
            inputsMenu.VisibilityChanged += Im;
            
            void Em(bool b)
            {
                if (!b) return;
                ChangeState(State.ExperiencesMenu);
                experiencesMenu.VisibilityChanged -= Em;
            } // Just once
            experiencesMenu.VisibilityChanged += Em;
        }
        

        private void OnEnable()
        {
            _rtsControls.Enable();
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
                // case State.Wandering:
                //     if (_defaultHelpText == null)
                //         _defaultHelpText = KDefaultHelpTextFormat.Replace("{cancel}",
                //                 _rtsControls.Player.Cancel.GetBindingDisplayString())
                //             .Replace("{move}", _rtsControls.Player.Move.GetBindingDisplayString())
                //             .Replace("{movey}", _rtsControls.Player.MoveY.GetBindingDisplayString());
                //     niwradMenu.ShowToast(_defaultHelpText).Forget();
                //     break;
                // case State.MainMenu:
                //     niwradMenu.ShowToast(KMainMenuHelpTextFormat).Forget();
                //     break;
                case State.InputMenu:
                    niwradMenu.ShowToast(KInputsMenuHelpTextFormat).Forget();
                    break;
                case State.ExperiencesMenu:
                    niwradMenu.ShowToast(KExperiencesMenuHelpTextFormat).Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
