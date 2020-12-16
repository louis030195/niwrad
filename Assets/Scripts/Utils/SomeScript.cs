using System;
using UnityEngine;
using Utils.Physics;

namespace Utils
{
    public class SomeScript : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log($"current pos {transform.position}, above ground {transform.position.PositionAboveGround()}");
        }
    }
}
