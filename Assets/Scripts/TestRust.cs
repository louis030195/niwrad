using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestRust : MonoBehaviour
{
	[DllImport("niwrad")]
	private static extern int hello_world();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(hello_world());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
