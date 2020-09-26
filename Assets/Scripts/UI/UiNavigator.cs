using System;
using Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class UiNavigator : MonoBehaviour
	{
		private EventSystem m_System;
        private Rts _rtsControls;

        private void Awake()
        {
            m_System = EventSystem.current;// EventSystemManager.currentSystem;
            _rtsControls = new Rts();
        }

        private void OnEnable()
        {
            _rtsControls.Enable();
        }

        private void OnDisable()
        {
            _rtsControls.Disable();
        }
        
		private void Start()
		{

            // TODO: test that not sure it works
            _rtsControls.UI.Navigate.performed += ctx =>
            {
                var next = m_System.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                if (next != null)
                {
                    var inputField = next.GetComponent<TMP_InputField>();
                    if (inputField != null)
                        inputField.OnPointerClick(new PointerEventData(m_System));  //if it's an input field, also set the text caret

                    m_System.SetSelectedGameObject(next.gameObject, new BaseEventData(m_System));
                }
                else Debug.Log($"next navigation element not found");
            };
        }
    }
}
