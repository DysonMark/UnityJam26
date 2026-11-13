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
            if (lookup.TryGetValue(soundName, out Sound s) && s.clips != null && s.clips.Length > 0)
                sfxSource.PlayOneShot(s.clips[Random.Range(0, s.clips.Length)], s.volume);
            else
                Debug.LogWarning($"AudioManager: no clip for \"{soundName}\"");
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();

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

