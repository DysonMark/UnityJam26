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

    // Strip picked always lands on the asked face, and with 24 strips every angle gets used
    [Test]
    public void StripIndex_RightFace_AllAngles()
    {
        var rng = new Random(42);
        for (int face = 1; face <= DiceMath.Faces; face++)
        {
            Assert.That(DiceMath.StripIndex(rng, 6, face), Is.EqualTo(face - 1));

            var angles = new HashSet<int>();
            for (int i = 0; i < 200; i++)
            {
                int index = DiceMath.StripIndex(rng, 24, face);
                Assert.That(index % DiceMath.Faces, Is.EqualTo(face - 1), $"face {face}");
                angles.Add(index / DiceMath.Faces);
            }
            Assert.That(angles.Count, Is.EqualTo(4), $"face {face}");
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

    [Test]
    public void PickAiBet_Lifelike_NoHeatNoChaser_SameAsPlain()
    {
        var rng = new Random(42);
        var sameRng = new Random(42);
        for (int i = 0; i < 1000; i++)
        {
            int plain = DiceMath.PickAiBet(rng, 2, 0.15f);
            rng.NextDouble(); rng.NextDouble(); // the copycat and chaser rolls the lifelike version makes
            Assert.That(DiceMath.PickAiBet(sameRng, 2, 0.15f, 5, 1f, 0f, 0f, 4), Is.EqualTo(plain));
        }
    }

    [Test]
    public void PickAiBet_Lifelike_FullHeatCopies_ChaserBetsLastRoll()
    {
        var rng = new Random(42);
        for (int i = 0; i < 100; i++)
        {
            Assert.That(DiceMath.PickAiBet(rng, 2, 0.15f, 5, 1f, 1f, 0f, 4), Is.EqualTo(5), "copycat");
            Assert.That(DiceMath.PickAiBet(rng, 2, 0.15f, 5, 0f, 1f, 1f, 4), Is.EqualTo(4), "chaser");
            Assert.That(DiceMath.PickAiBet(rng, 2, 0f, 5, 0f, 1f, 1f, 0), Is.EqualTo(2), "no roll yet, no chase");
        }
    }

    [Test]
    public void NextHeat_HotAfterLuckyWin_HalvesOtherwise()
    {
        Assert.That(DiceMath.NextHeat(0f, 3, 3, 40), Is.EqualTo(1f), "won on lucky");
        Assert.That(DiceMath.NextHeat(1f, 3, 3, 0), Is.EqualTo(0.5f), "lucky rolled, AI took it");
        Assert.That(DiceMath.NextHeat(1f, 5, 3, 40), Is.EqualTo(0.5f), "won on another number");
    }

    // Player's lucky is 3, second is null unless a test needs it.
    private const int PlayerLucky = 3;

    [Test]
    public void ResolvePot_SoloLuckyWin_PaysPotTimesMultiplier()
    {
        int[] bets = { 3, 1, 2, 4 };
        var result = DiceMath.ResolvePot(3, bets, 40, 40, PlayerLucky, null, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(80));
        Assert.That(result.Carryover, Is.EqualTo(0));
        Assert.That(result.WinnerCount, Is.EqualTo(1));
    }

    [Test]
    public void ResolvePot_CarriedPot_PaysOneX_OnlyAntesMultiplied()
    {
        // 200 carried + 40 antes, solo lucky hit x3: 200 + 40 * 3 = 320, not 240 * 3 = 720.
        int[] bets = { 3, 1, 2, 4 };
        var result = DiceMath.ResolvePot(3, bets, 240, 40, PlayerLucky, null, 3f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(320));
        Assert.That(result.Carryover, Is.EqualTo(0));
    }

    [Test]
    public void ResolvePot_UnevenSplit_RemainderCarriesOver()
    {
        // Open number 5, player + 2 AIs bet it: 50 / 3 = 16 each, 2 left over.
        int[] bets = { 5, 5, 5, 4 };
        var result = DiceMath.ResolvePot(5, bets, 50, 50, PlayerLucky, null, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(16));
        Assert.That(result.Carryover, Is.EqualTo(2));
        Assert.That(result.WinnerCount, Is.EqualTo(3));
    }

    [Test]
    public void ResolvePot_NoWinner_WholePotCarries()
    {
        int[] bets = { 3, 1, 2, 4 };
        var result = DiceMath.ResolvePot(6, bets, 40, 40, PlayerLucky, null, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(0));
        Assert.That(result.Carryover, Is.EqualTo(40));
        Assert.That(result.WinnerCount, Is.EqualTo(0));
    }

    [Test]
    public void ResolvePot_SecondNumberWin_PaysSecondMultiplier()
    {
        // Second number 5 is an open number, only the player bet it.
        int[] bets = { 5, 1, 2, 4 };
        var result = DiceMath.ResolvePot(5, bets, 40, 40, PlayerLucky, 5, 2f, 1.5f);

        Assert.That(result.PlayerPayout, Is.EqualTo(60));
        Assert.That(result.Carryover, Is.EqualTo(0));
        Assert.That(result.WinnerCount, Is.EqualTo(1));
    }

    [Test]
    public void ResolvePot_SecondNumberRolled_PaysEvenWhenPlayerBetLucky()
    {
        // Player bet lucky 3, second is 5, 5 rolls, AI 2 also bet 5: split 40 / 2 = 20, x1.25.
        int[] bets = { 3, 1, 5, 4 };
        var result = DiceMath.ResolvePot(5, bets, 40, 40, PlayerLucky, 5, 2f, 1.25f);

        Assert.That(result.PlayerPayout, Is.EqualTo(25));
        Assert.That(result.Carryover, Is.EqualTo(0));
        Assert.That(result.WinnerCount, Is.EqualTo(2));
    }

    [Test]
    public void ResolvePot_OnlyAiWins_PlayerGetsNothing_NothingCarries()
    {
        int[] bets = { 3, 1, 2, 4 };
        var result = DiceMath.ResolvePot(1, bets, 40, 40, PlayerLucky, null, 2f, 1.5f);

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
    public void StreakRise_EachHitInARowAddsHalfMore()
    {
        Assert.That(DiceMath.StreakRise(0.15f, 0, 0.5f), Is.EqualTo(0.15f).Within(1e-5), "first hit");
        Assert.That(DiceMath.StreakRise(0.15f, 1, 0.5f), Is.EqualTo(0.225f).Within(1e-5), "second in a row");
        Assert.That(DiceMath.StreakRise(0.15f, 2, 0.5f), Is.EqualTo(0.3f).Within(1e-5), "third in a row");
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
        Assert.That(cfg.suspicionPerLuckyRoll, Is.EqualTo(table.suspicionPerLuckyRoll + table.highRollerSuspicion).Within(0.0001f));
        Assert.That(cfg.loadedChance, Is.EqualTo(table.loadedChance));
        Assert.That(cfg.secondMultiplier, Is.EqualTo(table.secondMultiplier));

        UnityEngine.Object.DestroyImmediate(cfg);
        UnityEngine.Object.DestroyImmediate(table);
    }

    [Test]
    public void WithItems_SteadyHands_LowersSuspicionPerHit_NotBelowZero()
    {
        var table = UnityEngine.ScriptableObject.CreateInstance<TuningConfig>();
        var cfg = Shop.WithItems(table, new[] { Item.SteadyHands });
        Assert.That(cfg.suspicionPerLuckyRoll, Is.EqualTo(table.suspicionPerLuckyRoll - table.steadyHandsCut).Within(0.0001f));
        UnityEngine.Object.DestroyImmediate(cfg);

        table.steadyHandsCut = 1f;
        cfg = Shop.WithItems(table, new[] { Item.SteadyHands });
        Assert.That(cfg.suspicionPerLuckyRoll, Is.EqualTo(0f));

        UnityEngine.Object.DestroyImmediate(cfg);
        UnityEngine.Object.DestroyImmediate(table);
    }

    [Test]
    public void WithItems_ExtraRoll_LeavesRollsAlone()
    {
        // One-time items act when the player plays them, not when the table starts
        var table = UnityEngine.ScriptableObject.CreateInstance<TuningConfig>();
        var cfg = Shop.WithItems(table, new[] { Item.ExtraRoll });

        Assert.That(cfg.rollsPerTable, Is.EqualTo(table.rollsPerTable));

        UnityEngine.Object.DestroyImmediate(cfg);
        UnityEngine.Object.DestroyImmediate(table);
    }

    [Test]
    public void LowerSuspicion_Bribe_DropsButNotBelowZero()
    {
        Assert.That(DiceMath.LowerSuspicion(0.8f, 0.4f), Is.EqualTo(0.4f).Within(1e-5));
        Assert.That(DiceMath.LowerSuspicion(0.2f, 0.4f), Is.EqualTo(0f));
    }

    // Simulator: run by hand from the Test Runner, results go to the console.
    // Plays whole runs over the real table assets like in the game: suspicion and money carry, each table's
    // startingMoney is the buy-in, what's left is shop money. Lifelike AI (copycat + chaser) is on.
    // Items: the strategy buys the same item in every shop it can afford, so it applies to tables 2 and 3.
    // Strategies leave as soon as they're over the target.
    [Test, Explicit]
    public void Simulate()
    {
        const int runs = 5000;
        var tables = new TuningConfig[3];
        for (int i = 0; i < tables.Length; i++)
        {
            tables[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<TuningConfig>(
                $"Assets/ScriptableObjects/TuningConfig-Table{i + 1}.asset");
            Assert.That(tables[i], Is.Not.Null, $"TuningConfig-Table{i + 1}.asset not found");
            var c = tables[i];
            UnityEngine.Debug.Log($"Table {i + 1}: stack {c.startingMoney}, target {c.cashTarget}, ante {c.ante}, " +
                                  $"{c.rollsPerTable} rolls, loadedChance {c.loadedChance}, suspicion/hit {c.suspicionPerLuckyRoll}");
        }
        var prices = UnityEditor.AssetDatabase.LoadAssetAtPath<TuningConfig>(
            "Assets/ScriptableObjects/TuningConfig-Default.asset").itemPrices; // the shop reads this one

        SimulateRun(tables, prices, runs, false, false, null, "Clean");
        SimulateRun(tables, prices, runs, true, false, null, "Always loaded");
        SimulateRun(tables, prices, runs, true, true, null, "Careful loaded");
        foreach (Item item in Enum.GetValues(typeof(Item)))
            SimulateRun(tables, prices, runs, true, true, item, $"Careful + {item}");
        // TODO no balance assert yet. Add "clean wins < 5%" once the numbers are tuned.
    }

    // Plays many runs with one strategy, logs how they ended.
    // careful = only load while one more suspicious hit can't get you caught.
    private static void SimulateRun(TuningConfig[] tables, int[] prices, int runs, bool loaded, bool careful,
        Item? item, string name)
    {
        var rng = new Random(42);
        int wins = 0, caught = 0, broke = 0, outOfRolls = 0, cleared = 0;
        for (int r = 0; r < runs; r++)
        {
            float suspicion = 0f;
            var owned = new Item[0];
            string outcome = "target";
            for (int t = 0; t < tables.Length && outcome == "target"; t++)
            {
                var cfg = Shop.WithItems(tables[t], owned);
                int money = cfg.startingMoney;
                outcome = PlayTable(rng, cfg, loaded, careful, owned, ref suspicion, ref money);
                UnityEngine.Object.DestroyImmediate(cfg);
                if (outcome != "target") break;
                cleared++;
                int shopMoney = t + 1 < tables.Length ? money - tables[t + 1].startingMoney : 0;
                owned = item != null && shopMoney >= prices[(int)item.Value] ? new[] { item.Value } : new Item[0];
            }
            if (outcome == "target") wins++;
            else if (outcome == "caught") caught++;
            else if (outcome == "broke") broke++;
            else outOfRolls++;
        }
        UnityEngine.Debug.Log($"{name}: win {(double)wins / runs:P1}, caught {(double)caught / runs:P1}, " +
                              $"broke {(double)broke / runs:P1}, out of rolls {(double)outOfRolls / runs:P1}, " +
                              $"avg tables cleared {(double)cleared / runs:F2}");
    }

    // One table, same order as TurnManager: ante, bet, roll, pay out, suspicion, then caught / rolls / target / broke.
    // The bot always bets its lucky number, Second Number pays on its own when rolled.
    // Returns "target", "caught", "broke" or "rolls".
    private static string PlayTable(Random rng, TuningConfig cfg, bool loaded, bool careful, Item[] owned,
        ref float suspicion, ref int money)
    {
        int playerLucky = rng.Next(1, DiceMath.Faces + 1);
        int[] aiLucky = DiceMath.AssignAiLuckyNumbers(rng, playerLucky, 3);
        int[] open = DiceMath.OpenNumbers(playerLucky, aiLucky);
        int? playerSecond = Array.IndexOf(owned, Item.SecondNumber) >= 0 ? open[rng.Next(open.Length)] : (int?)null;
        // The bot plays its one-time items right away
        if (Array.IndexOf(owned, Item.BribeTheDealer) >= 0) suspicion = DiceMath.LowerSuspicion(suspicion, cfg.bribeAmount);
        int rolls = cfg.rollsPerTable + (Array.IndexOf(owned, Item.ExtraRoll) >= 0 ? cfg.extraRolls : 0);
        bool rabbitsFoot = Array.IndexOf(owned, Item.RabbitsFoot) >= 0;

        int pot = 0;
        float heat = 0f;
        int lastRoll = 0;
        int streak = 0;
        var bets = new int[aiLucky.Length + 1];
        for (int roll = 0; roll < rolls; roll++)
        {
            money -= cfg.ante;
            pot += cfg.ante * bets.Length;

            float next = DiceMath.StreakRise(cfg.suspicionPerLuckyRoll, streak, cfg.streakBonus);
            // (float) cast: Unity can compare the sum at double precision, 0.775 + 0.225 reads as just under 1
            // there but stores as exactly 1, so the careful bot loaded into getting caught
            bool load = loaded && (!careful || rabbitsFoot || !DiceMath.IsCaught((float)(suspicion + next)));
            bets[0] = playerLucky;
            for (int i = 0; i < aiLucky.Length; i++)
                bets[i + 1] = DiceMath.PickAiBet(rng, aiLucky[i], cfg.aiRandomBetChance, playerLucky,
                    cfg.copycatChance, heat, i == 0 ? cfg.chaserChance : 0f, lastRoll);

            int rolled = load ? DiceMath.RollLoaded(rng, playerLucky, cfg.loadedChance) : DiceMath.RollClean(rng);

            var result = DiceMath.ResolvePot(rolled, bets, pot, cfg.ante * bets.Length, playerLucky, playerSecond,
                cfg.luckyMultiplier, cfg.secondMultiplier);
            money += result.PlayerPayout;
            pot = result.Carryover;
            heat = DiceMath.NextHeat(heat, rolled, playerLucky, result.PlayerPayout);
            lastRoll = rolled;

            bool loadedHit = load && rolled == playerLucky;
            float rise = next;
            if (rabbitsFoot && loadedHit)
            {
                rise = 0f;
                rabbitsFoot = false;
            }
            suspicion = DiceMath.AddSuspicion(suspicion, load, rolled, playerLucky, rise);
            if (!load) suspicion = DiceMath.LowerSuspicion(suspicion, cfg.cleanCool);
            streak = loadedHit ? streak + 1 : 0;

            if (DiceMath.IsCaught(suspicion)) return "caught";
            if (money >= cfg.cashTarget) return "target";
            if (roll == rolls - 1) return "rolls";
            if (money < cfg.ante) return "broke";
        }
        return "rolls";
    }
}
