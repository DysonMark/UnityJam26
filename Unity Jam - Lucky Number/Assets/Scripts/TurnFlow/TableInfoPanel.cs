using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityJam.LuckyNumber.TurnFlow
{
    public class TableInfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text tableText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text potText;
        [SerializeField] private TMP_Text rollsLeftText;
        [SerializeField] private TMP_Text targetMoneyText;
        [SerializeField] private TMP_Text suspicionText; // optional, cloned from targetMoneyText when not set
        [SerializeField] private TMP_Text[] aiTexts;

        // ponytail: clone so it shows without a scene edit, one row below target, same spacing as rolls left -> target
        private void Awake()
        {
            if (suspicionText != null) return;
            suspicionText = Instantiate(targetMoneyText, targetMoneyText.transform.parent);
            suspicionText.rectTransform.anchoredPosition += targetMoneyText.rectTransform.anchoredPosition
                                                            - rollsLeftText.rectTransform.anchoredPosition;
        }

        public void Refresh(int money, int pot, int rollsLeft, int target, int table, float suspicion)
        {
            tableText.text = $"Table: {table}";
            moneyText.text = $"Money: {money}";
            potText.text = $"Pot: {pot}";
            rollsLeftText.text = $"Rolls Left: {rollsLeft}";
            targetMoneyText.text = $"Target Money: {target}";
            suspicionText.text = $"Suspicion: {suspicion:P0}";
        }

        public void RefreshAI(int[] lucky, int[] bets)
        {
            for (int i = 0; i < aiTexts.Length; i++)
            {
                string luckyNum = lucky[i] == 0 ? "-" : lucky[i].ToString();
                string bet = bets[i] == 0 ? "-" : bets[i].ToString();
                aiTexts[i].text = $"AI {i + 1} Lucky: {luckyNum} Bet: {bet}";
            }
        }
        private void Update()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                panel.SetActive(!panel.activeSelf);
        }
    }
}