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
using Gameplay;

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

        [SerializeField] private Menu inputsMenu;
        [SerializeField] private Menu experiencesMenu;
        private Rts _rtsControls;
        private State _currentState;

        private const string KPlayHelpTextFormat = "Reach the settings to start a new experience, " +
                                                   "You can move around using {move}, " +
                                                   "You can go up and down using {movey}";
        private const string KInputsMenuHelpTextFormat = "Arrange the key map to your will ...";

        private const string KExperiencesMenuHelpTextFormat = "Select or create an experience and tweak " +
                                                              "its parameters to your will, don't abuse " +
                                                              "or your computer will have hard times !";

        private void Awake()
        {
            _rtsControls = new Rts();
            _currentState = State.Wandering;
            
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

            void OnPlayStateStarted()
            {
                niwradMenu.ShowNotification(KPlayHelpTextFormat
                    .Replace("{move}", _rtsControls.Player.Move.GetBindingDisplayString())
                    .Replace("{movey}", _rtsControls.Player.MoveY.GetBindingDisplayString()),
                    4);
                Gm.instance.PlayStateStarted -= OnPlayStateStarted;
            }

            Gm.instance.PlayStateStarted += OnPlayStateStarted;
            
            void OnExperienceStateStarted()
            {
                niwradMenu.ShowNotification("You can try to play with artificial selection or try to change time", 4);
                Gm.instance.ExperienceStateStarted -= OnExperienceStateStarted;
            }

            Gm.instance.ExperienceStateStarted += OnExperienceStateStarted;
        }


        private void OnEnable()
        {
            _rtsControls.Enable();
        }

        private void OnDisable()
        {
            _rtsControls.Disable();
        }

        private void ChangeState(State s)
        {
            _currentState = s;
            UpdateUIHints();
        }


        private void UpdateUIHints()
        {
            switch (_currentState)
            {
                case State.InputMenu:
                    niwradMenu.ShowNotification(KInputsMenuHelpTextFormat);
                    break;
                case State.ExperiencesMenu:
                    niwradMenu.ShowNotification(KExperiencesMenuHelpTextFormat);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
