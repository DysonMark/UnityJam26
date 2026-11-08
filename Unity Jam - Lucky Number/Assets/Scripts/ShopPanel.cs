using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = System.Random;

// Shows 3 items from Shop.Pick, buying takes money in and hands it back through Money.
// TODO The run controller calls Open(money) after a table and reads Money when the shop closes
public class ShopPanel : MonoBehaviour
{
    [SerializeField] private TuningConfig config;
    [SerializeField] private TextMeshProUGUI moneyText;

    // Indexed by (int)Item, same order as the Item enum (8 entries).
    [SerializeField] private Sprite[] itemIcons;

    // One entry per slot, Shop.Offered (3) long, same order in every array.
    [SerializeField] private Button[] slotButtons;
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private TextMeshProUGUI[] slotNames;
    [SerializeField] private TextMeshProUGUI[] slotDescriptions;
    [SerializeField] private TextMeshProUGUI[] slotPrices;
    [SerializeField] private Color cantAffordColor = Color.red;

    // TODO - remove once the run controller calls Open(money).
    [SerializeField] private int testMoney = 100;

    public UnityEvent<Item> onBought;
    public UnityEvent onClosed;

    public int Money { get; private set; }

    private readonly Random rng = new Random();
    private Item[] offered;
    private bool[] sold;
    private Color priceColor; // the price text's normal color, read from the scene

    private void Awake()
    {
        priceColor = slotPrices[0].color;
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i; // copy, so each button's listener keeps its own slot number
            slotButtons[i].onClick.AddListener(() => Buy(slot));
        }
    }

    private void Start()
    {
        Open(testMoney);
    }

    public void Open(int money)
    {
        Money = money;
        offered = Shop.Pick(rng);
        sold = new bool[offered.Length];

        for (int i = 0; i < offered.Length; i++)
        {
            slotIcons[i].sprite = itemIcons[(int)offered[i]];
            slotNames[i].text = Shop.Names[(int)offered[i]];
            slotDescriptions[i].text = Shop.Descriptions[(int)offered[i]];
            slotPrices[i].text = "$" + Price(i);
        }
        Refresh();
        gameObject.SetActive(true);
    }

    // TODO Hook to a "Leave shop" button.
    public void Close()
    {
        gameObject.SetActive(false);
        onClosed?.Invoke();
    }

    private void Buy(int slot)
    {
        if (sold[slot] || Money < Price(slot)) return;

        Money -= Price(slot);
        sold[slot] = true;
        slotPrices[slot].text = "Sold";
        Debug.Log($"Bought {offered[slot]}, money left {Money}");
        onBought?.Invoke(offered[slot]);
        Refresh();
    }

    // Grey out anything sold or too expensive, and turn unaffordable prices red.
    private void Refresh()
    {
        moneyText.text = "$" + Money;
        for (int i = 0; i < offered.Length; i++)
        {
            bool canAfford = Money >= Price(i);
            slotButtons[i].interactable = !sold[i] && canAfford;
            slotPrices[i].color = sold[i] || canAfford ? priceColor : cantAffordColor;
        }
    }

    private int Price(int slot) => config.itemPrices[(int)offered[slot]];
}
