using UnityEngine;

namespace Utils
{
    public static class DebugHelper
    {
        public static void DrawSphere(Vector3 position, Color color, float size = 2f, float duration = 2f)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(sphere, duration);
            var mr = sphere.GetComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Standard")) {color = color};
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * size;
        }
        
        /// <summary>
        /// Render the ray and its colliding hit
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        public static void DrawRay(this RaycastHit hit, Vector3 origin, Vector3 destination, Color color, float duration = 2f)
        {
            Debug.DrawRay(origin, destination, color, duration);
            DrawSphere(hit.point, Color.red, duration: duration);
        }
    }
}
