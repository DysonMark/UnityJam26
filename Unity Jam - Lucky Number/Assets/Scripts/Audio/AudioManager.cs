using System.Collections.Generic;
using NUnit.Framework;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.Audio
{

    public class AudioManager : MonoBehaviour
    {
        [System.Serializable]
        public class Sound
        {
            public string name;
            public AudioClip clip;
            [UnityEngine.Range(0f, 1f)] public float volume;
        }

        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Sounds")]
        [SerializeField] private List<Sound> sounds = new List<Sound>();
        [SerializeField] private AudioClip startMusic;

        [Header("Buttons")]
        [Tooltip("Automatically plays this sound on every button in the scene.")]
        [SerializeField] private string buttonClickSound = "Click";

        private readonly Dictionary<string, Sound> lookup = new Dictionary<string, Sound>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

            foreach (Sound s in sounds)
            {
                if (!string.IsNullOrEmpty(s.name) && !lookup.ContainsKey(s.name))
                    lookup.Add(s.name, s);
            }

            musicSource.loop = true;
            musicSource.volume = PlayerPrefs.GetFloat("MusicVol", 1f);
            sfxSource.volume = PlayerPrefs.GetFloat("SFXVol", 1f);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            if (startMusic != null)
                PlayMusic(startMusic);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // --- playing sounds --- //

        public void PlaySFX(string soundName)
        {
            if (lookup.TryGetValue(soundName, out Sound s) && s.clip != null)
                sfxSource.PlayOneShot(s.clip, s.volume);
            else
                Debug.LogWarning($"AudioManager: no sound called \"{soundName}\"");
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();

        // --- volume hook to sliders --- //

        public void SetMusicVolume(float value)
        {
            musicSource.volume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        public void SetSFXVolume(float value)
        {
            sfxSource.volume = value;
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        public float MusicVolume => musicSource.volume;
        public float SFXVolume => sfxSource.volume;

        // --- auto button sounds --- //

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (string.IsNullOrEmpty(buttonClickSound)) return;

            // Includes inactive buttons, so menus that start hidden still get the sound.
            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Button b in buttons)
            {
                b.onClick.RemoveListener(PlayButtonClick);
                b.onClick.AddListener(PlayButtonClick);
            }
        }

        private void PlayButtonClick() => PlaySFX(buttonClickSound);
    }

}

