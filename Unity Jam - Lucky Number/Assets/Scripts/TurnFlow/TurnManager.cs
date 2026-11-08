using UnityEngine;

namespace UnityJam.LuckyNumber.TurnFlow
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] TuningConfig config;
        [SerializeField] TableInfoPanel infoPanel;

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
    }
}
