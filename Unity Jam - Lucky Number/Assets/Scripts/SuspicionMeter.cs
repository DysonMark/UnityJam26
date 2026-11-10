using System;
using System.Runtime.CompilerServices;
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
        [SerializeField] private float pulseAbove = 0.7f;

        private float suspicion;
        private float shown;
        private Image fill;

        private void Awake() => fill = slider.fillRect.GetComponent<Image>();

        // C5: slide toward the real value, cream -> rust, pulse when it's close to caught
        private void Update()
        {
            shown = Mathf.MoveTowards(shown, suspicion, fillSpeed * Time.deltaTime);
            slider.value = shown;
            fill.color = Color.Lerp(calmColor, hotColor, shown);
            float pulse = shown > pulseAbove ? 1f + 0.06f * Mathf.Sin(Time.time * 8f) : 1f;
            slider.transform.localScale = Vector3.one * pulse;
        }

        public float Value => suspicion;
        public bool IsCaught => DiceMath.IsCaught(suspicion);

        public void OnRoll(bool usedLoaded, int rolled, int luckyNumber, float perLuckyRoll)
        {
            float before = suspicion;
            suspicion = DiceMath.AddSuspicion(suspicion, usedLoaded, rolled, luckyNumber, perLuckyRoll);
            if (suspicion > before) StartCoroutine(Juice.Shake((RectTransform)slider.transform, 6f, 0.3f));
        }

        public void Lower(float amount)
        {
            suspicion = DiceMath.LowerSuspicion(suspicion, amount);
        }
    }
}