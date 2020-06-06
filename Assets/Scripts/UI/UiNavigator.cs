using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class UiNavigator : MonoBehaviour
	{
		private EventSystem m_System;

		private void Start()
		{
			m_System = EventSystem.current;// EventSystemManager.currentSystem;

		}
		// Update is called once per frame
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
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

			}
		}
	}
}
