using System;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DisableUnitOnUiInteraction : MonoBehaviour
    {
        [SerializeField] private UnitSelection unitSelection;
        private EventSystem _eventSystem;
        private BaseInputModule _currentInputModule;

        private void Start()
        {
            _eventSystem = GetComponent<EventSystem>();
            _currentInputModule = _eventSystem.currentInputModule;
        }

        private void Update()
        {
            unitSelection.disable = _eventSystem.currentSelectedGameObject != null;
        }
    }
}
