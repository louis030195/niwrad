using System.Linq;
using Input;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
// using UnityEngine.Rendering;
using Utils;

namespace Player {
// TODO: clean this up holy shit
	public class UnitSelection : MonoBehaviour {

		// public GameObject selectionCirclePrefab;
        private Rts _rtsControls;
		private Camera _cam;
        private bool _hit;
		private bool _isSelecting;
		private Vector3 _lastMousePosition;
        public bool disable; // Can disable, useful for example when interacting with UI

		private void Awake()
		{
			_cam = Camera.main;
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

		private void Update () {
			if (disable)
			{
				_isSelecting = false;
				return;
			}

            var mouse = Vector3.zero;
            bool clickPressed;
            bool clickReleased;
#if UNITY_STANDALONE || UNITY_EDITOR
            mouse = Mouse.current.position.ReadValue();
            clickPressed = Mouse.current.leftButton.wasPressedThisFrame;
            clickReleased = Mouse.current.leftButton.wasReleasedThisFrame;
#elif UNITY_IOS || UNITY_ANDROID
            enabled = false; // Disabled unit selection on mobile yet
            mouse = Touchscreen.current.position.ReadValue();
            clickPressed = Touchscreen.current.press.wasPressedThisFrame;
            clickReleased = Touchscreen.current.press.wasReleasedThisFrame;
#endif

			var ray = _cam.ScreenPointToRay(mouse);
			_hit = Physics.Raycast(ray, out var info, float.MaxValue);
            
			// If we press the left mouse button, save mouse location and begin selection
			if(clickPressed) // TODO: works cross platform ?
			{
				_isSelecting = true;
				_lastMousePosition = mouse;

				foreach (var selectableObject in FindObjectsOfType<SelectableUnit>())
				{
					selectableObject.information.SetActive(false);
					selectableObject.selectionCircle.SetActive(false);
				}
			}
			// If we let go of the left mouse button, end selection
			if (clickReleased) // TODO: works cross platform ?
			{
				_isSelecting = false;
			}

			// Highlight all objects within the selection box
			if (_isSelecting)
			{
				foreach (var selectableObject in FindObjectsOfType<SelectableUnit>())
				{
					if (IsWithinSelectionBounds(selectableObject.gameObject))
					{
						selectableObject.information.SetActive(true);
						selectableObject.selectionCircle.SetActive(true);

						// Grow the circle according to object collider, TODO: doesn't work
						var objectSize = selectableObject.GetComponent<Collider>().bounds.extents.x;
						selectableObject.selectionCircle.GetComponent<Projector>().orthographicSize = objectSize * 2;
					}
					else
					{
						selectableObject.information.SetActive(false);
						selectableObject.selectionCircle.SetActive(false);
					}
				}
			}
            
			if (clickReleased && _hit)
			{
				var selectableObject = info.collider.GetComponent<SelectableUnit>();
				// If the clicked object has something to show
                if (selectableObject == null) return;
                // Show its information
                selectableObject.information.SetActive(true);
                selectableObject.selectionCircle.SetActive(true);
            }

		}

		private void OnGUI()
        {
            if (!_isSelecting) return;
            // Create a rect from both mouse positions
                
            var rect = Draw.GetScreenRect( _lastMousePosition, Mouse.current.position.ReadValue() );
            Draw.DrawScreenRect( rect, new Color( 0f, 0f, 0f, 0.25f ) );
            Draw.DrawScreenRectBorder( rect, 2, Color.green );
        }

		private bool IsWithinSelectionBounds( GameObject go )
		{
			if (!_isSelecting ) return false;
			var viewportBounds = Draw.GetViewportBounds( _cam, _lastMousePosition, Mouse.current.position.ReadValue() );
			return viewportBounds.Contains( _cam.WorldToViewportPoint( go.transform.position ) );
		}
    }

}

