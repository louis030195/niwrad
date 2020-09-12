using Player;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.TestsHelpers
{
    public static class Helper
    {
        public const string TestScriptableObjectBasicAnimalCharacteristicsPath = "ScriptableObjects/BasicAnimalCharacteristics";
        public const string TestScriptableObjectBasicExperiencePath = "ScriptableObjects/BasicExperience";
        public const string SliderTemplatePath = "Prefabs/SliderTemplate";
        public const string CheckboxTemplatePath = "Prefabs/CheckboxTemplate";

        public static GameObject SpawnControllerCamera()
        {
            var cam = new GameObject("Camera");
            cam.AddComponent<Camera>();
            cam.AddComponent<CameraController>();
            return cam;
        }

        public static Light SpawnLight()
        {
            var light = new GameObject("Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.position = Vector3.up * 1000;
            return light;
        }

        public static GameObject SpawnCanvas()
        {
            var go = new GameObject("canvas");
            go.AddComponent<Canvas>();
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        public static GameObject RenderScriptableObject(ScriptableObject so)
        {
            var canvas = SpawnCanvas();
            var grid = canvas.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(600, 50);
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.spacing = new Vector2(0, 20);
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            SpawnLight();
            SpawnControllerCamera();
            
            var s = Resources.Load(SliderTemplatePath) as GameObject;
            var c = Resources.Load(CheckboxTemplatePath) as GameObject;
            so.Render(canvas.transform);
            return canvas;
        }
    }
}
