using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityJam.LuckyNumber.Audio;

// Win = all 3 tables are beat, a single win will display the shop UI
public enum EndReason
{
    Win,
    Caught,
    Broke,
    OutOfRolls,
}

// One panel for every ending, the text changes with the reason
public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private string menuSceneName = "Menu";

    // Indexed by (int)EndReason, same order as the enum.
    private static readonly string[] Titles =
    {
        "The House Remembers",
        "Wrong Number",
        "Empty Pockets",
        "Last Call",
    };

    private static readonly string[] Messages =
    {
        "Three tables, three fortunes. Your debt is paid and you walk out into the rain. Nobody stops you. They'll remember your face, though.",
        "A hand lands on your shoulder. Everyone saw the dice. The house doesn't forget a cheat, and neither does the alley out back.",
        "Your last chip slides across the felt. Somebody else is already pulling up your chair.",
        "The dealer sweeps the dice away. Not enough on the table, and no rolls left to fix it.",
    };

    public void Show(EndReason reason, int money)
    {
        titleText.text = Titles[(int)reason];
        messageText.text = Messages[(int)reason];
        moneyText.text = "$" + money;
        uiPanel.SetActive(true);
        if (reason == EndReason.Caught && AudioManager.Instance != null) AudioManager.Instance.Caught();
    }
    
    public void Restart()
    {
        Time.timeScale = 1f; //reset time in case game is paused
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
