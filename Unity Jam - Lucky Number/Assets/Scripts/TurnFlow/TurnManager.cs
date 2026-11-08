using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.TurnFlow
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] TuningConfig config;
        [SerializeField] TableInfoPanel infoPanel;
        [SerializeField] Button payAnteButton;
        [SerializeField] GameObject luckyNumberPanel;
        [SerializeField] GameObject betPanel;
        [SerializeField] int seed = 0;

        const int AiCount = 3;

        System.Random rng;
        int money;
        int pot;
        int rollsLeft;
        int playerBet;
        int playerLucky;
        int[] aiLucky = new int[AiCount];
        int[] aiBets = new int[AiCount];
        int[] openNumbers;


        private void Start()
        {
            rng = seed == 0 ? new System.Random() : new System.Random(seed);

            money = config.startingMoney;
            rollsLeft = config.rollsPerTable;
            luckyNumberPanel.SetActive(false);
            betPanel.SetActive(false);
            UpdatePanel();

            if (playerLucky == 0)
                luckyNumberPanel.SetActive(true);
            else
                AIBets();
        }

        public void OnPlayerPickedLucky(int number)
        {
            playerLucky = number;
            luckyNumberPanel.SetActive(false);

            aiLucky = DiceMath.AssignAiLuckyNumbers(rng, playerLucky, AiCount);
            openNumbers = DiceMath.OpenNumbers(playerLucky, aiLucky);

            AIBets();
        }

        private void AIBets()
        {
            for (int i = 0; i < AiCount; i++)
                aiBets[i] = DiceMath.PickAiBet(rng, aiLucky[i], config.aiRandomBetChance);

            UpdatePanel();
            betPanel.SetActive(true);
        }

        private void UpdatePanel()
        {
            infoPanel.Refresh(money, pot, rollsLeft, config.cashTarget, config.table);
            infoPanel.RefreshAI(aiLucky, aiBets);
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

        public void OnPlayerBet(int number)
        {
            playerBet = number;
            betPanel.SetActive(false);
            UpdatePanel();
        }
    }
}
