using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Manages visibility of the gameobject.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class Menu : MonoBehaviour, IMenu
    {
        #region Fields

        /// <summary>
        /// Reference to <see cref="CanvasGroup"/> used to show or hide the gameobject.
        /// </summary>
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// Button returning from this panel to main menu.
        /// </summary>
        [SerializeField] protected Button backButton;


        #endregion

        #region Properties

        /// <summary>
        /// If true, <see cref="Show"/> method was called and 
        /// this panel is visible to the viewer.
        /// </summary>
        public bool IsShown { get; protected set; }

        public bool isHud;

        #endregion
        
        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        #region Methods

        /// <summary>
        /// Makes this menu visible to the viewer.
        /// </summary>
        [ContextMenu("Show")]
        public virtual void Show()
        {
            Debug.Log($"Showing {gameObject.name}");
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            IsShown = true;
        }

        /// <summary>
        /// Hides this menu.
        /// </summary>
        [ContextMenu("Hide")]
        public virtual void Hide()
        {
            Debug.Log($"Hiding {gameObject.name}");
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            IsShown = false;
        }

        /// <summary>
        /// Sets the handler for <see cref="backButton"/>.
        /// </summary>
        public virtual void SetBackButtonHandler(Action onBack)
        {
            backButton.onClick.AddListener(() => onBack());
        }

        public virtual void Push()
        {
            Mm.instance.Push(this);
        }
        
        public virtual void Pop()
        {
            Mm.instance.PopTo(this);
        }

        #endregion

    }
}
