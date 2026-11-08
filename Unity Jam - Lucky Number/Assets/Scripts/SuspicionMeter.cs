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
        [SerializeField] private int seed = 12345;
        [SerializeField] private Slider slider;
        [SerializeField, Range(0f, 1f)] private float loadedChance = 0.3f;
        [SerializeField] private float suspicionPerLuckyRoll = 0.20f;

        private Random rng;
        private float suspicion;
        private int luckyNumber;
        private bool hasLuckyNumber;

        private void Awake()
        {
            rng = new Random(seed);
        }

        public void SetLuckyNumber(int number)
        {
            luckyNumber = number;
            hasLuckyNumber = true;
            Debug.Log($"Lucky number set to {luckyNumber}");
        }

        private void Update()
        {
            if (!hasLuckyNumber)
                return;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TryRollLoaded();
            }
        }

        private void TryRollLoaded()
        {
            int roll = DiceMath.RollLoaded(rng, luckyNumber, loadedChance);
            
            Debug.Log("Number that was rolled this round: " + roll);

            if (roll == luckyNumber)
            {
                AddSuspicion(suspicionPerLuckyRoll);
            }
        }

        private void AddSuspicion(float amount)
        {
            suspicion = Mathf.Clamp(suspicion + amount, 0f, slider.maxValue);
            slider.value = suspicion;
        }
    }
}
