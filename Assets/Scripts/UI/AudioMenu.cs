using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
    public class AudioMenu : Menu
    {
        public AudioMixer mixer;

        public Slider masterSlider;
        public Slider musicSlider;
        public Slider masterSfxSlider;

        private float _masterVolume;
        private float _musicVolume;
        private float _masterSfxVolume;

        private const float KMinVolume = -80f;
        private const string KMasterVolumeFloatName = "MasterVolume";
        private const string KMusicVolumeFloatName = "MusicVolume";
        private const string KMasterSfxVolumeFloatName = "MasterSFXVolume";

        protected override void Start()
        {
            base.Start();
            masterSlider.onValueChanged.AddListener(MasterVolumeChangeValue);
            musicSlider.onValueChanged.AddListener(MusicVolumeChangeValue);
            masterSfxSlider.onValueChanged.AddListener(MasterSFXVolumeChangeValue);
        }

        public override void Show()
        {
            base.Show();
            mixer.GetFloat(KMasterVolumeFloatName, out _masterVolume);
            mixer.GetFloat(KMusicVolumeFloatName, out _musicVolume);
            mixer.GetFloat(KMasterSfxVolumeFloatName, out _masterSfxVolume);

            masterSlider.value = 1.0f - _masterVolume / KMinVolume;
            musicSlider.value = 1.0f - _musicVolume / KMinVolume;
            masterSfxSlider.value = 1.0f - _masterSfxVolume / KMinVolume;
        }

        public override void Hide()
        {
            base.Hide();
            PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
            PlayerPrefs.SetFloat("MasterSFXVolume", _masterSfxVolume);
            PlayerPrefs.Save();
        }

        public void MasterVolumeChangeValue(float value)
        {
            _masterVolume = KMinVolume * (1.0f - value);
            mixer.SetFloat(KMasterVolumeFloatName, _masterVolume);
        }

        public void MusicVolumeChangeValue(float value)
        {
            _musicVolume = KMinVolume * (1.0f - value);
            mixer.SetFloat(KMusicVolumeFloatName, _musicVolume);
        }

        public void MasterSFXVolumeChangeValue(float value)
        {
            _masterSfxVolume = KMinVolume * (1.0f - value);
            mixer.SetFloat(KMasterSfxVolumeFloatName, _masterSfxVolume);
        }
    }
}
