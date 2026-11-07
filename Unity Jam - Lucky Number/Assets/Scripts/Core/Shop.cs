using System;
using System.Collections.Generic;

namespace UnityJam.LuckyNumber.Core
{
    // Permanent items first, one-time items last
    // IsOneTIme checks if index is > that BribeTheDealer, so order matters here
    public enum Item
    {
        HeavierLoad,
        SteadyHands,
        SecondNumber,
        HighRoller,
        PokerFace,
        BribeTheDealer, // first one-time item
        ExtraRoll,
        RabbitsFoot,
    }

    // Item data and the shop pick. Prices live in TuningConfig SO
    // TODO - Effects,  money/dice ones apply to a copy of TuningConfig SO and suspicion rukes
    public static class Shop
    {
        public const int Offered = 3;

        public static readonly int ItemCount = Enum.GetValues(typeof(Item)).Length;

        // Indexed by (int)item, same order as the enum.
        public static readonly string[] Names =
        {
            "Heavier Load",
            "Steady Hands",
            "Second Number",
            "High Roller",
            "Poker Face",
            "Bribe the Dealer",
            "Extra Roll",
            "Rabbit's Foot",
        };

        public static readonly string[] Descriptions =
        {
            "insert description",
            "insert description",
            "insert description",
            "insert description",
            "insert description",
            "insert description",
            "insert description",
            "insert description",
        };

        public static bool IsOneTime(Item item) => item >= Item.BribeTheDealer;

        // 3 different items, at least one permanent + one one-time, in random order
        public static Item[] Pick(Random rng)
        {
            var permanent = new List<Item>();
            var oneTime = new List<Item>();
            foreach (Item item in Enum.GetValues(typeof(Item)))
            {
                if (IsOneTime(item)) oneTime.Add(item);
                else permanent.Add(item);
            }
            
            var picks = new List<Item> { TakeRandom(rng, permanent), TakeRandom(rng, oneTime) }; //add one permanent + one one-time
            permanent.AddRange(oneTime);
            picks.Add(TakeRandom(rng, permanent)); //one random

            // Shuffle the order
            for (int i = picks.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (picks[i], picks[j]) = (picks[j], picks[i]);
            }
            return picks.ToArray();
        }

        // Removes + returns a random entry, so it can't be picked again
        private static Item TakeRandom(Random rng, List<Item> items)
        {
            int i = rng.Next(items.Count);
            Item item = items[i];
            items.RemoveAt(i);
            return item;
        }
    }
}
