using Gameplay;
using Api.Session;
using Evolution;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI
{
    [RequireComponent(typeof(UnitSelection))]
    public class Actions : MonoBehaviour
    {
        // [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private EscapeMenu escapeMenu;
        [SerializeField] private Menu evolutionMenu;
        private UnitSelection _unitSelection;

        private float _lastFireball;
        private bool _isDragging;
        private GameObject _draggedObject;
        private Camera _camera;


        private void Awake()
        {
	        _camera = Camera.main;
            _unitSelection = GetComponent<UnitSelection>();
        }

        private void Update() {
            if (_draggedObject != null && _isDragging)
            {
                DragObject();
            }
            // if (Time.time > m_LastFireball && Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.Mouse0))
            // {
            //     m_LastFireball = Time.time + 0.2f; // 2 sec cooldown
            //     ThrowFireBall();
            // }
        }

        // private void ThrowFireBall()
        // {
        //     Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        //     var camPos = _camera.transform.position;
        //     var go = Pool.Spawn(fireballPrefab, Vector3.Lerp(camPos, ray.direction, 0.1f),
        //         Quaternion.LookRotation(ray.direction));
        //     go.GetComponent<Rigidbody>().AddForce(go.transform.forward*1000f);
        //     // go.transform.localScale *= 10;
        // }


        private void DragObject()
        {
            _unitSelection.disable = true;
            var v3 = Input.mousePosition;
            v3.z = 100.0f;
            v3 = _camera.ScreenToWorldPoint(v3);
            _draggedObject.transform.position = v3;
        }

        private void StartDragging(Color color)
        {
	        _isDragging = true;
	        _draggedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
	        _draggedObject.transform.localScale = new Vector3(5, 5, 5);
	        _draggedObject.GetComponent<Renderer>().material.color = color;
        }

        private bool StopDragging(out Vector3 hitPos)
        {
            _unitSelection.disable = false;
            hitPos = Vector3.zero;
	        Destroy(_draggedObject);
	        _isDragging = false;
	        var ray = _camera.ScreenPointToRay(Input.mousePosition);
	        if (!Physics.Raycast(ray, out var hit, 1000.0f)) return false;
	        hitPos = hit.transform.position;
	        return true;
        }

        public void StartDraggingAnimal() => StartDragging(Color.red);

        public void StartDraggingTree() => StartDragging(Color.green);

        public void StartDraggingRobot() => StartDragging(Color.magenta);

        public void StopDraggingAnimal()
        {
	        if (!StopDragging(out var hitPos)) return;
	        if (!Gm.instance.online || Sm.instance.isServer)
	        {
		        var p = new Vector3(hitPos.x, 100, hitPos.z).PositionAboveGround();
		        if (p.Equals(Vector3.positiveInfinity))
		        {
			        Debug.LogWarning($"You tried to spawn outside map");
			        return;
		        }
		        Hm.instance.SpawnAnimalSync(p, Quaternion.identity);
	        }
	        else
	        {
		        var p = new Vector3(hitPos.x, 100, hitPos.z).PositionAboveGround();
		        if (p.Equals(Vector3.positiveInfinity))
		        {
			        Debug.LogWarning($"Client tried to spawn outside map");
			        return;
		        }
		        Hm.instance.RequestSpawnAnimal(p, Quaternion.identity);
	        }
        }

        public void StopDraggingTree()
        {
	        if (!StopDragging(out var hitPos)) return;
	        if (!Gm.instance.online || Sm.instance.isServer)
	        {
		        var p = new Vector3(hitPos.x, 100, hitPos.z).PositionAboveGround();
		        if (p.Equals(Vector3.positiveInfinity))
		        {
			        Debug.LogWarning($"You tried to spawn outside map");
			        return;
		        }
		        Hm.instance.SpawnTreeSync(p, Quaternion.identity);
	        }
	        else
	        {
		        var p = new Vector3(hitPos.x, 100, hitPos.z).PositionAboveGround();
		        if (p.Equals(Vector3.positiveInfinity))
		        {
			        Debug.LogWarning($"Client tried to spawn outside map");
			        return;
		        }
		        Hm.instance.RequestSpawnTree(p, Quaternion.identity);
	        }
        }

        public void StopDraggingRobot()
        {
	        if (!StopDragging(out var hitPos)) return;
	        // TODO: popup window with stats: which characteristic to look for ...
	        // e.g: I want fast animals = robot kill all slow animals
        }

        public void OnPointerClickAnimal(BaseEventData data)
        {
            if (((PointerEventData) data).button == PointerEventData.InputButton.Right)
            {
                escapeMenu.EscapePlusOther(evolutionMenu);
            }
        }
    }
}
