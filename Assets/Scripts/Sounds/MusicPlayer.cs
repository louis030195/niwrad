using System.Collections;
using UnityEngine;
using Utils;

namespace Sounds
{
    public class MusicPlayer : Singleton<MusicPlayer>
    {
        [System.Serializable]
        public class Stem
        {
            public AudioSource source;
            public AudioClip clip;
            public float startingSpeedRatio;    // The stem will start when this is lower than currentSpeed/maxSpeed.
        }
        
        public UnityEngine.Audio.AudioMixer mixer;
        public Stem[] stems;
        public float maxVolume = 0.1f;

        protected override void Awake()
        {
            base.Awake();
            // As this is one of the first script executed, set that here.
            // Application.targetFrameRate = 30;
            AudioListener.pause = false;

            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            if (PlayerPrefs.GetFloat("MasterVolume") > float.MinValue)
            {
                mixer.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVolume"));
                mixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume"));
                mixer.SetFloat("MasterSFXVolume", PlayerPrefs.GetFloat("MasterSFXVolume"));
                mixer.SetFloat("MasterUIVolume", PlayerPrefs.GetFloat("MasterUIVolume"));
            }
            else
            {
                mixer.GetFloat("MasterVolume", out var masterVolume);
                mixer.GetFloat("MusicVolume", out var musicVolume);
                mixer.GetFloat("MasterSFXVolume", out var masterSfxVolume);
                mixer.GetFloat("MasterUIVolume", out var masterUiVolume);
                PlayerPrefs.SetFloat("MasterVolume", masterVolume);
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
                PlayerPrefs.SetFloat("MasterSFXVolume", masterSfxVolume);
                PlayerPrefs.SetFloat("MasterUIVolume", masterUiVolume);
                PlayerPrefs.Save();
            }

            StartCoroutine(RestartAllStems());
        }
        
        
        public void SetStem(int index, AudioClip clip)
        {
            if (stems.Length <= index)
            {
                Debug.LogError("Trying to set an undefined stem");
                return;
            }

            stems[index].clip = clip;
        }

        public AudioClip GetStem(int index)
        {
            return stems.Length <= index ? null : stems[index].clip;
        }

        public IEnumerator RestartAllStems()
        {
            foreach (var t in stems)
            {
                t.source.clip = t.clip;
                t.source.volume = 0.0f;
                t.source.Play();
            }

            // This is to fix a bug in the Audio Mixer where attenuation will be applied only a few ms after the source start playing.
            // So we play all source at volume 0.0f first, then wait 50 ms before finally setting the actual volume.
            yield return new WaitForSeconds(0.05f);

            foreach (var t in stems)
            {
                t.source.volume = t.startingSpeedRatio <= 0.0f ? maxVolume : 0.0f;
            }
        }

        public void UpdateVolumes(float currentSpeedRatio)
        {
            const float fadeSpeed = 0.5f;

            foreach (var t in stems)
            {
                var target = currentSpeedRatio >= t.startingSpeedRatio ? maxVolume : 0.0f;
                t.source.volume = Mathf.MoveTowards(t.source.volume, target, fadeSpeed * Time.deltaTime);
            }
        }
    }
}
