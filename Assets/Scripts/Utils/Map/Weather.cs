using System;
using UnityEngine;

namespace Utils.Map
{
    public class Weather : MonoBehaviour
    {
        private Material _material;

        private void Start()
        {
            _material = GetComponent<MeshRenderer>().material;
        }

        private void Update()
        {
            _material.mainTextureOffset.Set(
                _material.mainTextureOffset.x < 1 ?
                    _material.mainTextureOffset.x + Time.deltaTime :
                    0,
                _material.mainTextureOffset.y
            );
        }
    }
}
