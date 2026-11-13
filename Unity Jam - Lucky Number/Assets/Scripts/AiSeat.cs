using System.Collections;
using TMPro;
using UnityEngine;
using UnityJam.LuckyNumber.Audio;
using UnityEngine.UI;

// One AI seat + it's visuals
public class AiSeat : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image luckyImage;
    [SerializeField] private TMP_Text luckyUnknown; // "?" until the table's lucky numbers are dealt
    [SerializeField] private Sprite[] luckyFaces;   // UI dice faces 1-6
    [SerializeField] private TMP_Text chipText;
    [SerializeField] private Image chipImage;
    [SerializeField] private DieRoller die;
    [SerializeField] private GameObject onYouTag;     // optional
    [SerializeField] private VfxPlayer sparkle;       // Sparkle_Burst_Gold
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform coinTarget; // where won coins land, defaults to this plate
    [SerializeField] private Color winChipColor = new Color(1f, 0.8f, 0.3f);
    [SerializeField] private float lostAlpha = 0.55f;
    [SerializeField] private float dimAlpha = 0.6f;
    [SerializeField] private Color luckyGlow = new Color(1f, 0.85f, 0.55f);

    private Color chipColor;

    public RectTransform CoinTarget => coinTarget != null ? coinTarget : (RectTransform)transform;

    private void Awake() => chipColor = chipImage.color;

    private void Start() => StartCoroutine(Juice.Pulse(luckyImage, luckyGlow));

    public void SetName(string seatName) => nameText.text = seatName;

    public void SetLucky(int face)
    {
        luckyImage.sprite = luckyFaces[face - 1];
        ShowLucky(true);
    }

    public void ClearLucky() => ShowLucky(false);

    private void ShowLucky(bool known)
    {
        luckyImage.gameObject.SetActive(known); // twinkle child goes with it
        luckyUnknown.gameObject.SetActive(!known);
    }

    // Waiting state: chip "?", no die, ON YOU cleared, full alpha
    public void ResetForAnte()
    {
        chipText.text = "?";
        chipImage.color = chipColor;
        die.Clear();
        SetOnYou(false);
        group.alpha = 1f;
    }

    public IEnumerator RollBet(System.Random rng, int bet)
    {
        yield return die.Roll(rng, bet);
        AudioManager.Sfx("AiBet");
        chipText.text = bet.ToString();
        yield return Juice.Punch(chipText.transform);
    }

    public void SetOnYou(bool on)
    {
        if (onYouTag != null) onYouTag.SetActive(on);
    }

    public void Win()
    {
        chipImage.color = winChipColor;
        sparkle.Play();
    }

    public void Lose() => group.alpha = lostAlpha;

    // While the player's die rolls
    public void Dim(bool on) => group.alpha = on ? dimAlpha : 1f;
}
