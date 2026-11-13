using System.Collections.Generic;
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
            public AudioClip[] clips; // one is picked at random each play
            [UnityEngine.Range(0f, 1f)] public float volume = 1f;
        }

        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Sounds")]
        [SerializeField] private List<Sound> sounds = new List<Sound>();

        [Header("Music")]
        [Tooltip("Base loop. Plays in every scene except the quiet one.")]
        [SerializeField] private AudioClip startMusic;
        [Tooltip("Loops the exact length of the base loop. Each fades in once suspicion reaches its Layer At value.")]
        [SerializeField] private AudioClip[] layers = new AudioClip[2];
        [SerializeField] private float[] layerAt = { 0.3f, 0.65f };
        [SerializeField] private float fadeIn = 1.5f;
        [SerializeField] private float fadeOut = 2.5f;
        [SerializeField] private float caughtFade = 0.3f;
        [SerializeField, Range(0f, 1f)] private float pausedVolume = 0.4f;
        [Tooltip("Scene with its own music: everything here fades out while it's loaded.")]
        [SerializeField] private string quietScene = "Intro";

        [Header("Buttons")]
        [Tooltip("Automatically plays this sound on every button in the scene.")]
        [SerializeField] private string buttonClickSound = "Click";

        private readonly Dictionary<string, Sound> lookup = new Dictionary<string, Sound>();
        private AudioSource[] layerSources;
        private float suspicion;
        private bool caught, quiet;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (Sound s in sounds)
            {
                if (!string.IsNullOrEmpty(s.name) && !lookup.ContainsKey(s.name))
                    lookup.Add(s.name, s);
            }

            musicSource.loop = true;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            if (Instance != this) return; // duplicate, being destroyed

            // base + layers all start on the same audio tick and never stop, so they stay in time; only volumes change
            double startAt = AudioSettings.dspTime + 0.2;
            musicSource.Stop();
            musicSource.volume = 0f;
            musicSource.clip = startMusic;
            if (startMusic != null) musicSource.PlayScheduled(startAt);

            layerSources = new AudioSource[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                AudioSource layer = musicSource.gameObject.AddComponent<AudioSource>();
                layer.outputAudioMixerGroup = musicSource.outputAudioMixerGroup;
                layer.playOnAwake = false;
                layer.loop = true;
                layer.volume = 0f;
                layer.clip = layers[i];
                if (layers[i] != null) layer.PlayScheduled(startAt);
                layerSources[i] = layer;
            }
        }

        private void Update()
        {
            if (layerSources == null) return;
            float on = quiet || caught ? 0f : Menus.PauseMenu.IsPaused ? pausedVolume : 1f;
            Fade(musicSource, on);
            for (int i = 0; i < layerSources.Length; i++)
                Fade(layerSources[i], suspicion >= layerAt[i] ? on : 0f);
        }

        private void Fade(AudioSource source, float target)
        {
            float time = caught ? caughtFade : target > source.volume ? fadeIn : fadeOut;
            source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime / time);
        }

        // --- music --- //

        // 0..1, from the suspicion meter; the layers follow it
        public void SetSuspicion(float value) => suspicion = value;

        // cut the music and play the sting; music comes back on the next scene load
        public void Caught()
        {
            caught = true;
            PlaySFX("Caught");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // --- playing sounds --- //

        public void PlaySFX(string soundName)
        {
            if (lookup.TryGetValue(soundName, out Sound s) && s.clips != null && s.clips.Length > 0)
                sfxSource.PlayOneShot(s.clips[Random.Range(0, s.clips.Length)], s.volume);
            else
                Debug.LogWarning($"AudioManager: no clip for \"{soundName}\"");
        }

        // --- auto button sounds --- //

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            suspicion = 0f;
            caught = false;
            quiet = scene.name == quietScene;

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

