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
        public Slider masterUiSlider;

        private float _masterVolume;
        private float _musicVolume;
        private float _masterSfxVolume;
        private float _masterUiVolume;

        private const float KMinVolume = -80f;
        private const string KMasterVolumeFloatName = "MasterVolume";
        private const string KMusicVolumeFloatName = "MusicVolume";
        private const string KMasterSfxVolumeFloatName = "MasterSFXVolume";
        private const string KMasterUiVolumeFloatName = "MasterUIVolume";

        protected override void Start()
        {
            base.Start();
            masterSlider.onValueChanged.AddListener(MasterVolumeChangeValue);
            musicSlider.onValueChanged.AddListener(MusicVolumeChangeValue);
            masterSfxSlider.onValueChanged.AddListener(MasterSfxVolumeChangeValue);
            masterUiSlider.onValueChanged.AddListener(MasterUIVolumeChangeValue);
        }

        public override void Show()
        {
            base.Show();
            mixer.GetFloat(KMasterVolumeFloatName, out _masterVolume);
            mixer.GetFloat(KMusicVolumeFloatName, out _musicVolume);
            mixer.GetFloat(KMasterSfxVolumeFloatName, out _masterSfxVolume);
            mixer.GetFloat(KMasterUiVolumeFloatName, out _masterUiVolume);

            masterSlider.value = 1.0f - _masterVolume / KMinVolume;
            musicSlider.value = 1.0f - _musicVolume / KMinVolume;
            masterSfxSlider.value = 1.0f - _masterSfxVolume / KMinVolume;
            masterUiSlider.value = 1.0f - _masterUiVolume / KMinVolume;
        }

        public override void Hide()
        {
            base.Hide();
            PlayerPrefs.SetFloat("MasterVolume", _masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
            PlayerPrefs.SetFloat("MasterSFXVolume", _masterSfxVolume);
            PlayerPrefs.SetFloat("MasterUIVolume", _masterUiVolume);
            PlayerPrefs.Save();
        }

        private void MasterVolumeChangeValue(float value)
        {
            _masterVolume = KMinVolume * (1.0f - value);
            mixer.SetFloat(KMasterVolumeFloatName, _masterVolume);
        }

        private void MusicVolumeChangeValue(float value)
        {
            _musicVolume = KMinVolume * (1.0f - value);
            mixer.SetFloat(KMusicVolumeFloatName, _musicVolume);
        }

        private void MasterSfxVolumeChangeValue(float value)
        {
            _masterSfxVolume = KMinVolume * (1.0f - value);
            mixer.SetFloat(KMasterSfxVolumeFloatName, _masterSfxVolume);
        }

        private void MasterUIVolumeChangeValue(float value)
        {
            _masterUiVolume = KMinVolume * (1.0f - value);
            mixer.SetFloat(KMasterUiVolumeFloatName, _masterUiVolume);
        }
    }
}
