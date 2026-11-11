using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UnityJam.LuckyNumber.Intro
{
    public class Typewriter : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        [Header("Timing")]
        [SerializeField] private float charDelay = 0.04f;
        [SerializeField] private float commaPause = 0.15f;
        [SerializeField] private float sentencePause = 0.3f;
        [SerializeField] private float ellipsisPause = 0.15f;

        [Header("Sound")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] blips;
        [SerializeField] private int blipEvery = 2;

        [Header("Events")]
        public UnityEvent OnFinished;
        public UnityEvent OnContinue;

        public bool IsTyping { get; private set; }

        private Coroutine routine;
        private bool skip;

        public void Play(string line)
        {
            if (routine != null) 
                StopCoroutine(routine);
            routine = StartCoroutine(Type(line));
        }

        public void Advance()
        {
            if (IsTyping) 
                skip = true;
            else 
                OnContinue.Invoke();
        }

        private IEnumerator Type(string line)
        {
            IsTyping = true;
            skip = false;

            text.text = line;
            text.maxVisibleCharacters = 0;
            text.ForceMeshUpdate();
            int total = text.textInfo.characterCount;

            for (int i = 1; i <= total; i++)
            {
                text.maxVisibleCharacters = i;
                char shown = text.textInfo.characterInfo[i - 1].character;
                char next = i < total ? text.textInfo.characterInfo[i].character : '\0';

                if (!char.IsWhiteSpace(shown) && i % blipEvery == 0) 
                    PlayBlip();

                yield return Wait(GetDelay(shown, next));

                if (skip) 
                    break;
            }

            text.maxVisibleCharacters = total;
            IsTyping = false;
            routine = null;
            OnFinished.Invoke();
        }

        private float GetDelay(char c, char next)
        {
            if (next == '\0') 
                return 0f;                     

            bool clauseEnds = char.IsWhiteSpace(next) || next == '"';

            switch (c)
            {
                case '.':
                    if (next == '.') 
                        return ellipsisPause;  
                    return clauseEnds ? sentencePause : charDelay;

                case '!':
                case '?':
                    if (next == '!' || next == '?') 
                        return charDelay;
                    return clauseEnds ? sentencePause : charDelay;

                case ',':
                    return clauseEnds ? commaPause : charDelay;

                default:
                    return charDelay;
            }
        }

        private IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds && !skip)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }


        private void PlayBlip()
        {
            if (audioSource == null || blips.Length == 0) 
                return;

            audioSource.pitch = Random.Range(0.92f, 1.05f);
            audioSource.PlayOneShot(blips[Random.Range(0, blips.Length)]);
        }

    }
}