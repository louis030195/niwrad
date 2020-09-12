using System;
using Gameplay;
using Api.Session;
using Evolution;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.Physics;
using Random = UnityEngine.Random;

namespace UI
{
    [RequireComponent(typeof(UnitSelection))]
    public class Actions : MonoBehaviour
    {
        [SerializeField] private EscapeMenu escapeMenu;
        [SerializeField] private Menu evolutionMenu;
        [SerializeField] private Slider sliderAnimal;
        [SerializeField] private Slider sliderVegetation;
        
        private UnitSelection _unitSelection;

        private bool _isDragging;
        private GameObject _draggedObject;
        private Camera _camera;


        private delegate void Spawn(Vector3 p, Quaternion r);
        private readonly Spawn _hackAnimal = (p, r) => Hm.instance.SpawnAnimalSync(p, r);
        private readonly Spawn _hackVegetation = (p, r) => Hm.instance.SpawnTreeSync(p, r);

        
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
        }
        
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

        private void StopDragging(int n, Color color, Spawn action)
        {
            _unitSelection.disable = false;
            var p = _draggedObject.transform.position;
	        Destroy(_draggedObject);
	        _isDragging = false;

            for (var i = 0; i < n; i++)
            {
                var seed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                seed.transform.position = p + Random.insideUnitSphere * (n * 0.1f);
                void Action()
                {
                    Destroy(seed);
                    action.Invoke(seed.transform.position, Quaternion.identity);
                }

                seed.AddComponent<TriggerActionOnCollision>()
                    .CollisionEnter(Action, "ground");
                seed.AddComponent<Rigidbody>().useGravity = true;
                seed.GetComponent<Renderer>().material.color = color;
            
                // Any case the seed is deleted automatically after a time (outside map ...)
                Destroy(seed, 10f);
            }
        }

        public void StartDraggingAnimal() => StartDragging(Color.red);

        public void StartDraggingTree() => StartDragging(Color.green);

        public void StopDraggingAnimal() => StopDragging((int) sliderAnimal.value, Color.red,
            !Gm.instance.online || Sm.instance.isServer ? _hackAnimal : Hm.instance.RequestSpawnAnimal);

        public void StopDraggingTree() => StopDragging((int) sliderVegetation.value, Color.green,
            !Gm.instance.online || Sm.instance.isServer ? _hackVegetation : Hm.instance.RequestSpawnTree);
        

        // public void OnPointerClickAnimal(BaseEventData data)
        // {
        //     if (((PointerEventData) data).button == PointerEventData.InputButton.Right)
        //     {
        //         escapeMenu.EscapePlusOther(evolutionMenu);
        //     }
        // }
    }
}
