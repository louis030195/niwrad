using UnityEngine;

namespace UI
{
    public class EscapeMenu : Menu
    {
        [SerializeField]
        private Menu escapeScrollView;

        private void Update()
        {
            // When no menu is shown, escape show escape menu
            if (Input.GetButtonDown("Cancel") && Mm.instance.IsEmpty())
            {
                Push();
                Mm.instance.Push(escapeScrollView);
            }
            // When escape menu is shown and user press escape, dismiss everything
            else if (IsShown && Input.GetButtonDown("Cancel"))
            {
                Mm.instance.PopTo(this);
            }
        }
    }
}
