using System;
using System.Collections.Generic;

public readonly struct PotResult
{
    public readonly int PlayerPayout; // how much the player wins
    public readonly int Carryover;    // lefotver in cases of nobody wining or uneven split pot
    public readonly int WinnerCount;  // > 1 = tie

    public PotResult(int playerPayout, int carryover, int winnerCount)
    {
        PlayerPayout = playerPayout;
        Carryover = carryover;
        WinnerCount = winnerCount;
    }
}

// All dice, lucky number, and pot math happens here
// Pass in a seeded Random, any bugged results are repeatable
// use c# rng, not unity, to maintain ability to unit test
public static class DiceMath
{
    public const int Faces = 6;

    public static int RollClean(Random rng)
    {
        return rng.Next(1, Faces + 1);
    }

    //loadedChance increases luckyNumber, otherwise uniform over the other faces.
    public static int RollLoaded(Random rng, int luckyNumber, float loadedChance)
    {
        if (rng.NextDouble() < loadedChance) return luckyNumber;
        return RollOtherFace(rng, luckyNumber); // missed the load
    }

    // Picks one of the 5 faces other than skip, evenly.
    // Roll 1..5, then shift anything at or above skip up by one, so skip is skipped.
    // Used by RollLoaded (skip param value should player's lucky) and PickAiBet (skip param value should AI's own lucky).
    private static int RollOtherFace(Random rng, int skip)
    {
        int roll = rng.Next(1, Faces);
        return roll >= skip ? roll + 1 : roll; //returns face value of the roll 1-6
    }

    // unique numbers per AI, none should match to the players
    public static int[] AssignAiLuckyNumbers(Random rng, int playerLucky, int aiCount)
    {
        var freeNums = new List<int>();
        for (int n = 1; n <= Faces; n++)
        {
            if (n != playerLucky) freeNums.Add(n);
        }

        // Shuffle the free numbers randomly, then give the first aiCount to the AIs.
        for (int i = freeNums.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (freeNums[i], freeNums[j]) = (freeNums[j], freeNums[i]);
        }
        return freeNums.GetRange(0, aiCount).ToArray();
    }
    
    //number that were not chosen as 'lucky numbers'
    public static int[] OpenNumbers(int playerLucky, int[] aiLucky)
    {
        var open = new List<int>();
        for (int n = 1; n <= Faces; n++)
        {
            if (n != playerLucky && Array.IndexOf(aiLucky, n) < 0) open.Add(n);
        }
        return open.ToArray();
    }

    //mostly bets their lucky number, sometimes a random other face (never their own).
    public static int PickAiBet(Random rng, int aiLucky, float randomBetChance)
    {
        // Random bet: passing aiLucky as skip is what stops the AI randomly picking its own number.
        if (rng.NextDouble() < randomBetChance) return RollOtherFace(rng, aiLucky);
        return aiLucky;
    }

    // Lifelike AI on top of the plain bet.
    // Copycat: may bet the player's lucky number after the player wins on it. heat is 1 right after that win, halves each roll.
    // Chaser: may bet the last rolled number (gambler's fallacy). Pass chaserChance 0 for AIs that don't chase, lastRoll 0 = none yet.
    public static int PickAiBet(Random rng, int aiLucky, float randomBetChance, int playerLucky,
        float copycatChance, float heat, float chaserChance, int lastRoll)
    {
        int bet = PickAiBet(rng, aiLucky, randomBetChance);
        if (rng.NextDouble() < copycatChance * heat) return playerLucky;
        if (lastRoll > 0 && rng.NextDouble() < chaserChance) return lastRoll;
        return bet;
    }

    // How hot the copycats are for the next roll: 1 after the player wins on their lucky number, else it halves.
    public static float NextHeat(float heat, int rolledNum, int playerLucky, int playerPayout) =>
        rolledNum == playerLucky && playerPayout > 0 ? 1f : heat * 0.5f;

    // bets[0] is the player, bets[1..] are the AIs.
    // The player's second number pays whenever it's rolled, even if they bet something else (backup lucky number).
    // multiple winners = split pot / winnerCount, remainder carries
    // no winner = whole pot carries over
    // Player's share is multiplied on a lucky (or second) number hit; the extra comes from the house.
    // pot = carryover + this roll's antes, the caller builds it each time (no persistent game state)
    public static PotResult ResolvePot(int rolledNum, int[] bets, int pot,
        int playerLuckyNum, int? playerSecond, float luckyMultiplier, float secondMultiplier)
    {
        bool playerWins = bets[0] == rolledNum || rolledNum == playerSecond;
        int winners = playerWins ? 1 : 0;
        for (int i = 1; i < bets.Length; i++)
        {
            if (bets[i] == rolledNum) winners++;
        }
        if (winners == 0) return new PotResult(0, pot, 0);

        // AI shares just leave the pot (AI money isn't tracked). Only the remainder carries.
        int share = pot / winners;
        int carryover = pot % winners;
        if (!playerWins) return new PotResult(0, carryover, winners);

        float multiplier = 1f;
        if (rolledNum == playerLuckyNum) multiplier = luckyMultiplier;
        else if (rolledNum == playerSecond) multiplier = secondMultiplier;

        // rounds down in the case of remainders from splitting
        return new PotResult((int)(share * multiplier), carryover, winners);
    }

    // Suspicion runs 0..1, caught at 1, it carries between tables
    public static float AddSuspicion(float suspicion, bool usedLoaded, int rolledNum, int luckyNum, float perLuckyRoll)
    {
        if (usedLoaded && rolledNum == luckyNum) suspicion += perLuckyRoll;
        return Math.Clamp(suspicion, 0f, 1f);
    }

    // Hot streak: every loaded lucky hit in a row makes the next one more suspicious. streak = hits in a row so far
    public static float StreakRise(float perLuckyRoll, int streak, float streakBonus) =>
        perLuckyRoll * (1f + streakBonus * streak);

    public static bool IsCaught(float suspicion) => suspicion >= 1f;

    // Bribe the Dealer
    public static float LowerSuspicion(float suspicion, float amount) => Math.Clamp(suspicion - amount, 0f, 1f);
}
