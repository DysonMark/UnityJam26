using UnityEngine;

namespace UnityJam.LuckyNumber.Core
{
    // all balance number lives here, items pass a modified copy of this SO into the math calls
    [CreateAssetMenu(fileName = "TuningConfig", menuName = "Lucky Number/Tuning Config")]
    public class TuningConfig : ScriptableObject
    {
        public int startingMoney = 100;
        public int ante = 10;                 // per player per roll
        public int rollsPerTable = 10;
        public int cashTarget = 200;          // placeholder, set from simulator results
        public float luckyMultiplier = 2f;
        public float secondMultiplier = 1.5f;
        [Range(0f, 1f)] public float loadedChance = 0.5f;
        [Range(0f, 1f)] public float aiRandomBetChance = 0.15f;

        // indexed by (int)Item, same order as the Item enum. TODO replace placeholder pricing
        public int[] itemPrices = { 60, 50, 60, 60, 50, 30, 30, 30 };

        // Item effects, added to the table's values. TODO replace placeholder numbers
        [Range(0f, 1f)] public float heavierLoadBonus = 0.15f; // added to loadedChance
        public float highRollerBonus = 0.5f;                   // added to luckyMultiplier
    }
}
