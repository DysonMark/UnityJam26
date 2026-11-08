using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.Rendering;

public class DiceTests
{
    private const int Rolls = 60000;
    private const double Tolerance = 0.01;

    // Fraction of rolls that landed on each face. base 1 array, so index 0 is unused, faces[3] means a 3.
    private static double[] Frequencies(Func<int> roll)
    {
        var faces = new double[DiceMath.Faces + 1];
        for (int i = 0; i < Rolls; i++) faces[roll()]++;
        for (int n = 1; n <= DiceMath.Faces; n++) faces[n] /= Rolls;
        return faces;
    }

    [Test]
    public void RollClean_AlwaysInRange()
    {
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
        {
            Assert.That(DiceMath.RollClean(rng), Is.InRange(1, 6));
        }
    }

    [Test]
    public void RollClean_FacesAreEven()
    {
        var rng = new Random(42);
        var faces = Frequencies(() => DiceMath.RollClean(rng));
        for (int n = 1; n <= DiceMath.Faces; n++)
        {
            Assert.That(faces[n], Is.EqualTo(1.0 / 6).Within(Tolerance), $"face {n}");
        }
    }

    [Test]
    public void RollLoaded_HitsLuckyAtLoadedChance_OtherFacesEven()
    {
        var rng = new Random(42);
        const int lucky = 4;
        const float chance = 0.5f;
        var faces = Frequencies(() => DiceMath.RollLoaded(rng, lucky, chance));

        Assert.That(faces[lucky], Is.EqualTo(chance).Within(Tolerance), "lucky face");
        for (int n = 1; n <= DiceMath.Faces; n++)
        {
            if (n == lucky) continue;
            Assert.That(faces[n], Is.EqualTo((1 - chance) / 5).Within(Tolerance), $"face {n}");
        }
    }

    [Test]
    public void RollLoaded_ZeroChance_NeverLucky()
    {
        var rng = new Random(42);
        for (int i = 0; i < 10000; i++)
        {
            int roll = DiceMath.RollLoaded(rng, 6, 0f);
            Assert.That(roll, Is.InRange(1, 5));
        }
    }

    [Test]
    public void RollLoaded_FullChance_AlwaysLucky()
    {
        var rng = new Random(42);
        const int lucky = 3;
        const float chance = 1f;
        for (int i = 0; i < 10000; i++)
        {
            int roll = DiceMath.RollLoaded(rng, lucky, chance);
            Assert.That(roll, Is.EqualTo(3));
        }
    }

    [Test]
    public void SameSeed_SameSequence()
    {
        // To fail the test, change the seed number.

        var rng = new Random(42);
        var sameRng = new Random(42);
        const int lucky = 2;
        const float chance = 0.5f;

        for (int i = 0; i < 10000; i++)
        {
            int roll = DiceMath.RollLoaded(rng, lucky, chance);
            int secondRoll = DiceMath.RollLoaded(sameRng, lucky, chance);
            Assert.That(roll, Is.EqualTo(secondRoll));
        }

    }

    [Test]
    public void AssignAiLuckyNumbers_UniqueAndNotPlayers()
    {
        var rng = new Random(42);
        for (int player = 1; player <= DiceMath.Faces; player++)
        {
            for (int i = 0; i < 100; i++)
            {
                int[] ai = DiceMath.AssignAiLuckyNumbers(rng, player, 3);
                Assert.That(ai, Has.Length.EqualTo(3));
                Assert.That(ai, Is.Unique);
                Assert.That(ai, Has.No.Member(player));
                Assert.That(ai, Is.All.InRange(1, DiceMath.Faces));
            }
        }
    }

    [Test]
    public void PickAiBet_RandomBetsAtChance_NeverOwnNumber()
    {
        var rng = new Random(42);
        const int aiLucky = 2;
        const float chance = 0.15f;
        int away = 0;
        for (int i = 0; i < Rolls; i++)
        {
            if (DiceMath.PickAiBet(rng, aiLucky, chance) != aiLucky) away++;
        }
        // If random bets could land on aiLucky, this would come out near 0.125 (0.15 * 5/6) and fail.
        Assert.That((double)away / Rolls, Is.EqualTo(chance).Within(Tolerance));
    }

    [Test]
    public void OpenNumbers_TwoLeftWithThreeAi()
    {
        int[] aiLuckyNumbers = { 2, 3, 4 };
        const int playerLucky = 1;

        var open = DiceMath.OpenNumbers(playerLucky, aiLuckyNumbers).ToList();

        Assert.That(open, Has.Count.EqualTo(2));

        Assert.That(open, Has.No.Member(playerLucky));
        foreach(int ai in aiLuckyNumbers)
        {
            Assert.That(open, Has.No.Member(ai));
        }

    }

    [Test]
    public void PickAiBet_ZeroChance_AlwaysOwnNumber()
    {
        var rng = new Random(67);

        const int aiLuckyNum = 6;
        var bet = DiceMath.PickAiBet(rng, aiLuckyNum, 0);

        Assert.That(bet, Is.EqualTo(aiLuckyNum));

    }

    // Player's lucky is 3, second is null unless a test needs it.
    private const int PlayerLucky = 3;

    [Test]
    public void ResolvePot_SoloLuckyWin_PaysPotTimesMultiplier()
    {
        int[] bets = { 3, 1, 2, 4 };
        var result = DiceMath.ResolvePot(3, bets, 40, PlayerLucky, null, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(80));
        Assert.That(result.Carryover, Is.EqualTo(0));
        Assert.That(result.WinnerCount, Is.EqualTo(1));
    }

    [Test]
    public void ResolvePot_UnevenSplit_RemainderCarriesOver()
    {
        // Open number 5, player + 2 AIs bet it: 50 / 3 = 16 each, 2 left over.
        int[] bets = { 5, 5, 5, 4 };
        var result = DiceMath.ResolvePot(5, bets, 50, PlayerLucky, null, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(16));
        Assert.That(result.Carryover, Is.EqualTo(2));
        Assert.That(result.WinnerCount, Is.EqualTo(3));
    }

    [Test]
    public void ResolvePot_NoWinner_WholePotCarries()
    {
        int[] bets = { 3, 1, 2, 4 };
        var result = DiceMath.ResolvePot(6, bets, 40, PlayerLucky, null, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(0));
        Assert.That(result.Carryover, Is.EqualTo(40));
        Assert.That(result.WinnerCount, Is.EqualTo(0));
    }

    [Test]
    public void ResolvePot_SecondNumberWin_PaysSecondMultiplier()
    {
        // Second number 5 is an open number, only the player bet it.
        int[] bets = { 5, 1, 2, 4 };
        var result = DiceMath.ResolvePot(5, bets, 40, PlayerLucky, 5, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(60));
        Assert.That(result.Carryover, Is.EqualTo(0));
        Assert.That(result.WinnerCount, Is.EqualTo(1));
    }

    [Test]
    public void ResolvePot_OnlyAiWins_PlayerGetsNothing_NothingCarries()
    {
        int[] bets = { 3, 1, 2, 4 };
        var result = DiceMath.ResolvePot(1, bets, 40, PlayerLucky, null, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(0));
        Assert.That(result.Carryover, Is.EqualTo(0));
        Assert.That(result.WinnerCount, Is.EqualTo(1));
    }

    [Test]
    public void AddSuspicion_OnlyLoadedLuckyHitRaisesIt()
    {
        Assert.That(DiceMath.AddSuspicion(0f, true, PlayerLucky, PlayerLucky, 0.2f), Is.EqualTo(0.2f).Within(1e-5));
        Assert.That(DiceMath.AddSuspicion(0f, false, PlayerLucky, PlayerLucky, 0.2f), Is.EqualTo(0f), "clean hit");
        Assert.That(DiceMath.AddSuspicion(0f, true, PlayerLucky + 1, PlayerLucky, 0.2f), Is.EqualTo(0f), "loaded miss");
    }

    [Test]
    public void AddSuspicion_CaughtAfterFiveHits_CapsAtOne()
    {
        float s = 0f;
        for (int i = 0; i < 4; i++) s = DiceMath.AddSuspicion(s, true, PlayerLucky, PlayerLucky, 0.2f);
        Assert.That(DiceMath.IsCaught(s), Is.False, "4 hits");
        s = DiceMath.AddSuspicion(s, true, PlayerLucky, PlayerLucky, 0.2f);
        Assert.That(DiceMath.IsCaught(s), Is.True, "5 hits");
        s = DiceMath.AddSuspicion(s, true, PlayerLucky, PlayerLucky, 0.2f);
        Assert.That(s, Is.EqualTo(1f), "capped");
    }

    [Test]
    public void AddSuspicion_BuildsOnCarriedValue()
    {
        // Suspicion carries between tables, so a new table starts from the old value, not 0.
        float carried = 0.6f;
        Assert.That(DiceMath.AddSuspicion(carried, true, PlayerLucky, PlayerLucky, 0.2f), Is.EqualTo(0.8f).Within(1e-5));
        Assert.That(DiceMath.AddSuspicion(carried, false, 1, PlayerLucky, 0.2f), Is.EqualTo(carried), "no hit, unchanged");
    }

    [Test]
    public void Shop_Pick_ThreeUnique_AtLeastOneOfEachType()
    {
        var rng = new Random(42);
        for (int i = 0; i < 1000; i++)
        {
            Item[] picks = Shop.Pick(rng);
            Assert.That(picks, Has.Length.EqualTo(Shop.Offered));
            Assert.That(picks, Is.Unique);
            Assert.That(Array.Exists(picks, Shop.IsOneTime), "no one-time item");
            Assert.That(Array.Exists(picks, item => !Shop.IsOneTime(item)), "no permanent item");
        }
    }

    [Test]
    public void Shop_Pick_EveryItemShowsUp()
    {
        var rng = new Random(42);
        var seen = new bool[Shop.ItemCount];
        for (int i = 0; i < 1000; i++)
        {
            foreach (Item item in Shop.Pick(rng)) seen[(int)item] = true;
        }
        Assert.That(seen, Is.All.True);
    }

    [Test]
    public void Shop_ItemData_MatchesEnum()
    {
        // Names, descriptions, and prices are indexed by (int)item, so a missed entry would shift every item after it.
        var cfg = UnityEngine.ScriptableObject.CreateInstance<TuningConfig>();
        Assert.That(Shop.Names, Has.Length.EqualTo(Shop.ItemCount));
        Assert.That(Shop.Descriptions, Has.Length.EqualTo(Shop.ItemCount));
        Assert.That(cfg.itemPrices, Has.Length.EqualTo(Shop.ItemCount));
        UnityEngine.Object.DestroyImmediate(cfg);
    }

    [Test]
    public void WithItems_HeavierLoad_RaisesLoadedChance_TableUnchanged()
    {
        var table = UnityEngine.ScriptableObject.CreateInstance<TuningConfig>();
        var cfg = Shop.WithItems(table, new[] { Item.HeavierLoad });

        Assert.That(cfg.loadedChance, Is.EqualTo(table.loadedChance + table.heavierLoadBonus).Within(0.0001f));
        Assert.That(table.loadedChance, Is.EqualTo(0.5f), "the table's own config should not change");

        UnityEngine.Object.DestroyImmediate(cfg);
        UnityEngine.Object.DestroyImmediate(table);
    }

    [Test]
    public void WithItems_HeavierLoad_StacksButCapsAtOne()
    {
        var table = UnityEngine.ScriptableObject.CreateInstance<TuningConfig>();
        var cfg = Shop.WithItems(table, new[] { Item.HeavierLoad, Item.HeavierLoad, Item.HeavierLoad, Item.HeavierLoad });

        Assert.That(cfg.loadedChance, Is.EqualTo(1f));

        UnityEngine.Object.DestroyImmediate(cfg);
        UnityEngine.Object.DestroyImmediate(table);
    }

    [Test]
    public void WithItems_HighRoller_RaisesLuckyMultiplier_OnlyThat()
    {
        var table = UnityEngine.ScriptableObject.CreateInstance<TuningConfig>();
        var cfg = Shop.WithItems(table, new[] { Item.HighRoller });

        Assert.That(cfg.luckyMultiplier, Is.EqualTo(table.luckyMultiplier + table.highRollerBonus).Within(0.0001f));
        Assert.That(cfg.loadedChance, Is.EqualTo(table.loadedChance));
        Assert.That(cfg.secondMultiplier, Is.EqualTo(table.secondMultiplier));

        UnityEngine.Object.DestroyImmediate(cfg);
        UnityEngine.Object.DestroyImmediate(table);
    }

    // Simulator: run by hand from the Test Runner, results go to the console.
    // Uses the real tuning asset, so it balances the numbers the game actually uses.
    // TODO add suspicion logic, TODO add in item logic
    [Test, Explicit]
    public void Simulate()
    {
        const int tables = 5000;
        var cfg = UnityEditor.AssetDatabase.LoadAssetAtPath<TuningConfig>(
            "Assets/ScriptableObjects/TuningConfig-Default.asset");
        Assert.That(cfg, Is.Not.Null, "TuningConfig-Default.asset not found");

        UnityEngine.Debug.Log($"Simulating {tables} tables: start {cfg.startingMoney}, target {cfg.cashTarget}, " +
                              $"ante {cfg.ante}, {cfg.rollsPerTable} rolls, loadedChance {cfg.loadedChance}");

        SimulateStrategy(cfg, tables, false, false, "Clean, own lucky ");
        SimulateStrategy(cfg, tables, true, false, "Loaded, own lucky");
        SimulateStrategy(cfg, tables, false, true, "Clean, open number");
        SimulateStrategy(cfg, tables, false, true, "Clean, second number", hasSecond: true);
        // TODO no balance assert yet. Add "clean reaches target < 5%" once suspicion is in and the numbers are tuned.
    }

    // Plays many tables with one strategy, logs the results.
    private static void SimulateStrategy(TuningConfig cfg, int tables, bool loaded, bool betOpen, string name,
        bool hasSecond = false)
    {
        var rng = new Random(42);
        int reached = 0, broke = 0;
        long totalMoney = 0;
        for (int t = 0; t < tables; t++)
        {
            var (money, hitTarget) = PlayTable(rng, cfg, loaded, betOpen, hasSecond);
            if (hitTarget) reached++;
            else if (money < cfg.ante) broke++;
            totalMoney += money;
        }
        UnityEngine.Debug.Log($"{name}: reached {(double)reached / tables:P1}, broke {(double)broke / tables:P1}, " +
                              $"avg money {(double)totalMoney / tables:F0}");
    }

    // One table: ante, bet, roll, pay out, until out of rolls, broke, or target hit.
    // hasSecond = player owns Second Number: the open number they bet on is their second, paying secondMultiplier.
    private static (int money, bool reachedTarget) PlayTable(Random rng, TuningConfig cfg, bool loaded, bool betOpen,
        bool hasSecond)
    {
        int playerLucky = rng.Next(1, DiceMath.Faces + 1);
        int[] aiLucky = DiceMath.AssignAiLuckyNumbers(rng, playerLucky, 3);
        int playerBet = betOpen ? DiceMath.OpenNumbers(playerLucky, aiLucky)[0] : playerLucky;
        int? playerSecond = hasSecond ? DiceMath.OpenNumbers(playerLucky, aiLucky)[0] : (int?)null;

        int money = cfg.startingMoney;
        int carryover = 0;
        var bets = new int[aiLucky.Length + 1];
        for (int roll = 0; roll < cfg.rollsPerTable; roll++)
        {
            if (money < cfg.ante) return (money, false);
            money -= cfg.ante;
            int pot = carryover + cfg.ante * bets.Length;

            bets[0] = playerBet;
            for (int i = 0; i < aiLucky.Length; i++)
                bets[i + 1] = DiceMath.PickAiBet(rng, aiLucky[i], cfg.aiRandomBetChance);

            int rolled = loaded
                ? DiceMath.RollLoaded(rng, playerLucky, cfg.loadedChance)
                : DiceMath.RollClean(rng);

            var result = DiceMath.ResolvePot(rolled, bets, pot, playerLucky, playerSecond,
                cfg.luckyMultiplier, cfg.secondMultiplier);
            money += result.PlayerPayout;
            carryover = result.Carryover;

            if (money >= cfg.cashTarget) return (money, true);
        }
        return (money, false);
    }
}
