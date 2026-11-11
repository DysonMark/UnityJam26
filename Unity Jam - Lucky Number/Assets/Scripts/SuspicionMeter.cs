using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityJam.LuckyNumber.Interactions;
using Random = System.Random;

namespace UnityJam.LuckyNumber.Meters
{
    public class SuspicionMeter : MonoBehaviour
    {
        [SerializeField] private Slider slider; // 0..1 range, the math in DiceMath assumes it
        [SerializeField] private Color calmColor = new Color(0.96f, 0.89f, 0.79f);  // palette cream
        [SerializeField] private Color hotColor = new Color(0.49f, 0.21f, 0.12f);   // palette rust
        [SerializeField] private float fillSpeed = 0.6f;     // slider units per second
        [SerializeField] private float heartbeatFrom = 0.3f; // beats from here, faster and bigger as it climbs
        [SerializeField] private TMP_Text percentText;        // "35%" in the middle of the meter
        [SerializeField] private TMP_Text floatTextPrefab;    // "+15%" / "-5%" beside the meter
        [SerializeField] private RectTransform floatFrom;     // where it starts, the SUSPICION tag, clear of the % readout

        private float suspicion;
        private float shown;
        private float beat;
        private Image fill;

        private void Awake() => fill = slider.fillRect.GetComponent<Image>();

        // C5: slide toward the real value, cream -> rust, heartbeat that speeds up as it climbs
        private void Update()
        {
            shown = Mathf.MoveTowards(shown, suspicion, fillSpeed * Time.deltaTime);
            slider.value = shown;
            fill.color = Color.Lerp(calmColor, hotColor, shown);
            percentText.text = $"{Mathf.RoundToInt(shown * 100f)}%";

            beat += Time.deltaTime * Mathf.Lerp(3f, 10f, shown); // phase, so a speed change doesn't jump
            float amp = shown >= heartbeatFrom ? Mathf.Lerp(0.04f, 0.12f, shown) : 0f;
            slider.transform.localScale = Vector3.one * (1f + amp * Mathf.Pow(Mathf.Abs(Mathf.Sin(beat)), 6f));
        }

        public float Value => suspicion;
        public bool IsCaught => DiceMath.IsCaught(suspicion);

        public void OnRoll(bool usedLoaded, int rolled, int luckyNumber, float perLuckyRoll)
        {
            float before = suspicion;
            suspicion = DiceMath.AddSuspicion(suspicion, usedLoaded, rolled, luckyNumber, perLuckyRoll);
            if (suspicion > before) StartCoroutine(Juice.Shake((RectTransform)slider.transform, 6f, 0.3f));
            ShowChange(before);
        }

        public void Lower(float amount)
        {
            float before = suspicion;
            suspicion = DiceMath.LowerSuspicion(suspicion, amount);
            ShowChange(before);
        }

        // "+15%" / "-5%" floating up from the meter
        private void ShowChange(float before)
        {
            int change = Mathf.RoundToInt((suspicion - before) * 100f);
            if (change == 0) return;
            StartCoroutine(Juice.FloatText(floatTextPrefab, floatFrom, (change > 0 ? "+" : "") + change + "%"));
        }
    }
}