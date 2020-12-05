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
        /// Reference to <see cref="UnityEngine.CanvasGroup"/> used to show or hide the gameobject.
        /// </summary>
        private CanvasGroup _canvasGroup;

        [SerializeField] private Button backButton;

        #endregion

        #region Properties

        /// <summary>
        /// If true, <see cref="Show"/> method was called and 
        /// this panel is visible to the viewer.
        /// </summary>
        public bool IsShown { get; protected set; }

        public event Action<bool> VisibilityChanged;

        #endregion

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Start()
        {
            if (backButton != null) backButton.onClick.AddListener(Pop); // TODO: Should trigger before all UI-set actions
        }

        protected virtual void OnDestroy()
        {
            try
            {
                Pop();
            }
            catch
            {
                // ignored
            }
        }

        #region Methods

        /// <summary>
        /// Makes this menu visible to the viewer.
        /// </summary>
        [ContextMenu("Show")]
        public virtual void Show()
        {
            // Debug.Log($"Showing {gameObject.name}");
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            IsShown = true;
            VisibilityChanged?.Invoke(true);
        }

        /// <summary>
        /// Hides this menu.
        /// </summary>
        [ContextMenu("Hide")]
        public virtual void Hide()
        {
            // Debug.Log($"Hiding {gameObject.name}");
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            IsShown = false;
            VisibilityChanged?.Invoke(false);
        }

        public virtual void Push()
        {
            Mm.instance.Push(this);
        }
        
        public virtual void Pop()
        {
            Mm.instance.Pop();
        }

        #endregion

    }
}
