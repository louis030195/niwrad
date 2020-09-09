using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TimescaleMenu : Menu
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
    }
}
