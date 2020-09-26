using System;
using Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class EscapeHandler : MonoBehaviour
    {
        [SerializeField] private Menu escapeScrollView;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button ecapeButton;

        private Rts _rtsControls;
        private void Awake()
        {
            _rtsControls = new Rts();
            _rtsControls.Player.Cancel.started += ShowEscapeMenu;
            ecapeButton.onClick.AddListener(SubShow);
// #if UNITY_IOS || UNITY_ANDROID
// #endif

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
            resumeButton.onClick.AddListener(() => Mm.instance.PopAll());
            quitButton.onClick.AddListener(Quit);
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
            SubShow();
        }

        private void SubShow()
        {
            if (Mm.instance.IsEmpty()) Mm.instance.Push(escapeScrollView);
            else Mm.instance.PopAll();
        }
    }
}
