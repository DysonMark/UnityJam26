using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.TurnFlow
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private TuningConfig config;
        [SerializeField] private TableInfoPanel infoPanel;
        [SerializeField] private Button payAnteButton;
        [SerializeField] private Button cleanButton;
        [SerializeField] private Button loadedButton;
        [SerializeField] private GameObject luckyNumberPanel;
        [SerializeField] private GameObject betPanel;
        [SerializeField] private GameObject diePanel;
        [SerializeField] private TMP_Text rollResultText;
        [SerializeField] private int seed = 0;
        [SerializeField] private float showResultAfterTime = 1.5f;

        const int AiCount = 3;

        //Private variables

        private System.Random rng;
        private int money;
        private int pot;
        private int rollsLeft;
        private int playerBet;
        private int playerLucky;
        private int[] aiLucky = new int[AiCount];
        private int[] aiBets = new int[AiCount];
        private int[] openNumbers;
        private bool usedLoaded;
        private int rolledNumber;


        private void Start()
        {
            rng = seed == 0 ? new System.Random() : new System.Random(seed);

            money = config.startingMoney;
            rollsLeft = config.rollsPerTable;

            luckyNumberPanel.SetActive(false);
            betPanel.SetActive(false);
            diePanel.SetActive(false);

            UpdatePanel();
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

            if (playerLucky == 0)
                luckyNumberPanel.SetActive(true);
            else
                AIBets();
        }

        public void OnPlayerBet(int number)
        {
            playerBet = number;
            betPanel.SetActive(false);
            UpdatePanel();

            //TODO: check if hasItem after
            diePanel.SetActive(true);
        }

        public void ChooseClean() => Roll(false);
        public void ChooseLoaded() => Roll(true);
        private void Roll(bool loaded)
        {
            usedLoaded = loaded;

            rolledNumber = loaded ? DiceMath.RollLoaded(rng, playerLucky, config.loadedChance) : DiceMath.RollClean(rng);

            StartCoroutine(ShowResult());
        }

        private IEnumerator ShowResult()
        {
            cleanButton.interactable = false;
            loadedButton.interactable = false;
            rollResultText.text = $"Rolled: {rolledNumber}";
            yield return new WaitForSeconds(showResultAfterTime);
            diePanel.SetActive(false);
        }
    }
}