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
#if UNITY_STANDALONE
			// Lock cursor withing window in standalone
			Cursor.lockState = CursorLockMode.Confined;
            // Can unlock cursor from the window in standalone by pressing escape
            _rtsControls.UI.Cancel.performed += ctx => Cursor.lockState = CursorLockMode.None;
#endif
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
                // print("disable !");
				_isSelecting = false;
				return;
			}

            var mouse = Mouse.current.position.ReadValue();
			var ray = _cam.ScreenPointToRay(mouse);
			_hit = Physics.Raycast(ray, out var info, float.MaxValue);


			// If we press the left mouse button, save mouse location and begin selection
			if(Mouse.current.leftButton.wasPressedThisFrame) // TODO: works cross platform ?
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
			if (Mouse.current.leftButton.wasReleasedThisFrame) // TODO: works cross platform ?
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
            
			if (Mouse.current.leftButton.wasReleasedThisFrame && _hit)
			{
				var selectableObject = info.collider.GetComponent<SelectableUnit>();
				// If the clicked object has something to show
				if (selectableObject != null)
				{
					// Show its information
					selectableObject.information.SetActive(true);
					selectableObject.selectionCircle.SetActive(true);
				}
			}

		}

		private void OnGUI()
        {
            if (!_isSelecting) return;
            // Create a rect from both mouse positions
                
            var rect = Draw.GetScreenRect( _lastMousePosition, Mouse.current.position.ReadValue() );
            Draw.DrawScreenRect( rect, new Color( 0.8f, 0.8f, 0.95f, 0.25f ) );
            Draw.DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
        }

		private bool IsWithinSelectionBounds( GameObject go )
		{
			if( !_isSelecting ) return false;
			var viewportBounds = Draw.GetViewportBounds( _cam, _lastMousePosition, Mouse.current.position.ReadValue() );
			return viewportBounds.Contains( _cam.WorldToViewportPoint( go.transform.position ) );
		}
    }

}

