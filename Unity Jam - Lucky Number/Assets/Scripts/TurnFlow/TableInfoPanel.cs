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

        public void Refresh(int money, int pot, int rollsLeft, int target, int table)
        {
            tableText.text = $"Table: {table}";
            moneyText.text = $"Money: {money}";
            potText.text = $"Pot: {pot}";
            rollsLeftText.text = $"Rolls Left: {rollsLeft}";
            targetMoneyText.text = $"Target Money: {target}";
        }

        private void Update()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
                panel.SetActive(!panel.activeSelf);
        }
    }
}