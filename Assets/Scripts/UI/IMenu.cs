using System;

namespace UI
{
    /// <summary>
    /// Interface implemented by <see cref="Menu"/> />.
    /// Declares common menu methods and <see cref="IMenu.IsShown"/> property.
    /// </summary>
    public interface IMenu
    {
        /// <summary>
        /// If true, object implementing <see cref="IMenu"/> is currently shown.
        /// </summary>
        bool IsShown { get; }

        /// <summary>
        /// Shows this menu to the user.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides this menu from the user.
        /// </summary>
        void Hide();

        /// <summary>
        /// Sets the listener of this menu's back button.
        /// </summary>
        void SetBackButtonHandler(Action onBack);
    }
}
