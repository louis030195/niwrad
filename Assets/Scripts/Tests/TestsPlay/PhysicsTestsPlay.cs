using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Player;
using UI;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Utils;
using LightType = UnityEngine.LightType;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Tests.TestsPlay
{
    /// <summary>
    /// Quite slow and visual tests in play mode !
    /// </summary>
    public class PhysicsTestsPlay
    {
        private GameObject SpawnControllerCamera()
        {
            var cam = new GameObject("Camera");
            cam.AddComponent<Camera>();
            cam.AddComponent<CameraController>();
            return cam;
        }
        
        // A Test behaves as an ordinary method
        [UnityTest]
        public IEnumerator PositionAboveGround() => UniTask.ToCoroutine(async () =>
        {
            Debug.Log($"Press escape to stop test");
            
            var light = new GameObject("Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.position = Vector3.up * 1000;
            var cam = SpawnControllerCamera();
            var height = 30;
            var terrain = ProceduralTerrain.Generate(10, 
                height, 
                030195, 
                1000, 
                0.8f
                );
            var middleOfTerrain = terrain.GetComponent<Terrain>().GetCenter();

            var nbObjects = 1000;
            var goScale = 2;
            var gos = new GameObject[nbObjects];
            // Test that spawning above ground then correctly adjust slightly above
            foreach (var i in Enumerable.Range(0, nbObjects))
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.layer = LayerMask.NameToLayer("Water"); // Random layer so that ray-cast doesn't hit
                go.GetComponent<Renderer>().material = MaterialHelper.RandomMaterial("Standard");
                var randomPositionOnTerrain = Random.insideUnitCircle * Mathf.Pow(2, 9);
                go.transform.position = middleOfTerrain + 
                                        new Vector3(randomPositionOnTerrain.x, 0, randomPositionOnTerrain.y) +
                                        Vector3.up * height; // TODO: see @Utils.Spatial.PositionAboveGround
                var aboveGround = go.transform.position.PositionAboveGround(prefabHeight:goScale, LayerMask.NameToLayer("Water"));
                Physics.Raycast(aboveGround, Vector3.down, out var hit, Mathf.Infinity);
                Assert.NotNull(hit.transform, "didn't hit");
                hit.DrawRay(go.transform.position, 
                    Vector3.down * Mathf.Abs(go.transform.position.y - aboveGround.y), 
                    Color.blue, 
                    Mathf.Infinity);
                Assert.Less(hit.transform.position.y as IComparable, aboveGround.y, "Ray-cast hit should be below object !");
                gos[i] = go;
            }

            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Escape));
            
            // Cleanup
            Object.DestroyImmediate(cam);
            foreach (var go in gos)
            {
                Object.DestroyImmediate(go);
            }
            Object.DestroyImmediate(terrain);
        });
    }
}
