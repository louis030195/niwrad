using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EscapeHandler : MonoBehaviour
    {
        [SerializeField] private Menu escapeScrollView;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            resumeButton.onClick.AddListener(() => Mm.instance.PopAll());
            quitButton.onClick.AddListener(Quit);
        }

        private void Update()
        {
            // When no menu is shown, escape show escape menu
            if (Input.GetButtonDown("Cancel") && Mm.instance.IsEmpty())
            {
                Mm.instance.Push(escapeScrollView);
            }
            // When escape menu is shown and user press escape, dismiss everything
            else if (Input.GetButtonDown("Cancel"))
            {
                Mm.instance.PopAll();
            }
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
    }
}
