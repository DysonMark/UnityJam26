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

    // Metallic sweep across a button every few seconds while it can be pressed. stripe sits under a RectMask2D.
    public static IEnumerator Shine(RectTransform stripe, Selectable button, float period = 2.5f, float time = 0.45f)
    {
        float edge = ((RectTransform)stripe.parent).rect.width / 2f + stripe.rect.width;
        stripe.anchoredPosition = new Vector2(-edge, 0f);
        while (true)
        {
            yield return new WaitForSeconds(period);
            if (!button.interactable) continue;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                stripe.anchoredPosition = new Vector2(Mathf.Lerp(-edge, edge, t / time), 0f);
                yield return null;
            }
            stripe.anchoredPosition = new Vector2(-edge, 0f);
        }
    }

    // Slow pulse forever: colour toward glow and a gentle swell in step, stop it with StopCoroutine.
    public static IEnumerator Pulse(Graphic graphic, Color glow, float period = 1.6f, float grow = 0.08f)
    {
        Color start = graphic.color;
        Vector3 size = graphic.transform.localScale;
        for (float t = 0; ; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(t * 2f / period, 1f));
            graphic.color = Color.Lerp(start, glow, k);
            graphic.transform.localScale = size * (1f + grow * k);
            yield return null;
        }
    }

    // count coins arc from -> to, one every gap seconds. One loop moves all of them, so no MonoBehaviour needed.
    public static IEnumerator FlyCoins(RectTransform prefab, RectTransform from, RectTransform to,
        int count = 6, float gap = 0.06f, float flightTime = 0.35f, float arcHeight = 60f)
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
