using System;
using Gameplay;
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
        private Rts _rtsControls;

        private void Awake()
        {
            _rtsControls = new Rts();
        }

        private void Start()
        {
            _unitSelection = GetComponentInParent<UnitSelection>(); // TODO: anything better?
            Gm.instance.MenuStateStarted += () =>
            {
                _unitSelection.disable = true;
            };
            Gm.instance.ExperienceStateStarted += () =>
            {
                _unitSelection.disable = false;
            };
            Gm.instance.PlayStateStarted += () => _unitSelection.disable = false;
        }
        
        private void OnEnable()
        {
            _rtsControls.Enable();
        }

        private void OnDisable()
        {
            _rtsControls.Disable();
        }
    }
}
