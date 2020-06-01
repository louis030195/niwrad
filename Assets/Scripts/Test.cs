using UnityEngine;
using Utils;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
	    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
	    var mf = go.GetComponent<MeshFilter>();
	    var m = MeshesExtension.Mutation(mf.sharedMesh);
	    mf.sharedMesh = m;
    }

}
