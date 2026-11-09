using UnityEngine;

// all balance number lives here, items pass a modified copy of this SO into the math calls
[CreateAssetMenu(fileName = "TuningConfig", menuName = "Lucky Number/Tuning Config")]
public class TuningConfig : ScriptableObject
{
    public int table = 0;
    public int startingMoney = 100;       // buy-in: paid out of the last table's winnings, the rest is shop money
    public int ante = 10;                 // per player per roll
    public int rollsPerTable = 10;
    public int cashTarget = 200;          // placeholder, set from simulator results. Should be >= next table's startingMoney
    public float luckyMultiplier = 2f;
    public float secondMultiplier = 1.25f;
    [Range(0f, 1f)] public float loadedChance = 0.5f;
    [Range(0f, 1f)] public float aiRandomBetChance = 0.15f;
    [Range(0f, 1f)] public float copycatChance = 0.25f; // AI bets your lucky number right after you win on it, fades each roll
    [Range(0f, 1f)] public float chaserChance = 0.3f;   // one AI bets the last rolled number
    [Range(0f, 1f)] public float suspicionPerLuckyRoll = 0.2f; // loaded roll hits lucky, caught at 1

    // indexed by (int)Item, same order as the Item enum. TODO replace placeholder pricing
    public int[] itemPrices = { 60, 50, 60, 60, 30, 30, 50 };

    // Item effects, added to the table's values. TODO replace placeholder numbers
    [Range(0f, 1f)] public float heavierLoadBonus = 0.15f; // added to loadedChance
    public float highRollerBonus = 0.5f;                   // added to luckyMultiplier
    [Range(0f, 1f)] public float highRollerSuspicion = 0.05f; // added to suspicionPerLuckyRoll
    [Range(0f, 1f)] public float steadyHandsCut = 0.05f;      // taken off suspicionPerLuckyRoll
    [Range(0f, 1f)] public float bribeAmount = 0.2f;          // taken off suspicion when the table starts
    public int extraRolls = 2;                                 // added to rollsPerTable
}
