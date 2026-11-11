using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.TurnFlow
{
    public class TableInfoPanel : MonoBehaviour
    {
        [SerializeField] private TuningConfig config;
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text tableText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text potText;
        [SerializeField] private TMP_Text rollsLeftText;
        [SerializeField] private TMP_Text targetMoneyText;
        [SerializeField] private TMP_Text suspicionText; // optional, cloned from targetMoneyText when not set
        [SerializeField] private UnityEngine.UI.Slider suspicionSlider;
        [SerializeField] private TMP_Text[] aiLuckyTexts;
        [SerializeField] private TMP_Text[] aiBetTexts;
        [SerializeField] private UnityEngine.UI.Slider moneyGauge;
        [SerializeField] private UnityEngine.UI.Image[] rollDots;
        [SerializeField] private Color rollOn = Color.white;
        [SerializeField] private Color rollOff = new Color(0.22f, 0.20f, 0.18f, 1f);

        // ponytail: clone so it shows without a scene edit, one row below target, same spacing as rolls left -> target
        private void Awake()
        {
            if (suspicionText != null) 
                return;
            suspicionText = Instantiate(targetMoneyText, targetMoneyText.transform.parent);
            suspicionText.rectTransform.anchoredPosition += targetMoneyText.rectTransform.anchoredPosition
                                                            - rollsLeftText.rectTransform.anchoredPosition;
        }

        public void Refresh(int money, int pot, int rollsLeft, int target, int table, float suspicion)
        {
            tableText.text = $"Table {table}";
            moneyText.text = $"{money} <size=50%><color=#A39A88>/ {target}</color></size>";
            potText.text = $"Pot \n{pot}";
            int total = config.rollsPerTable;
            int lit = Mathf.Clamp(Mathf.CeilToInt(rollDots.Length * (float)rollsLeft / Mathf.Max(1, total)), 0, rollDots.Length);

            for (int i = 0; i < rollDots.Length; i++)
                rollDots[i].color = i < lit ? rollOn : rollOff;

            rollsLeftText.text = $"{rollsLeft} out of {total}";
            targetMoneyText.text = $"Target Money: {target}";
            float frac = Mathf.InverseLerp(suspicionSlider.minValue, suspicionSlider.maxValue, suspicionSlider.value);
            suspicionText.text = $"Suspicion {frac:P0}";

            moneyGauge.maxValue = Mathf.Max(1, target);
            moneyGauge.value = money;
        }

        public void RefreshAI(int[] lucky, int[] bets)
        {
            for (int i = 0; i < aiLuckyTexts.Length; i++)
            {
                aiLuckyTexts[i].text = lucky[i] == 0 ? "-" : lucky[i].ToString();
                aiBetTexts[i].text = bets[i] == 0 ? "-" : bets[i].ToString();
            }
        }
        private void Update()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                panel.SetActive(!panel.activeSelf);
        }
    }
}