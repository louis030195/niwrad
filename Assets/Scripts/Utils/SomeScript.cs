using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public class SomeScript : MonoBehaviour
    {
        // Start is called before the first frame update
        async void Start()
        {
            var t = ProceduralTerrain.Generate(10,
                10,
                030195,
                1000,
                0.8f);
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var randomPositionOnTerrain = Random.insideUnitCircle * Mathf.Pow(2, 9);
            go.transform.position = new Vector3(10, 100, 10);/*t.GetComponent<Terrain>().GetCenter() + 
                                    new Vector3(randomPositionOnTerrain.x, 0, randomPositionOnTerrain.y) +
                                    Vector3.up * 10;*/
            Debug.Log(go.transform.position);
            await Task.Delay(2000);
            go.transform.position = go.transform.position.PositionAboveGround(prefabHeight: 2);
            Debug.Log(go.transform.position);
        }
    }
}
