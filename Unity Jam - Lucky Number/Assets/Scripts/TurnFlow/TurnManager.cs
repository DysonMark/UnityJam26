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
        [SerializeField] private GameObject diePanel;       // Clean / Loaded buttons + their dice
        [SerializeField] private TMP_Text rollResultText;
        [SerializeField] private TMP_Text anteValueText;
        [SerializeField] private TMP_Text hudMoneyText;
        [SerializeField] private DieRoller cleanDie;        // white die above the Clean button
        [SerializeField] private DieRoller loadedDie;       // red die above the Loaded button
        [SerializeField] private AiController aiController;
        [SerializeField] private TMP_Text luckyLabel;
        [SerializeField] private TMP_Text betLabel;
        [SerializeField] private int seed = 0;
        [SerializeField] private float showResultAfterTime = 1.5f;

        [Header("Juice")]
        [SerializeField] private RectTransform table;       // shaken on die landing and jackpot
        [SerializeField] private Graphic flashOverlay;      // full-screen Image, alpha 0
        [SerializeField] private Color jackpotFlash = new Color(1f, 0.8f, 0.3f, 0.6f);
        [SerializeField] private TMP_Text jackpotText;      // "LUCKY 4!", off until a jackpot
        [SerializeField] private VfxPlayer confetti;        // Confetti_Burst_Gold
        [SerializeField] private VfxPlayer landDust;        // Land_Dust_Stone, moved to the die that rolled
        [SerializeField] private VfxPlayer landFlash;       // Hit_Flash_Yellow
        [SerializeField] private TMP_Text floatTextPrefab;  // "+60" / "-10" near the money
        [SerializeField] private Color luckyGlow = new Color(1f, 0.85f, 0.55f);
        [SerializeField] private TMP_Text streakText;       // "HOT x3", loaded lucky hits in a row 
        [SerializeField] private VfxPlayer streakBurst;     // Star_Burst_Yellow
        [SerializeField] private GameObject bannerPanel;    // table intro 
        [SerializeField] private TMP_Text bannerText;
        [SerializeField] private float bannerTime = 1.6f;

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
        private bool pickingSecond;  // lucky panel is open again for the Second Number item
        private SelectLuckyNumber[] luckyButtons;
        private TMP_Text luckyTitle;
        private string luckyTitleText;
        private readonly List<Item> unplayed = new List<Item>(); // one-time items for this table, not played yet
        private int shownMoney;      // what the HUD shows while it counts up
        private Coroutine moneyCount;


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

            StartCoroutine(Juice.Pulse(luckyLabel, luckyGlow));
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
            if (aiController != null) aiController.Ante(); // TEMP: null guard until Phase 2 wires the scene

            luckyNumberPanel.SetActive(false);
            betPanel.SetActive(false);
            diePanel.SetActive(false);
            payAnteButton.interactable = true;
            leaveButton.gameObject.SetActive(false);
            if (streakText != null) streakText.gameObject.SetActive(false); // TEMP: null guard until Phase 2 wires the scene
            if (jackpotText != null) jackpotText.gameObject.SetActive(false); // TEMP: null guard until Phase 2 wires the scene

            UpdatePanel();
            StartCoroutine(Banner());
        }

        private IEnumerator Banner()
        {
            if (bannerPanel == null || bannerText == null) yield break; // TEMP: null guard until Phase 2 wires the scene
            bannerText.text = $"TABLE {tableIndex + 1}\n<size=60%>Make ${config.cashTarget} in {config.rollsPerTable} rolls</size>";
            bannerPanel.SetActive(true);
            yield return Juice.Punch(bannerPanel.transform, 0.3f, 0.25f);
            yield return new WaitForSeconds(bannerTime);
            bannerPanel.SetActive(false);
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
                StartCoroutine(AIBets());
                return;
            }

            playerLucky = number;
            playerSecond = null;
            aiLucky = DiceMath.AssignAiLuckyNumbers(rng, playerLucky, AiCount);
            openNumbers = DiceMath.OpenNumbers(playerLucky, aiLucky);
            if (aiController != null) aiController.SetLucky(aiLucky); // TEMP: null guard until Phase 2 wires the scene

            if (owned.Contains(Item.SecondNumber))
            {
                pickingSecond = true;
                ShowSecondPick(true);
                luckyNumberPanel.SetActive(true);
                return;
            }

            StartCoroutine(AIBets());
        }

        // Second pick: only the open numbers' buttons and a new title. Off: everything back for the next table
        private void ShowSecondPick(bool on)
        {
            foreach (SelectLuckyNumber button in luckyButtons)
                button.gameObject.SetActive(!on || System.Array.IndexOf(openNumbers, button.LuckyNumber) >= 0);
            luckyTitle.text = on ? "Pick your second number" : luckyTitleText;
        }

        private IEnumerator AIBets()
        {
            for (int i = 0; i < AiCount; i++)
                aiBets[i] = DiceMath.PickAiBet(rng, aiLucky[i], config.aiRandomBetChance, playerLucky,
                    config.copycatChance, heat, i == 0 ? config.chaserChance : 0f, lastRoll); // AI 1 is the chaser

            UpdatePanel();
            if (aiController != null) yield return aiController.TakeBets(rng, aiBets, playerLucky); // TEMP: null guard until Phase 2 wires the scene
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
            if (money != shownMoney)
            {
                if (moneyCount != null) StopCoroutine(moneyCount);
                string format = "{0} <size=50%><color=#A39A88>/ " + config.cashTarget + "</color></size>";
                moneyCount = StartCoroutine(Juice.CountUp(hudMoneyText, shownMoney, money, 0.5f, format));
                shownMoney = money;
            }
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
            if (floatTextPrefab != null) StartCoroutine(Juice.FloatText(floatTextPrefab, hudMoneyText.rectTransform, "-" + config.ante)); // TEMP: null guard until Phase 2 wires the scene

            payAnteButton.interactable = false;
            if (aiController != null) aiController.Ante(); // TEMP: null guard until Phase 2 wires the scene
            UpdatePanel();

            if (playerLucky == 0)
                luckyNumberPanel.SetActive(true);
            else
                StartCoroutine(AIBets());
        }

        public void OnPlayerBet(int number)
        {
            playerBet = number;
            betPanel.SetActive(false);
            UpdatePanel();

            cleanButton.interactable = true;
            loadedButton.interactable = true;
            cleanButton.gameObject.SetActive(true);
            loadedButton.gameObject.SetActive(true);
            cleanDie.gameObject.SetActive(true);
            loadedDie.gameObject.SetActive(true);
            rollResultText.text = "";
            if (cleanDie != null) cleanDie.Idle(); // TEMP: null guard until Phase 2 wires the scene
            if (loadedDie != null) loadedDie.Idle(); // TEMP: null guard until Phase 2 wires the scene
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
            if (aiController != null) aiController.DimAll(true); // TEMP: null guard until Phase 2 wires the scene
            DieRoller die = usedLoaded ? loadedDie : cleanDie;
            // hide the set not picked while the chosen die rolls
            (usedLoaded ? cleanButton : loadedButton).gameObject.SetActive(false);
            (usedLoaded ? cleanDie : loadedDie).gameObject.SetActive(false);
            if (die != null) yield return die.Roll(rng, rolledNumber); // TEMP: null guard until Phase 2 wires the scene
            bool jackpot = rolledNumber == playerLucky;
            if (die != null) Land(die.transform, jackpot ? 14f : 6f); // TEMP: null guard until Phase 2 wires the scene // one shake at a time, two would drift the table
            if (jackpot) StartCoroutine(Jackpot());
            rollResultText.text = $"Rolled: {rolledNumber}";
            yield return new WaitForSeconds(showResultAfterTime);
            diePanel.SetActive(false);

            int before = money;
            ResolvePayout();
            bool won = money > before;
            if (aiController != null) yield return aiController.Payout(rolledNumber, aiBets, won ? hudMoneyText.rectTransform : null); // TEMP: null guard until Phase 2 wires the scene
            if (won && floatTextPrefab != null) StartCoroutine(Juice.FloatText(floatTextPrefab, hudMoneyText.rectTransform, "+" + (money - before)));
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
            ShowStreak();
            rollsLeft--;
            UpdatePanel();

            EndTurn();
        }

        // punch the die, shake the table, dust + flash where it landed
        private void Land(Transform die, float shake)
        {
            StartCoroutine(Juice.Punch(die, 0.25f));
            if (table != null) StartCoroutine(Juice.Shake(table, shake, 0.3f)); // TEMP: null guard until Phase 2 wires the scene
            if (landDust == null || landFlash == null) return; // TEMP: null guard until Phase 2 wires the scene
            landDust.transform.position = die.position;
            landFlash.transform.position = die.position;
            landDust.Play();
            landFlash.Play();
        }

        // rolled the player's lucky number
        private IEnumerator Jackpot()
        {
            if (jackpotText == null || confetti == null || flashOverlay == null) yield break; // TEMP: null guard until Phase 2 wires the scene
            jackpotText.text = $"LUCKY {rolledNumber}!";
            jackpotText.gameObject.SetActive(true);
            confetti.Play();
            StartCoroutine(Juice.Flash(flashOverlay, jackpotFlash, 0.4f));
            yield return Juice.Punch(jackpotText.transform, 0.6f, 0.3f);
            yield return new WaitForSeconds(1.2f);
            jackpotText.gameObject.SetActive(false);
        }

        // loaded lucky hits in a row. Shown from 2, each hit also raises suspicion more (StreakRise)
        private void ShowStreak()
        {
            if (streakText == null || streakBurst == null) return; // TEMP: null guard until Phase 2 wires the scene
            streakText.gameObject.SetActive(streak >= 2);
            if (streak < 2) return;
            streakText.text = $"HOT x{streak}";
            StartCoroutine(Juice.Punch(streakText.transform));
            streakBurst.Play();
        }

        private void ResolvePayout()
        {
            int[] bets = { playerBet, aiBets[0], aiBets[1], aiBets[2] };

            PotResult result = DiceMath.ResolvePot(rolledNumber, bets, pot, config.ante * bets.Length, playerLucky, playerSecond, config.luckyMultiplier, config.secondMultiplier);

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