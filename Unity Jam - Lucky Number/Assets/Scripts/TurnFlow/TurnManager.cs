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
        [SerializeField] private Button leaveButton;   // shown between turns once you're over the target
        [SerializeField] private RectTransform anteShine; // stripe swept across Pay Ante, under a RectMask2D
        [SerializeField] private Button cleanButton;
        [SerializeField] private Button loadedButton;
        [SerializeField] private TMP_Text loadedRiskText; // Loaded subtitle: the suspicion a hit would add
        [SerializeField] private GameObject luckyNumberPanel;
        [SerializeField] private GameObject betPanel;
        [SerializeField] private ItemTray itemTray;         // on betPanel, one-time item buttons
        [SerializeField] private GameObject diePanel;       // Clean / Loaded buttons + their dice
        [SerializeField] private TMP_Text rollResultText;
        [SerializeField] private TMP_Text anteValueText;
        [SerializeField] private TMP_Text hudMoneyText;
        [SerializeField] private Image moneyBar;            // wallet fill toward this table's target
        [SerializeField] private DieRoller cleanDie;        // white die above the Clean button
        [SerializeField] private DieRoller loadedDie;       // red die above the Loaded button
        [SerializeField] private AiController aiController;
        [SerializeField] private TMP_Text luckyLabel;       // "?" until the lucky die shows
        [SerializeField] private Image luckyDie;            // lucky die face, like the AI seats
        [SerializeField] private Image secondDie;           // Second Number item, beside luckyDie
        [SerializeField] private Sprite[] luckyFaces;       // UI dice faces 1-6
        [SerializeField] private TMP_Text betLabel;
        [SerializeField] private TMP_Text potLabel;         // "POT $40" at the top of the table
        [SerializeField] private int seed = 0;
        [SerializeField] private float showResultAfterTime = 1f;

        [Header("Juice")]
        [SerializeField] private RectTransform table;       // shaken on die landing and jackpot
        [SerializeField] private TMP_Text jackpotText;      // "LUCKY 4!", off until a jackpot
        [SerializeField] private float jackpotTime = 2.4f;
        [SerializeField] private TMP_Text potText;          // "POT CARRIES OVER", at the pot when nobody wins
        [SerializeField] private VfxPlayer confetti;        // Confetti_Burst_Gold
        [SerializeField] private VfxPlayer landDust;        // Land_Dust_Stone, moved to the die that rolled
        [SerializeField] private TMP_Text floatTextPrefab;  // "+60" / "-10" near the money
        [SerializeField] private Color luckyGlow = new Color(1f, 0.85f, 0.55f);
        [SerializeField] private TMP_Text streakText;       // "HOT x3", loaded lucky hits in a row 
        [SerializeField] private VfxPlayer streakBurst;     // Star_Burst_Yellow
        [SerializeField] private GameObject bannerPanel;    // table intro 
        [SerializeField] private TMP_Text bannerText;
        [SerializeField] private float bannerTime = 3.2f;

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

            leaveButton.onClick.AddListener(LeaveTable);
            StartCoroutine(Juice.Shine(anteShine, payAnteButton));

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

            StartCoroutine(Juice.Pulse(luckyDie, luckyGlow));
            StartCoroutine(Juice.Pulse(secondDie, luckyGlow));
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
            aiController.ClearLucky();
            luckyLabel.text = "?";
            luckyDie.gameObject.SetActive(false);
            ShowLuckyPair(false);
            betLabel.text = "?";
            anteValueText.text = "-" + config.ante.ToString();
            aiController.Ante();

            luckyNumberPanel.SetActive(false);
            betPanel.SetActive(false);
            diePanel.SetActive(false);
            payAnteButton.interactable = true;
            leaveButton.gameObject.SetActive(false);
            streakText.gameObject.SetActive(false);
            jackpotText.gameObject.SetActive(false);

            UpdatePanel();
            StartCoroutine(Banner());
        }

        private IEnumerator Banner()
        {
            bannerText.text = $"TABLE {tableIndex + 1}\n<size=60%>Make ${config.cashTarget} in {config.rollsPerTable} rolls</size>";
            bannerPanel.SetActive(true);
            payAnteButton.interactable = false; // no ante under the banner, it would sit over the lucky panel
            yield return Juice.Punch(bannerPanel.transform, 0.3f, 0.25f);
            yield return new WaitForSeconds(bannerTime);
            bannerPanel.SetActive(false);
            payAnteButton.interactable = true;
        }

        public void OnPlayerPickedLucky(int number)
        {
            luckyNumberPanel.SetActive(false);

            if (pickingSecond)
            {
                pickingSecond = false;
                playerSecond = number;
                secondDie.sprite = luckyFaces[number - 1];
                ShowLuckyPair(true);
                ShowSecondPick(false);
                StartCoroutine(AIBets());
                return;
            }

            playerLucky = number;
            playerSecond = null;
            luckyDie.sprite = luckyFaces[number - 1];
            luckyDie.gameObject.SetActive(true);
            luckyLabel.text = ""; // the die shows it
            aiLucky = DiceMath.AssignAiLuckyNumbers(rng, playerLucky, AiCount);
            openNumbers = DiceMath.OpenNumbers(playerLucky, aiLucky);
            aiController.SetLucky(aiLucky);

            if (owned.Contains(Item.SecondNumber))
            {
                pickingSecond = true;
                ShowSecondPick(true);
                luckyNumberPanel.SetActive(true);
                return;
            }

            StartCoroutine(AIBets());
        }

        // One die centred, or the lucky + second dice side by side, centred as a pair
        private void ShowLuckyPair(bool pair)
        {
            secondDie.gameObject.SetActive(pair);
            RectTransform die = luckyDie.rectTransform;
            die.anchoredPosition = new Vector2(pair ? -secondDie.rectTransform.anchoredPosition.x : 0f, die.anchoredPosition.y);
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
            yield return aiController.TakeBets(rng, aiBets, playerLucky);
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
                string format = "{0} <size=70%><color=#A39A88>/ " + config.cashTarget + "</color></size>";
                moneyCount = StartCoroutine(Juice.CountUp(hudMoneyText, shownMoney, money, 0.5f, format));
                shownMoney = money;
            }
            potLabel.text = $"POT <color=#E58929>${pot}</color>";
            moneyBar.fillAmount = Mathf.Clamp01((float)money / config.cashTarget);
            infoPanel.Refresh(money, pot, rollsLeft, config.cashTarget, tableIndex + 1, suspicionMeter.Value);
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
            StartCoroutine(Juice.FloatText(floatTextPrefab, hudMoneyText.rectTransform, "-" + config.ante));

            payAnteButton.interactable = false;
            betLabel.text = "?"; // like the AI chips, cleared each ante
            aiController.Ante();
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
            float risk = rabbitsFoot ? 0f : DiceMath.StreakRise(config.suspicionPerLuckyRoll, streak, config.streakBonus);
            loadedRiskText.text = $"draws eyes: +{Mathf.RoundToInt(risk * 100f)}% on a hit";
            cleanDie.Idle();
            loadedDie.Idle();
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
            aiController.DimAll(true);
            DieRoller die = usedLoaded ? loadedDie : cleanDie;
            // hide the set not picked while the chosen die rolls
            (usedLoaded ? cleanButton : loadedButton).gameObject.SetActive(false);
            (usedLoaded ? cleanDie : loadedDie).gameObject.SetActive(false);
            yield return die.Roll(rng, rolledNumber);
            bool jackpot = rolledNumber == playerLucky;
            Land(die.transform, jackpot ? 14f : 6f); // one shake at a time, two would drift the table
            if (jackpot) StartCoroutine(Jackpot());
            rollResultText.text = jackpot ? "" : $"Rolled: {rolledNumber}"; // the jackpot text says it
            yield return new WaitForSeconds(showResultAfterTime);
            diePanel.SetActive(false);

            int before = money;
            ResolvePayout();
            bool won = money > before;
            yield return aiController.Payout(rolledNumber, aiBets, won ? hudMoneyText.rectTransform : null);
            if (won) StartCoroutine(Juice.FloatText(floatTextPrefab, hudMoneyText.rectTransform, "+" + (money - before)));
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
            StartCoroutine(Juice.Shake(table, shake, 0.3f));
            landDust.transform.position = die.position;
            landDust.Play();
        }

        // rolled the player's lucky number
        private IEnumerator Jackpot()
        {
            jackpotText.text = $"LUCKY {rolledNumber}!";
            jackpotText.gameObject.SetActive(true);
            confetti.Play();
            yield return Juice.Punch(jackpotText.transform, 0.6f, 0.3f);
            yield return new WaitForSeconds(jackpotTime);
            jackpotText.gameObject.SetActive(false);
        }

        // nobody hit: the pot stays on the table and grows next roll
        private IEnumerator PotCarries()
        {
            potText.text = $"POT CARRIES OVER\n<size=140%><color=#E58929>${pot}</color></size>";
            potText.gameObject.SetActive(true);
            yield return Juice.Punch(potText.transform, 0.4f, 0.25f);
            yield return new WaitForSeconds(1.6f);
            potText.gameObject.SetActive(false);
        }

        // loaded lucky hits in a row. Shown from 2, each hit also raises suspicion more (StreakRise)
        private void ShowStreak()
        {
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
            {
                rollResultText.text = $"Rolled: {rolledNumber} - No win, pot carries over";
                StartCoroutine(PotCarries());
            }
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