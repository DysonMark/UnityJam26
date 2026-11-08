using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityJam.LuckyNumber.Meters;

namespace UnityJam.LuckyNumber.TurnFlow
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private SuspicionMeter suspicionMeter;
        [SerializeField] private TuningConfig[] tables;
        [SerializeField] private TableInfoPanel infoPanel;
        [SerializeField] private Button payAnteButton;
        [SerializeField] private Button cleanButton;
        [SerializeField] private Button loadedButton;
        [SerializeField] private GameObject luckyNumberPanel;
        [SerializeField] private GameObject betPanel;
        [SerializeField] private GameObject diePanel;
        [SerializeField] private TMP_Text rollResultText;
        [SerializeField] private TMP_Text luckyLabel;
        [SerializeField] private TMP_Text betLabel;
        [SerializeField] private int seed = 0;
        [SerializeField] private float showResultAfterTime = 1.5f;

        const int AiCount = 3;

        //Private variables

        private int tableIndex;
        private TuningConfig config => tables[tableIndex];
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
            StartTable();

            money = config.startingMoney;
            rollsLeft = config.rollsPerTable;

            luckyNumberPanel.SetActive(false);
            betPanel.SetActive(false);
            diePanel.SetActive(false);

            UpdatePanel();
        }

        private void StartTable()
        {
            money = config.startingMoney;
            rollsLeft = config.rollsPerTable;
            pot = 0;

            playerLucky = 0;
            aiLucky = new int[AiCount];
            aiBets = new int[AiCount];
            suspicionMeter.ResetMeter();
            luckyLabel.text = "-";
            betLabel.text = "-";

            luckyNumberPanel.SetActive(false);
            betPanel.SetActive(false);
            diePanel.SetActive(false);
            payAnteButton.interactable = true;

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
            infoPanel.Refresh(money, pot, rollsLeft, config.cashTarget, tableIndex + 1);
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
            cleanButton.interactable = true;
            loadedButton.interactable = true;
            rollResultText.text = "";
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

            ResolvePayout();
            suspicionMeter.OnRoll(usedLoaded, rolledNumber, playerLucky);
            rollsLeft--;
            UpdatePanel();

            EndTurn();
        }

        private void ResolvePayout()
        {
            int[] bets = { playerBet, aiBets[0], aiBets[1], aiBets[2] };

            PotResult result = DiceMath.ResolvePot(rolledNumber, bets, pot, playerLucky, null, config.luckyMultiplier, config.secondMultiplier);

            money += result.PlayerPayout;
            pot = result.Carryover;

            if (result.PlayerPayout > 0)
                rollResultText.text = $"Rolled: {rolledNumber} - You win {result.PlayerPayout}!";
            else if (result.WinnerCount == 0)
                rollResultText.text = $"Rolled: {rolledNumber} - No win, pot carries over";
            else
                rollResultText.text = $"Rolled: {rolledNumber} - AI wins";
        }

        private void EndTurn()
        {
            if (money >= config.cashTarget)
            {
                if (tableIndex < tables.Length - 1)
                {
                    tableIndex++;
                    StartTable();
                    Debug.Log($"Table cleared! Now on table {tableIndex + 1}");
                }
                else
                {
                    Debug.Log("All tables cleared - you win!");
                }
                return;
            }

            if (suspicionMeter.IsCaught) 
            { 
                Debug.Log("Caught cheating!"); 
                return; 
            }

            if (rollsLeft <= 0) 
            { 
                Debug.Log("Out of rolls"); 
                return; 
            }

            if (money < config.ante) 
            { 
                Debug.Log("Out of money"); 
                return; 
            }

            payAnteButton.interactable = true;
        }
    }
}