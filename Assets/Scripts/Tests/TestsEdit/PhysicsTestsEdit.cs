using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Tests.TestsEdit
{
    public class PhysicsTests
    {
        [Test]
        public void TerrainGetCenter()
        {
            var n = 10;
            var height = 10;
            var terrain = ProceduralTerrain.Generate(n, height, 030195, 1.5f, 0.5f);
            var middleOfTerrain = terrain.GetComponent<Terrain>().GetCenter();
            Assert.AreEqual(
                new Vector3((Mathf.Pow(2, n)+ 1)/2, height/2, (Mathf.Pow(2, n)+1)/2), 
                middleOfTerrain
            );
        }
        
        // A Test behaves as an ordinary method
        [Test]
        public void PositionAboveGround()
        {
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
                var randomPositionOnTerrain = Random.insideUnitCircle * Mathf.Pow(2, 9);
                go.transform.position = middleOfTerrain + 
                                        new Vector3(randomPositionOnTerrain.x, 0, randomPositionOnTerrain.y) +
                                        Vector3.up * height; // TODO: see @Utils.Spatial.PositionAboveGround
                var aboveGround = go.transform.position.PositionAboveGround(prefabHeight:goScale, LayerMask.NameToLayer("Water"));
                Physics.Raycast(aboveGround, Vector3.down, out var hit, Mathf.Infinity);
                Assert.NotNull(hit.transform, "didn't hit");
                Assert.Less(hit.transform.position.y as IComparable, aboveGround.y, "Ray-cast hit should be below object !");
                gos[i] = go;
            }

            
            // Cleanup
            foreach (var go in gos)
            {
                Object.DestroyImmediate(go);
            }
            Object.DestroyImmediate(terrain);
        }
    }
}
