using System.Collections;
using TMPro;
using UnityEngine;

// Runs the AI seats with pauses so it feels real
// Bets are picked by TurnManager, this only shows them.
public class AiController : MonoBehaviour
{
    [SerializeField] private AiSeat[] seats;
    [SerializeField] private float startDelay = 0.05f;
    [SerializeField] private Vector2 thinkPause = new Vector2(0.03f, 0.1f);
    [SerializeField] private float seatStagger = 0.1f;
    [SerializeField] private RectTransform pot;        // coins fly from here
    [SerializeField] private RectTransform coinPrefab;
    [SerializeField] private TMP_Text statusText;      // "The table is betting..." while the seats bet
    [SerializeField] private string[] names =
    {
        "Mr. Gray", "The Widow", "Lucky Sal", "Doc Malone", "Velvet", "Two-Bit Eddie",
        "Madame Rue", "The Deacon", "Slim", "Mrs. Finch",
    };

    private void Awake()
    {
        // Distinct names per seat: shuffle the first seats.Length entries
        for (int i = 0; i < seats.Length; i++)
        {
            int pick = Random.Range(i, names.Length);
            (names[i], names[pick]) = (names[pick], names[i]);
            seats[i].SetName(names[i]);
        }
    }

    public void SetLucky(int[] aiLucky)
    {
        for (int i = 0; i < seats.Length; i++) seats[i].SetLucky(aiLucky[i]);
    }

    public void Ante()
    {
        foreach (AiSeat seat in seats) seat.ResetForAnte();
    }

    public IEnumerator TakeBets(System.Random rng, int[] bets, int playerLucky)
    {
        // Spotlight: every seat dims, each one lights up and pops when it bets
        statusText.gameObject.SetActive(true);
        DimAll(true);
        yield return new WaitForSeconds(startDelay);
        Coroutine last = null;
        for (int i = 0; i < seats.Length; i++)
        {
            yield return new WaitForSeconds(Random.Range(thinkPause.x, thinkPause.y));
            seats[i].Dim(false);
            StartCoroutine(Juice.Punch(seats[i].transform, 0.12f, 0.2f));
            last = StartCoroutine(SeatBet(seats[i], rng, bets[i], playerLucky));
            yield return new WaitForSeconds(seatStagger);
        }
        yield return last;
        statusText.gameObject.SetActive(false);
    }

    public void DimAll(bool on)
    {
        foreach (AiSeat seat in seats) seat.Dim(on);
    }
    
    // playerTarget: where the player's coins land, null when the player didn't win
    public IEnumerator Payout(int rolled, int[] bets, RectTransform playerTarget)
    {
        for (int i = 0; i < seats.Length; i++)
        {
            seats[i].Dim(false);
            if (bets[i] != rolled) seats[i].Lose();
        }

        Coroutine last = null;
        if (playerTarget != null) last = StartCoroutine(Juice.FlyCoins(coinPrefab, pot, playerTarget));
        for (int i = 0; i < seats.Length; i++)
            if (bets[i] == rolled)
                last = StartCoroutine(Juice.FlyCoins(coinPrefab, pot, seats[i].CoinTarget));
        yield return last;

        // Sparkle once the coins land
        for (int i = 0; i < seats.Length; i++)
            if (bets[i] == rolled) seats[i].Win();
    }

    private static IEnumerator SeatBet(AiSeat seat, System.Random rng, int bet, int playerLucky)
    {
        yield return seat.RollBet(rng, bet);
        seat.SetOnYou(bet == playerLucky);
    }
}
