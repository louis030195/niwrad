using UnityEngine;

namespace Player
{
	public class CameraController : MonoBehaviour {

		[SerializeField] private float movementSpeed = 5f;
		[SerializeField] private float zoomSpeed = 100f;
		[SerializeField] private float rotationSpeed = 3.0f;

		private float m_EulerX;
		private float m_EulerY;

		private void Update () {
			// Zoom / de-zoom
			transform.position += Vector3.up * (-Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);

			//Get Forward face
			Vector3 dir = transform.forward;
			//Reset/Ignore y axis
			dir.y = 0;
			dir.Normalize();

			// Move position with arrows around
			transform.position += (transform.right * Input.GetAxis("Horizontal") + dir * Input.GetAxis("Vertical")) * movementSpeed;

			// Rotate camera on Y axis
			if(Input.GetMouseButton(1) && !Input.GetMouseButton(2)) {
				transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * rotationSpeed, 0));
				m_EulerX = transform.rotation.eulerAngles.x;
				m_EulerY = transform.rotation.eulerAngles.y;
				transform.rotation = Quaternion.Euler(m_EulerX, m_EulerY, 0);
			}

			// Rotate camera on X axis
			if(Input.GetMouseButton(1) && Input.GetMouseButton(2)) {
				transform.Rotate(-new Vector3(Input.GetAxis("Mouse Y") * rotationSpeed, 0, 0));
				m_EulerX = transform.rotation.eulerAngles.x;
				m_EulerY = transform.rotation.eulerAngles.y;
				transform.rotation = Quaternion.Euler(m_EulerX, m_EulerY, 0);
			}
		}
	}
}
