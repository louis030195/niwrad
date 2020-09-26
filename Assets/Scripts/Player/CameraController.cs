using System;
using Input;
using Player.Joysticks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(UnitSelection))]
	public class CameraController : MonoBehaviour
    {
        [HideInInspector] public bool disable;
		[SerializeField] private float movementSpeed = 5f;
		[SerializeField] private float zoomSpeed = 10f;
		[SerializeField] private float rotationSpeed = 0.1f;
        [SerializeField] private GameObject moveJoystick;
        [SerializeField] private GameObject rotateJoystick;
        [SerializeField] private GameObject flyJoystick;
        private UnitSelection _unitSelection;
        private FixedJoystick _moveJoystick;
        private FixedJoystick _rotateJoystick;
        private FixedJoystick _flyJoystick;
		private float _eulerX;
		private float _eulerY;
        private Rts _rtsControls;
        private bool _mobile;
        private void Awake()
        {
            _moveJoystick = moveJoystick.GetComponent<FixedJoystick>();
            _rotateJoystick = rotateJoystick.GetComponent<FixedJoystick>();
            _flyJoystick = flyJoystick.GetComponent<FixedJoystick>();
            moveJoystick.SetActive(false);
            rotateJoystick.SetActive(false);
            flyJoystick.SetActive(false);
#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
            moveJoystick.SetActive(true);
            rotateJoystick.SetActive(true);
            flyJoystick.SetActive(true);
            rotationSpeed *= 10; // Somehow slower on mobile
            _mobile = true;
#endif
            _unitSelection = GetComponent<UnitSelection>();
            _rtsControls = new Rts();
            // Skip PC inputs if it's mobile
            if (_mobile) return;
            _rtsControls.Player.MoveY.performed += ctx =>
            {
                if (!disable)
                    transform.position += Vector3.up * (ctx.ReadValue<Vector2>().y * zoomSpeed);
            };
            
        }

        private void OnEnable()
        {
            _rtsControls.Enable();
        }

        private void OnDisable()
        {
            _rtsControls.Disable();
        }


        private void Update ()
        {
            if (disable) return;
            var t = transform;
            // Disable unit selection when using joysticks on mobile
            _unitSelection.disable = _mobile && _moveJoystick.isDragging || 
                                     _mobile && _rotateJoystick.isDragging ||
                                    _mobile && _flyJoystick.isDragging;
            if (_mobile) transform.position += Vector3.up * (_flyJoystick.Vertical * zoomSpeed);
            //Get Forward face
            var dir = t.forward;
            //Reset/Ignore y axis
            dir.y = 0;
            dir.Normalize();
            // Move position with arrows around
            var move = _mobile ? _moveJoystick.Direction : _rtsControls.Player.Move.ReadValue<Vector2>();
            t.position += (t.right * move.x + dir * move.y) * movementSpeed;
            
            // Rotation on Y axis
            if (Mouse.current.rightButton.isPressed || _mobile && _rotateJoystick.isDragging) // TODO: check works cross platform
            {
                var look = _mobile ? _rotateJoystick.Horizontal : _rtsControls.Player.Look.ReadValue<Vector2>().x;
                t.Rotate(new Vector3(0, look * rotationSpeed, 0));
                _eulerX = t.rotation.eulerAngles.x;
                _eulerY = t.rotation.eulerAngles.y;
                t.rotation = Quaternion.Euler(_eulerX, _eulerY, 0);
            };
			// Rotate camera on X axis
			// if(Input.GetMouseButton(1) && Input.GetMouseButton(2)) {
			// 	t.Rotate(-new Vector3(Input.GetAxis("Mouse Y") * rotationSpeed, 0, 0));
			// 	_eulerX = t.rotation.eulerAngles.x;
			// 	_eulerY = t.rotation.eulerAngles.y;
			// 	t.rotation = Quaternion.Euler(_eulerX, _eulerY, 0);
			// }
		}
    }
}
