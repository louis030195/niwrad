using Evolution;
using Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TimeMenu : Menu
    {
        [SerializeField] private Slider timescaleSlider;
        [SerializeField] private TextMeshProUGUI timescaleText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Button resetButton;


        protected override void Awake()
        {
            base.Awake();
            timescaleSlider.onValueChanged.AddListener(value =>
            {
                Time.timeScale = value;
                timescaleText.text = $"{value}";
            });
            pauseButton.onClick.AddListener(Pause);
            playButton.onClick.AddListener(Play);
            resetButton.onClick.AddListener(Reset);
        }

        private void Pause()
        {
            Gm.instance.Pause();
            timescaleText.text = $"{Time.timeScale}";
        }

        private void Play()
        {
            Gm.instance.Play();
            timescaleText.text = $"{Time.timeScale}";
        }
        
        private void Reset()
        {
            Gm.instance.Reset();
        }
    }
}
