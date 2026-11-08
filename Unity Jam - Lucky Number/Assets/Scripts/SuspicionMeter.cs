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
        [SerializeField] private Slider slider;
        [SerializeField] private float suspicionPerLuckyRoll = 0.20f;

        private float suspicion;
        private int luckyNumber;
        private bool hasLuckyNumber;

        public bool IsCaught => suspicion >= slider.maxValue;

        public void OnRoll(bool usedLoaded, int rolled, int luckyNumber)
        {
            if (usedLoaded && rolled == luckyNumber)
                AddSuspicion(suspicionPerLuckyRoll);
        }

        private void AddSuspicion(float amount)
        {
            suspicion = Mathf.Clamp(suspicion + amount, 0f, slider.maxValue);
            slider.value = suspicion;
        }
        public void ResetMeter()
        {
            suspicion = 0f;
            slider.value = 0f;
        }
    }
}