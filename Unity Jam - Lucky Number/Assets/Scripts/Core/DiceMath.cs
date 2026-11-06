using System;
using System.Collections.Generic;

namespace UnityJam.LuckyNumber.Core
{
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
            // TODO - math, return a regular clean roll as a placeholder
            return RollClean(rng);
        }

        // unique numbers per AI, none should match to the players
        public static int[] AssignAiLuckyNumbers(Random rng, int playerLucky, int aiCount)
        {
            // TODO - math, return lowest free numbers as a placeholder.
            var result = new int[aiCount];
            int next = 1;
            for (int i = 0; i < aiCount; i++)
            {
                if (next == playerLucky) next++;
                result[i] = next++;
            }
            return result;
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

        //mostly bets their lucky number, sometimes a random face.
        public static int PickAiBet(Random rng, int aiLucky, float randomBetChance)
        {
            // TODO - math, returns lucjy number as a placeholder.
            return aiLucky;
        }

        // bets[0] is the player, bets[1..] are the AIs.
        // multiple winners = split pot / winnerCount, remainder carries
        // no winner = whole pot carries over
        // Player's share is multiplied on a lucky nuumber hit; the extra comes from the house.
        public static PotResult ResolvePot(int rolled, int[] bets, int pot,
            int playerLucky, int? playerSecond, float luckyMultiplier, float secondMultiplier)
        {
            // TODO - math, nobody wins, pot carries over for placehlder
            return new PotResult(0, pot, 0);
        }
    }
}
