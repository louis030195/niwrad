using TMPro;
using UnityEngine;

namespace UI
{
	public class SelectableUnit : MonoBehaviour
	{
		public GameObject selectionCircle;
		public GameObject information;
		[Tooltip("Reference to the text above the head")] public TextMeshProUGUI nameText;
		public string defaultName = "Host";

		private Camera m_Cam;

		private void Start()
		{
			m_Cam = Camera.main;
			var canvas = information.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.worldCamera = m_Cam;
			nameText.text = defaultName;

			// Place the information on top of the mesh
			// Fow now it's just hard coded
			// var p = GetComponent<MeshFilter>().sharedMesh.bounds.center;
			// p.y *= 3f; // With a slight adjustments
			// information.transform.localPosition = p;
		}

		private void Update()
		{
			if (m_Cam == null)
			{
				m_Cam = Camera.main;
				return;
			}
			// Make the canvas always look to the camera
			var rotation = m_Cam.transform.rotation;
			information.transform.LookAt(information.transform.position + rotation * -Vector3.back,rotation * Vector3.up);
		}
	}
}
