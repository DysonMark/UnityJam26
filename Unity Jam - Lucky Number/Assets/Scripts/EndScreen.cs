using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    { //TODO
        "You Win",
        "Caught",
        "Broke",
        "Out of Rolls",
    };

    private static readonly string[] Messages =
    { //TODO
        "Insert witty line about how you won here",
        "Insert witty line about how you got caught here",
        "Insert witty line here about how you're broke",
        "Insert  witty line about how you ran out of rolls here",
    };

    public void Show(EndReason reason, int money)
    {
        titleText.text = Titles[(int)reason];
        messageText.text = Messages[(int)reason];
        moneyText.text = "$" + money;
        uiPanel.SetActive(true);
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
