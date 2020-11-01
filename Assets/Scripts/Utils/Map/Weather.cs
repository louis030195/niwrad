using UnityEngine;

namespace Utils.Map
{
    public class Weather : MonoBehaviour
    {
        [Range(1_000_000, 10_000_000), Tooltip("Delay between skyboxes transitions")] 
        public int delayBetweenSkyboxesTransitions = 5_000_000;
        public Material[] skyboxes;
        private int _currentSkybox;

        // Update is called once per frame
        private void Update()
        {
            // if (Time.time % delayBetweenSkyboxesTransitions == 0)
            // {
            //     _currentSkybox = _currentSkybox + 1 < skyboxes.Length ? _currentSkybox + 1 : 0;
            //     RenderSettings.skybox = skyboxes[_currentSkybox];
            // }
        }
    }
}
