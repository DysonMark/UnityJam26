using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityJam.LuckyNumber.Interactions;
using UnityJam.LuckyNumber.Meters;

namespace UnityJam.LuckyNumber.TurnFlow
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private SuspicionMeter suspicionMeter;
        [SerializeField] private TuningConfig[] tables;
        [SerializeField] private TableInfoPanel infoPanel;
        [SerializeField] private ShopPanel shop;
        [SerializeField] private EndScreen endScreen;
        [SerializeField] private Button payAnteButton; // also "keep rolling" once you're over the target
        [SerializeField] private Button leaveButton;   // optional, cloned from payAnteButton when not set
        [SerializeField] private Button cleanButton;
        [SerializeField] private Button loadedButton;
        [SerializeField] private GameObject luckyNumberPanel;
        [SerializeField] private GameObject betPanel;
        [SerializeField] private ItemTray itemTray;         // on betPanel, one-time item buttons
        [SerializeField] private GameObject diePanel;
        [SerializeField] private TMP_Text rollResultText;
        [SerializeField] private TMP_Text anteValueText;
        [SerializeField] private Image dieImage;            // on diePanel, plays the roll strip
        [SerializeField] private Texture2D[] cleanStrips;   // white dice, index 0 = strip that lands on 1
        [SerializeField] private Texture2D[] loadedStrips;  // red dice, same order
        [SerializeField] private float rollFrameTime = 0.03f;
        [SerializeField] private TMP_Text luckyLabel;
        [SerializeField] private TMP_Text betLabel;
        [SerializeField] private int seed = 0;
        [SerializeField] private float showResultAfterTime = 1.5f;

        const int AiCount = 3;

        //Private variables

        private int tableIndex;
        private TuningConfig config; // this table's config with owned items applied
        private readonly List<Item> owned = new List<Item>(); // bought in the shop, last for the next table only
        private System.Random rng;
        private int money;
        private int pot;
        private int rollsLeft;
        private int playerBet;
        private int playerLucky;
        private int? playerSecond; // only with the Second Number item, one of the open numbers
        private int[] aiLucky = new int[AiCount];
        private int[] aiBets = new int[AiCount];
        private int[] openNumbers;
        private bool usedLoaded;
        private bool rabbitsFoot; // cancels the suspicion of the next loaded lucky hit
        private int streak;       // loaded lucky hits in a row
        private float heat;       // copycat AIs, 1 right after you win on your lucky number
        private int lastRoll;     // chaser AI bets it, 0 = no roll yet this table
        private int rolledNumber;
        private Sprite[] rollFrames; // last roll's frames, destroyed on the next roll
        private bool pickingSecond;  // lucky panel is open again for the Second Number item
        private SelectLuckyNumber[] luckyButtons;
        private TMP_Text luckyTitle;
        private string luckyTitleText;
        private readonly List<Item> unplayed = new List<Item>(); // one-time items for this table, not played yet


        private void Awake()
        {
            shop.onBought.AddListener(owned.Add);
            shop.onClosed.AddListener(NextTable);

            // ponytail: clone of the ante button so it works without a scene edit, assign a real one to restyle
            if (leaveButton == null)
            {
                leaveButton = Instantiate(payAnteButton, payAnteButton.transform.parent);
                leaveButton.onClick = new Button.ButtonClickedEvent(); // drop the copied PayAnte call
                leaveButton.GetComponentInChildren<TMP_Text>().text = "Leave table";
                var rect = (RectTransform)leaveButton.transform;
                rect.anchoredPosition += new Vector2(0f, rect.rect.height * 1.2f);
            }
            leaveButton.onClick.AddListener(LeaveTable);

            // The second number pick reuses the lucky panel: same buttons, only the open numbers shown
            luckyButtons = luckyNumberPanel.GetComponentsInChildren<SelectLuckyNumber>(true);
            luckyTitle = luckyNumberPanel.GetComponentInChildren<TMP_Text>(true);
            luckyTitleText = luckyTitle.text;

            itemTray.onPlayed.AddListener(PlayItem);
        }

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
            if (config != null) Destroy(config);
            config = Shop.WithItems(tables[tableIndex], owned);

            money = config.startingMoney;
            rollsLeft = config.rollsPerTable;
            pot = 0;
            heat = 0f;
            lastRoll = 0;
            streak = 0;

            // One-time items wait in the tray on the bet panel until the player plays them
            rabbitsFoot = false;
            unplayed.Clear();
            foreach (Item item in owned)
                if (Shop.IsOneTime(item)) unplayed.Add(item);

            playerLucky = 0;
            aiLucky = new int[AiCount];
            aiBets = new int[AiCount];
            luckyLabel.text = "-";
            betLabel.text = "-";
            anteValueText.text = "-" + config.ante.ToString();

            luckyNumberPanel.SetActive(false);
            betPanel.SetActive(false);
            diePanel.SetActive(false);
            payAnteButton.interactable = true;
            leaveButton.gameObject.SetActive(false);

            UpdatePanel();
        }

        public void OnPlayerPickedLucky(int number)
        {
            luckyNumberPanel.SetActive(false);

            if (pickingSecond)
            {
                pickingSecond = false;
                playerSecond = number;
                luckyLabel.text = $"{playerLucky}, 2nd: {playerSecond}";
                ShowSecondPick(false);
                AIBets();
                return;
            }

            playerLucky = number;
            playerSecond = null;
            aiLucky = DiceMath.AssignAiLuckyNumbers(rng, playerLucky, AiCount);
            openNumbers = DiceMath.OpenNumbers(playerLucky, aiLucky);

            if (owned.Contains(Item.SecondNumber))
            {
                pickingSecond = true;
                ShowSecondPick(true);
                luckyNumberPanel.SetActive(true);
                return;
            }

            AIBets();
        }

        // Second pick: only the open numbers' buttons and a new title. Off: everything back for the next table
        private void ShowSecondPick(bool on)
        {
            foreach (SelectLuckyNumber button in luckyButtons)
                button.gameObject.SetActive(!on || System.Array.IndexOf(openNumbers, button.LuckyNumber) >= 0);
            luckyTitle.text = on ? "Pick your second number" : luckyTitleText;
        }

        private void AIBets()
        {
            for (int i = 0; i < AiCount; i++)
                aiBets[i] = DiceMath.PickAiBet(rng, aiLucky[i], config.aiRandomBetChance, playerLucky,
                    config.copycatChance, heat, i == 0 ? config.chaserChance : 0f, lastRoll); // AI 1 is the chaser

            UpdatePanel();
            itemTray.Show(unplayed);
            betPanel.SetActive(true);
        }

        private void PlayItem(Item item)
        {
            unplayed.Remove(item);
            switch (item)
            {
                case Item.BribeTheDealer:
                    suspicionMeter.Lower(config.bribeAmount);
                    break;
                case Item.ExtraRoll:
                    rollsLeft += config.extraRolls;
                    break;
                case Item.RabbitsFoot:
                    rabbitsFoot = true;
                    break;
            }
            UpdatePanel();
            itemTray.Show(unplayed);
        }

        private void UpdatePanel()
        {
            infoPanel.Refresh(money, pot, rollsLeft, config.cashTarget, tableIndex + 1, suspicionMeter.Value);
            infoPanel.RefreshAI(aiLucky, aiBets);
        }

        public void PayAnte()
        {
            if (money < config.ante)
            {
                Debug.Log("Can't afford the ante");
                return;
            }

            leaveButton.gameObject.SetActive(false);
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
            Texture2D[] strips = usedLoaded ? loadedStrips : cleanStrips;
            yield return PlayRoll(strips[rolledNumber - 1]);
            rollResultText.text = $"Rolled: {rolledNumber}";
            yield return new WaitForSeconds(showResultAfterTime);
            diePanel.SetActive(false);

            ResolvePayout();
            bool loadedHit = usedLoaded && rolledNumber == playerLucky;
            float rise = DiceMath.StreakRise(config.suspicionPerLuckyRoll, streak, config.streakBonus);
            if (rabbitsFoot && loadedHit)
            {
                rise = 0f;
                rabbitsFoot = false;
            }
            suspicionMeter.OnRoll(usedLoaded, rolledNumber, playerLucky, rise);
            if (!usedLoaded) suspicionMeter.Lower(config.cleanCool);
            streak = loadedHit ? streak + 1 : 0;
            rollsLeft--;
            UpdatePanel();

            EndTurn();
        }

        // Strip = square frames left to right, last one shows the rolled face and stays up
        private IEnumerator PlayRoll(Texture2D strip)
        {
            if (rollFrames != null)
                foreach (Sprite frame in rollFrames) Destroy(frame);

            int size = strip.height;
            rollFrames = new Sprite[strip.width / size];
            for (int i = 0; i < rollFrames.Length; i++)
                rollFrames[i] = Sprite.Create(strip, new Rect(i * size, 0, size, size), new Vector2(0.5f, 0.5f));

            foreach (Sprite frame in rollFrames)
            {
                dieImage.sprite = frame;
                yield return new WaitForSeconds(rollFrameTime);
            }
        }

        private void ResolvePayout()
        {
            int[] bets = { playerBet, aiBets[0], aiBets[1], aiBets[2] };

            PotResult result = DiceMath.ResolvePot(rolledNumber, bets, pot, playerLucky, playerSecond, config.luckyMultiplier, config.secondMultiplier);

            money += result.PlayerPayout;
            pot = result.Carryover;
            heat = DiceMath.NextHeat(heat, rolledNumber, playerLucky, result.PlayerPayout);
            lastRoll = rolledNumber;

            if (result.PlayerPayout > 0)
                rollResultText.text = $"Rolled: {rolledNumber} - You win {result.PlayerPayout}!";
            else if (result.WinnerCount == 0)
                rollResultText.text = $"Rolled: {rolledNumber} - No win, pot carries over";
            else
                rollResultText.text = $"Rolled: {rolledNumber} - AI wins";
        }

        private void EndTurn()
        {
            if (suspicionMeter.IsCaught)
            {
                endScreen.Show(EndReason.Caught, money);
                return;
            }

            if (rollsLeft <= 0)
            {
                if (money >= config.cashTarget) LeaveTable();
                else endScreen.Show(EndReason.OutOfRolls, money);
                return;
            }

            // Over the target: leave now, or pay the next ante to keep rolling
            if (money >= config.cashTarget)
            {
                leaveButton.gameObject.SetActive(true);
                payAnteButton.interactable = true;
                return;
            }

            if (money < config.ante)
            {
                endScreen.Show(EndReason.Broke, money);
                return;
            }

            payAnteButton.interactable = true;
        }

        // Table cleared. Buy-in: the next table's stack comes out of your money, the rest is shop money
        public void LeaveTable()
        {
            leaveButton.gameObject.SetActive(false);
            payAnteButton.interactable = false;
            if (tableIndex >= tables.Length - 1)
            {
                endScreen.Show(EndReason.Win, money);
                return;
            }
            owned.Clear(); // items expire with the table they were bought for
            shop.Open(Mathf.Max(0, money - tables[tableIndex + 1].startingMoney));
        }

        // Shop closed, StartTable sets money to the buy-in, leftover shop money doesn't carry
        private void NextTable()
        {
            tableIndex++;
            StartTable();
        }
    }
}