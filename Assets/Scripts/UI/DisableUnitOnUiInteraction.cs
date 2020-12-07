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
            // FIX
            // _rtsControls.UI.Click.started += _ =>
            // {
            //     _unitSelection.disable = true;
            //     Debug.Log("started");
            // };
            // _rtsControls.UI.Click.performed += _ =>
            // {
            //     _unitSelection.disable = true;
            //     Debug.Log("performed");
            //
            // };
            // _rtsControls.UI.Click.canceled += _ =>
            // {
            //     _unitSelection.disable = false;
            //     Debug.Log("canceled");
            // };
            // _eventSystem.currentSelectedGameObject
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
            // TODO: instead remove eventtrigger & use new IS for d&d
            _unitSelection.disable = _eventSystem.currentSelectedGameObject != null || _rtsControls.UI.Click.triggered; //
        }
    }
}
