using Evolution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TimeMenu : Menu
    {
        [SerializeField] private Slider timescaleSlider;
        [SerializeField] private TextMeshProUGUI timescaleText;
        
        private void Awake()
        {
            timescaleSlider.onValueChanged.AddListener(value =>
            {
                Time.timeScale = value;
                timescaleText.text = $"{value}";
            });
        }
        
        public void Pause()
        {
            Hm.instance.Pause();
            timescaleText.text = $"{Time.timeScale}";
        }
        
        public void Play()
        {
            Hm.instance.Play();
            timescaleText.text = $"{Time.timeScale}";
        }
        
        public void Reset()
        {
            Hm.instance.Reset();
        }
    }
}
