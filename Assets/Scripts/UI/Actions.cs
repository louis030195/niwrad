using System;
using System.Linq;
using Gameplay;
using Api.Session;
using Evolution;
using Input;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils;
using Utils.Physics;
using Random = UnityEngine.Random;

namespace UI
{
    [RequireComponent(typeof(UnitSelection))]
    public class Actions : MonoBehaviour
    {
        [SerializeField] private Slider sliderAnimal;
        [SerializeField] private Slider sliderVegetation;
        [SerializeField] private GameObject seedTemplate;

        private Rts _rtsControls;
        private UnitSelection _unitSelection;
        private bool _isDragging;
        private GameObject _draggedObject;
        private Camera _camera;


        private delegate void Spawn(Vector3 p, Quaternion r);
        private readonly Spawn _hackAnimal = (p, r) => Hm.instance.SpawnAnimalSync(p, r, 
            Gm.instance.Experience.AnimalCharacteristics,
            Gm.instance.Experience.AnimalCharacteristicsMinimumBound,
            Gm.instance.Experience.AnimalCharacteristicsMaximumBound);
        private readonly Spawn _hackVegetation = (p, r) => Hm.instance.SpawnVegetationSync(p, r, 
            Gm.instance.Experience.VegetationCharacteristics,
            Gm.instance.Experience.VegetationCharacteristicsMinimumBound,
            Gm.instance.Experience.VegetationCharacteristicsMaximumBound);

        
        private void Awake()
        {
	        _camera = Camera.main;
            _unitSelection = GetComponent<UnitSelection>();
            _rtsControls = new Rts();
#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
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

        private void Update() {
            if (_draggedObject != null && _isDragging)
            {
                DragObject();
            }
        }
        
        private void DragObject()
        {
            _unitSelection.disable = true;
            Vector3 seedPosition;
#if UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
            seedPosition = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.FirstOrDefault().screenPosition;
#else
            seedPosition = Mouse.current.position.ReadValue();
#endif
            seedPosition.z = 100.0f;
            seedPosition = _camera.ScreenToWorldPoint(seedPosition);
            _draggedObject.transform.position = seedPosition;
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
                var seed = Pool.Spawn(seedTemplate);
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

        public void StartDraggingAnimal() => StartDragging(Color.blue);

        public void StartDraggingTree() => StartDragging(Color.green);

        public void StopDraggingAnimal() => StopDragging((int) sliderAnimal.value, Color.blue,
            !Gm.instance.online || Sm.instance.isServer ? _hackAnimal : Hm.instance.RequestSpawnAnimal);

        public void StopDraggingTree() => StopDragging((int) sliderVegetation.value, Color.green,
            !Gm.instance.online || Sm.instance.isServer ? _hackVegetation : Hm.instance.RequestSpawnVegetation);
    }
}
