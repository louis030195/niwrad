using System;
using Input;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace UI
{
    public class DisableUnitOnUiInteraction : MonoBehaviour
    {
        private UnitSelection _unitSelection;
        private EventSystem _eventSystem;
        private BaseInputModule _currentInputModule;
        private Rts _rtsControls;

        private void Awake()
        {
            _rtsControls = new Rts();
        }

        private void Start()
        {
            _unitSelection = GetComponentInParent<UnitSelection>(); // TODO: anything better?
            _eventSystem = GetComponent<EventSystem>();
            _currentInputModule = _eventSystem.currentInputModule;
        }
        
        private void OnEnable()
        {
            _rtsControls.Enable();
        }

        private void OnDisable()
        {
            _rtsControls.Disable();
        }


        private void Update()
        {
            _unitSelection.disable = _rtsControls.UI.Click.triggered; //_eventSystem.currentSelectedGameObject != null;
        }
    }
}
