using System;
using Player;
using UnityEngine;

namespace UI
{
    public class EscapeMenu : Menu
    {
        [SerializeField]
        private Menu escapeScrollView;

        private UnitSelection _unitSelection;
        private CameraController _cameraController;
        
        private void Start()
        {
            _unitSelection = GetComponentInParent<UnitSelection>();
            _cameraController = GetComponentInParent<CameraController>();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                EscapePlusOther();
            }
        }

        public void EscapePlusOther(Menu menu = null) // TODO: better name, less monolithic dependencies
        {
            if (!IsShown)
            {
                // Can't select anything while scrolling the menu
                _unitSelection.disable = true;
                _cameraController.disable = true;
                Push();
                Mm.instance.Push(menu ? menu : escapeScrollView);
            }
            else {
                Mm.instance.PopTo(this); // When the menu is shown, un-stack up to the "root" menu
                // Mm.instance.Pop(); // Is it interesting to pop current menu only ?
                _unitSelection.disable = false;
                _cameraController.disable = false;
            }
        }
    }
}
