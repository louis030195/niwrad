using System;
using Gameplay;
using Api.Match;
using Api.Session;
using Evolution;
using UnityEngine;
using Utils;

namespace UI
{
    public class Actions : MonoBehaviour
    {
        [SerializeField] private GameObject fireballPrefab;

        private float m_LastFireball;
        private bool m_IsDragging;
        private GameObject m_DraggedObject;
        private Camera m_Camera;


        private void Awake()
        {
	        m_Camera = Camera.main;
        }

        private void Update() {
            if (m_DraggedObject != null && m_IsDragging) {
                DragObject();
            }
            // if (Time.time > m_LastFireball && Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.Mouse0))
            // {
            //     m_LastFireball = Time.time + 0.2f; // 2 sec cooldown
            //     ThrowFireBall();
            // }
        }

        private void ThrowFireBall()
        {
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            var camPos = m_Camera.transform.position;
            var go = Pool.Spawn(fireballPrefab, Vector3.Lerp(camPos, ray.direction, 0.1f),
                Quaternion.LookRotation(ray.direction));
            go.GetComponent<Rigidbody>().AddForce(go.transform.forward*1000f);
            // go.transform.localScale *= 10;
        }


        private void DragObject()
        {
            var v3 = Input.mousePosition;
            v3.z = 100.0f;
            v3 = m_Camera.ScreenToWorldPoint(v3);
            m_DraggedObject.transform.position = v3;
        }

        private void StartDragging(Color color)
        {
	        m_IsDragging = true;
	        m_DraggedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
	        m_DraggedObject.transform.localScale = new Vector3(5, 5, 5);
	        m_DraggedObject.GetComponent<Renderer>().material.color = color;
        }

        private bool StopDragging(out Vector3 hitPos)
        {
	        hitPos = Vector3.zero;
	        Destroy(m_DraggedObject);
	        m_IsDragging = false;
	        var ray = m_Camera.ScreenPointToRay(Input.mousePosition);
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
    }
}
