using Evolution;
using UnityEngine;

namespace UI
{
    public class Lifecycle : MonoBehaviour
    {
        public void Pause()
        {
            Hm.instance.Pause();
        }
        
        public void Play()
        {
            Hm.instance.Play();
        }
        
        public void Reset()
        {
            Debug.Log($"Reset");
            Hm.instance.Reset();
        }
    }
}
