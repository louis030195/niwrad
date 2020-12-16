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
    public class ArtificialSelection : MonoBehaviour
    {
        [SerializeField] private Slider sliderAnimal;
        [SerializeField] private Slider sliderPlant; // TODO: herbi + carni
        [SerializeField] private EventTrigger triggerAnimal;
        [SerializeField] private EventTrigger triggerPlant;
        [SerializeField] private GameObject seedTemplate;
        private UnitSelection _unitSelection;

        private Rts _rtsControls;
        private bool _isDragging;
        private GameObject _draggedObject;
        private Camera _camera;


        private delegate void Spawn(Vector3 p, Quaternion r);
        private readonly Spawn _hackAnimal = (p, r) => Hm.instance.SpawnAnimalSync(p, r, 
            Gm.instance.Experience.AnimalCharacteristics,
            Gm.instance.Experience.AnimalCharacteristicsMinimumBound,
            Gm.instance.Experience.AnimalCharacteristicsMaximumBound);
        private readonly Spawn _hackPlant = (p, r) => Hm.instance.SpawnPlantSync(p, r, 
            Gm.instance.Experience.PlantCharacteristics,
            Gm.instance.Experience.PlantCharacteristicsMinimumBound,
            Gm.instance.Experience.PlantCharacteristicsMaximumBound);

        
        private void Awake()
        {
            _unitSelection = GetComponentInParent<UnitSelection>(); // TODO: anything better?
	        _camera = Camera.main;
            _rtsControls = new Rts();
            
            // Setup UI callbacks, TODO: must check if something cleaner with new input system
            var e = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            e.callback.AddListener(StartDraggingAnimal);
            triggerAnimal.triggers.Add(e);
            e = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            e.callback.AddListener(StopDraggingAnimal);
            triggerAnimal.triggers.Add(e);
            
            e = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            e.callback.AddListener(StartDraggingPlant);
            triggerPlant.triggers.Add(e);
            e = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            e.callback.AddListener(StopDraggingPlant);
            triggerPlant.triggers.Add(e);
            
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
#if UNITY_STANDALONE || UNITY_EDITOR
            seedPosition = Mouse.current.position.ReadValue();
#elif UNITY_IOS || UNITY_ANDROID
            seedPosition = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.First().screenPosition;
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

        private void StartDraggingAnimal(BaseEventData _) => StartDragging(Color.blue);

        private void StartDraggingPlant(BaseEventData _) => StartDragging(Color.green);

        private void StopDraggingAnimal(BaseEventData _) => StopDragging((int) sliderAnimal.value, Color.blue,
            !Gm.instance.online || Sm.instance.isServer ? _hackAnimal : Hm.instance.RequestSpawnAnimal);

        private void StopDraggingPlant(BaseEventData _) => StopDragging((int) sliderPlant.value, Color.green,
            !Gm.instance.online || Sm.instance.isServer ? _hackPlant : Hm.instance.RequestSpawnPlant);
    }
}
