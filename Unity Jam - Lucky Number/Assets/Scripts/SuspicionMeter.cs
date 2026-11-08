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

        private float suspicion;

        public bool IsCaught => DiceMath.IsCaught(suspicion);

        public void OnRoll(bool usedLoaded, int rolled, int luckyNumber, float perLuckyRoll)
        {
            suspicion = DiceMath.AddSuspicion(suspicion, usedLoaded, rolled, luckyNumber, perLuckyRoll);
            slider.value = suspicion;
        }

        public void Lower(float amount)
        {
            suspicion = DiceMath.LowerSuspicion(suspicion, amount);
            slider.value = suspicion;
        }
    }
}