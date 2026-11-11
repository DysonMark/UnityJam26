using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UnityJam.LuckyNumber.Intro
{
    public class IntroSequence : MonoBehaviour
    {
        [SerializeField] private Typewriter typewriter;
        [SerializeField, TextArea(2, 5)] private string[] lines;

        [Header("Music")]
        [SerializeField] private AudioSource music;
        [SerializeField] private float musicVolume = 0.6f;
        [SerializeField] private float fadeInTime = 2f;
        [SerializeField] private float fadeOutTime = 1.5f;

        [Header("Reading time")]
        [SerializeField] private float holdBase = 1.2f;
        [SerializeField] private float holdPerChar = 0.035f;

        [Header("Lighting")]
        [SerializeField] private IntroEffects lights;

        [Header("Next scene")]
        [SerializeField] private string mainSceneName = "Main";

        private void Start()
        {
            StartCoroutine(PlayIntro());
        }

        private IEnumerator PlayIntro()
        {
            if (music != null)
            {
                music.volume = 0f;
                music.Play();
                StartCoroutine(FadeVolume(music, 0f, musicVolume, fadeInTime));
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (lights != null)
                    lights.SetBeat(i);

                typewriter.Play(line);

                bool isCompletedByPress = false;
                while (typewriter.IsTyping)
                {
                    if (PressedThisFrame())
                    {
                        typewriter.Advance();
                        while (typewriter.IsTyping)
                            yield return null;
                        isCompletedByPress = true;
                        break;
                    }
                    yield return null;
                }

                if (isCompletedByPress)
                {
                    while (typewriter.IsTyping)
                        yield return null;
                    yield return null;
                }

                float hold = holdBase + line.Length * holdPerChar;
                float t = 0f;
                while (t < hold && !PressedThisFrame())
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                yield return null;
            }

            if (music != null)
                yield return FadeVolume(music, music.volume, 0f, fadeOutTime);

            SceneManager.LoadScene(mainSceneName);
        }

        private bool PressedThisFrame()
        {
            return (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);
        }

        private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            source.volume = to;
        }
    }
}
