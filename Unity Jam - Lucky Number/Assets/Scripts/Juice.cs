using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shared coroutines for the VFX
public static class Juice
{
    // Scale up and back to 1
    public static IEnumerator Punch(Transform target, float scale = 0.4f, float time = 0.15f)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            target.localScale = Vector3.one * (1f + scale * Mathf.Sin(t / time * Mathf.PI));
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    // Random jitter that fades out, then back to the start position
    public static IEnumerator Shake(RectTransform target, float amount = 8f, float time = 0.25f)
    {
        Vector2 start = target.anchoredPosition;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            target.anchoredPosition = start + Random.insideUnitCircle * amount * (1f - t / time);
            yield return null;
        }
        target.anchoredPosition = start;
    }

    // format gets the current value as {0}, e.g. "{0} <size=70%>/ 500</size>"
    public static IEnumerator CountUp(TMP_Text text, int from, int to, float time = 0.5f, string format = "{0}")
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            text.text = string.Format(format, Mathf.RoundToInt(Mathf.Lerp(from, to, t / time)));
            yield return null;
        }
        text.text = string.Format(format, to);
    }

    // Jump to color, fade back to the original
    public static IEnumerator Flash(Graphic graphic, Color color, float time = 0.3f)
    {
        Color start = graphic.color;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            graphic.color = Color.Lerp(color, start, t / time);
            yield return null;
        }
        graphic.color = start;
    }

    // Slow color pulse between the graphic's color and glow, forever, stop it with StopCoroutine.
    public static IEnumerator Pulse(Graphic graphic, Color glow, float period = 1.6f)
    {
        Color start = graphic.color;
        for (float t = 0; ; t += Time.deltaTime)
        {
            graphic.color = Color.Lerp(start, glow, Mathf.PingPong(t * 2f / period, 1f));
            yield return null;
        }
    }

    // count coins arc from -> to, one every gap seconds. One loop moves all of them, so no MonoBehaviour needed.
    public static IEnumerator FlyCoins(RectTransform prefab, RectTransform from, RectTransform to,
        int count = 6, float gap = 0.08f, float flightTime = 0.45f, float arcHeight = 60f)
    {
        Transform parent = from.GetComponentInParent<Canvas>().transform;
        var coins = new RectTransform[count];
        float total = gap * (count - 1) + flightTime;
        for (float t = 0; t < total; t += Time.deltaTime)
        {
            for (int i = 0; i < count; i++)
            {
                float k = (t - i * gap) / flightTime;
                if (k < 0f) continue;
                if (coins[i] == null && k < 1f) coins[i] = Object.Instantiate(prefab, parent);
                if (coins[i] == null) continue;
                if (k >= 1f) { Object.Destroy(coins[i].gameObject); coins[i] = null; continue; }
                coins[i].position = Vector3.Lerp(from.position, to.position, k) + Vector3.up * (arcHeight * Mathf.Sin(k * Mathf.PI));
            }
            yield return null;
        }
        foreach (RectTransform coin in coins)
            if (coin != null) Object.Destroy(coin.gameObject);
    }

    // "+60" / "-10" that rises and fades at a spot (C3)
    public static IEnumerator FloatText(TMP_Text prefab, RectTransform at, string message, float rise = 50f, float time = 0.8f)
    {
        TMP_Text text = Object.Instantiate(prefab, at.GetComponentInParent<Canvas>().transform);
        text.text = message;
        Vector3 start = at.position;
        Color color = text.color;
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            float k = t / time;
            text.rectTransform.position = start + Vector3.up * (rise * k);
            text.color = new Color(color.r, color.g, color.b, color.a * (1f - k));
            yield return null;
        }
        Object.Destroy(text.gameObject);
    }
}
