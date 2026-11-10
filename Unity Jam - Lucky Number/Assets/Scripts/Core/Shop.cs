using System;
using System.Collections.Generic;

// Permanent items first, one-time items last
// IsOneTIme checks if index is > that BribeTheDealer, so order matters here
public enum Item
{
    HeavierLoad,
    SteadyHands,
    SecondNumber,
    HighRoller,
    BribeTheDealer, // first one-time item
    ExtraRoll,
    RabbitsFoot,
}

// Item data, the shop pick, and item effects. Prices and effect amounts live in TuningConfig SO
// One-time items are played by the player from the bet panel, see TurnManager.PlayItem
public static class Shop
{
    public const int Offered = 3;

    public static readonly int ItemCount = Enum.GetValues(typeof(Item)).Length;

    // Indexed by (int)item, same order as the enum.
    public static readonly string[] Names =
    {
        "Heavier Load",
        "Stiff Drink",
        "Second Number",
        "High Roller",
        "Bribe the Dealer",
        "Extra Roll",
        "Horseshoe",
    };

    public static readonly string[] Descriptions =
    {
        "A little more lead. Loaded rolls find your number more often.",
        "Calms the nerves. Loaded wins draw less suspicion.",
        "Every man needs a backup. Pick a second number that also pays.",
        "Bigger wins on your number. Bigger eyes on you, too.",
        "Everyone has a price. Lowers suspicion now.",
        "One more throw before the chair goes cold.",
        "Luck owes you one. Your next loaded win goes unnoticed.",
    };

    public static bool IsOneTime(Item item) => item >= Item.BribeTheDealer;

    // Copy of the table's config with every owned item applied. The table's own config is never changed.
    // Owning an item twice applies it twice (TODO carrying items between tables is an open question).
    // Caller should Destroy the copy when the table ends.
    public static TuningConfig WithItems(TuningConfig table, IEnumerable<Item> owned)
    {
        TuningConfig cfg = UnityEngine.Object.Instantiate(table);
        foreach (Item item in owned)
        {
            switch (item)
            {
                case Item.HeavierLoad:
                    cfg.loadedChance = Math.Min(1f, cfg.loadedChance + cfg.heavierLoadBonus);
                    break;
                case Item.SteadyHands:
                    cfg.suspicionPerLuckyRoll = Math.Max(0f, cfg.suspicionPerLuckyRoll - cfg.steadyHandsCut);
                    break;
                case Item.HighRoller:
                    cfg.luckyMultiplier += cfg.highRollerBonus;
                    cfg.suspicionPerLuckyRoll += cfg.highRollerSuspicion;
                    break;
                // NOTE Second Number item changes nothing here: the game picks it from OpenNumbers
                // and passes it to ResolvePot() as playerSecond, which pays secondMultiplier whenever it's rolled.
                // One-time items (Bribe the Dealer, Extra Roll, Horseshoe) change nothing here, they act when played.
            }
        }
        return cfg;
    }

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
