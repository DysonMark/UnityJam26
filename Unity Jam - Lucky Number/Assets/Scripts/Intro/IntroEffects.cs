using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.Intro
{
    public class IntroEffects : MonoBehaviour
    {
        [SerializeField] Image[] lightLayers;
        [SerializeField] float[] beatLevels = { 0.35f, 0.55f, 0.75f, 1f };

        Coroutine fade;

        public void SetBeat(int beat)
        {
            beat = Mathf.Clamp(beat, 0, beatLevels.Length - 1);
            if (fade != null)
                StopCoroutine(fade);

            StartCoroutine(FadeTo(beatLevels[beat]));
        }

        private void SetAlpha(float alpha)
        {
            foreach(var img in lightLayers)
            {
                var c = img.color;
                c.a = alpha;
                img.color = c;
            }
        }
        IEnumerator FadeTo(float target)
        {
            float start = lightLayers[0].color.a, t = 0f;

            while (t < 1.2f)
            {
                t += Time.deltaTime;
                SetAlpha(Mathf.Lerp(start, target, t / 1.2f));
                yield return null;
            }
        }
    }
}
