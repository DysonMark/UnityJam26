using UnityEngine;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.TurnFlow
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] TuningConfig config;
        [SerializeField] TableInfoPanel infoPanel;
        [SerializeField] Button payAnteButton;

        int money;
        int pot;
        int rollsLeft;
        private void Start()
        {
            money = config.startingMoney;
            rollsLeft = config.rollsPerTable;
            UpdatePanel();
        }

        private void UpdatePanel()
        {
            infoPanel.Refresh(money, pot, rollsLeft, config.cashTarget, config.table);
        }

        public void PayAnte()
        {
            if (money < config.ante)
            {
                Debug.Log("Can't afford the ante");
                return;
            }

            money -= config.ante;
            pot += config.ante * 4;

            payAnteButton.interactable = false;
            UpdatePanel();
        }
    }
}
