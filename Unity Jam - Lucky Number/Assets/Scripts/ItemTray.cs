using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// "Use <item>" buttons on the bet panel, one per unplayed one-time item.
// Clicking one hands the item back through onPlayed, TurnManager applies it.
// Lives in the scene: size, spacing and alignment come from this object's layout group and the button prefab.
public class ItemTray : MonoBehaviour
{
    [SerializeField] private Button buttonPrefab;
    public UnityEvent<Item> onPlayed = new UnityEvent<Item>();

    public void Show(IEnumerable<Item> items)
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        foreach (Item item in items)
        {
            Button button = Instantiate(buttonPrefab, transform);
            button.GetComponentInChildren<TMP_Text>().text = $"Use {Shop.Names[(int)item]}";
            Item played = item;
            button.onClick.AddListener(() => onPlayed.Invoke(played));
        }
    }
}
