using UI;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;

namespace Player {

	public class UnitSelection : MonoBehaviour {

		// public GameObject selectionCirclePrefab;

		private Camera m_Cam;
		private const string ShaderPath = "Hidden/Internal-Colored";

		private Material m_LineMaterial;
		private MeshCollider m_Col;
		private Vector3[] m_Vertices;
		private int[] m_Triangles;

		private bool m_Hit;
		private Vector3 m_Point;
		private Vector3 m_Normal;
		private Quaternion m_Rotation;
		private bool m_IsSelecting;
		private Vector3 m_LastMousePosition;
        
        
		public bool disable; // Can disable, useful for example when interacting with UI

		private void Awake()
		{
			m_Cam = Camera.main;

#if UNITY_STANDALONE
			// Lock cursor withing window in standalone
			Cursor.lockState = CursorLockMode.Confined;
#endif
		}

		private void Update () {
			if (m_Cam == null)
			{
				m_Cam = Camera.main;
				if (m_Cam == null)
				{
					Debug.LogError($"There is no camera in the scene !");
					return;
				}
			}
#if UNITY_STANDALONE
			// Can unlock cursor from the window in standalone by pressing escape
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				Cursor.lockState = CursorLockMode.None;
			}
#endif
			if (disable)
			{
				m_IsSelecting = false;
				return;
			}
			var mouse = Input.mousePosition;
			var ray = m_Cam.ScreenPointToRay(mouse);
			m_Hit = Physics.Raycast(ray, out var info, float.MaxValue);
			if(m_Hit) {
				m_Point = info.point;
				// var t = info.triangleIndex * 3;
				// var a = m_Triangles[t];
				// var b = m_Triangles[t + 1];
				// var c = m_Triangles[t + 2];
				// var va = m_Vertices[a];
				// var vb = m_Vertices[b];
				// var vc = m_Vertices[c];
				// m_Normal = transform.TransformDirection(Vector3.Cross(vb - va, vc - va));
				// m_Rotation = Quaternion.LookRotation(m_Normal);
			}

			// If we press the left mouse button, save mouse location and begin selection
			if( Input.GetMouseButtonDown(0))
			{
				m_IsSelecting = true;
				m_LastMousePosition = Input.mousePosition;

				foreach (var selectableObject in FindObjectsOfType<SelectableUnit>())
				{
					selectableObject.information.SetActive(false);
					selectableObject.selectionCircle.SetActive(false);
				}
			}
			// If we let go of the left mouse button, end selection
			if (Input.GetMouseButtonUp(0))
			{
				m_IsSelecting = false;
			}

			// Highlight all objects within the selection box
			if (m_IsSelecting)
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

			// if(Input.GetMouseButtonUp(0) && hit) {
			// 	var go = Instantiate(prefabs[Random.Range(0, prefabs.Count)]) as GameObject;
			// 	go.transform.position = point;
			// 	go.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
			// 	go.transform.localRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
			//
			// 	var tree = go.GetComponent<ProceduralTree>();
			// 	tree.Data.randomSeed = Random.Range(0, 300);
			// }
			if (Input.GetMouseButtonUp(0) && m_Hit)
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
			if (m_IsSelecting)
			{
				// Create a rect from both mouse positions
				var rect = Draw.GetScreenRect( m_LastMousePosition, Input.mousePosition );
				Draw.DrawScreenRect( rect, new Color( 0.8f, 0.8f, 0.95f, 0.25f ) );
				Draw.DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
			}
		}

		private bool IsWithinSelectionBounds( GameObject go )
		{
			if( !m_IsSelecting ) return false;
			var viewportBounds = Draw.GetViewportBounds( m_Cam, m_LastMousePosition, Input.mousePosition );
			return viewportBounds.Contains( m_Cam.WorldToViewportPoint( go.transform.position ) );
		}

		private const int Resolution = 16;
		private const float Pi2 = Mathf.PI * 2f;
		private const float Radius = 0.5f;
		private readonly Color m_Color = Color.red;//new Color(0.6f, 0.75f, 1f);
		private static readonly int ZTest = Shader.PropertyToID("_ZTest");

		private void OnRenderObject () {
			if(!m_Hit) return;

			CheckInit();

			m_LineMaterial.SetPass(0);
			m_LineMaterial.SetInt(ZTest, (int)CompareFunction.Always);

			GL.PushMatrix();
			GL.Begin(GL.LINES);
			GL.Color(m_Color);

			for(int i = 0; i < Resolution; i++) {
				var cur = (float)i / Resolution * Pi2;
				var next = (float)(i + 1) / Resolution * Pi2;
				var p1 = m_Rotation * new Vector3(Mathf.Cos(cur), Mathf.Sin(cur), 0f);
				var p2 = m_Rotation * new Vector3(Mathf.Cos(next), Mathf.Sin(next), 0f);
				GL.Vertex(m_Point + p1 * Radius);
				GL.Vertex(m_Point + p2 * Radius);
			}

			GL.End();
			GL.PopMatrix();
		}

		// private void OnEnable () {
		// 	m_Col = GetComponent<MeshCollider>();
		// 	var mesh = GetComponent<MeshFilter>().sharedMesh;
		// 	m_Vertices = mesh.vertices;
		// 	m_Triangles = mesh.triangles;
		// }

		private void CheckInit () {
			if (m_LineMaterial == null) {
				var shader = Shader.Find(ShaderPath);
				if (shader == null) return;
				m_LineMaterial = new Material(shader) {hideFlags = HideFlags.HideAndDontSave};
			}
		}
    }

}

