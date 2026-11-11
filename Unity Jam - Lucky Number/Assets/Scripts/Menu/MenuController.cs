using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "Main";
        [SerializeField] private Button playButton;
        [SerializeField] private RectTransform playShine; // stripe swept across Play, under a RectMask2D
        [SerializeField] private OptionsPanel optionsPanel;

        private void Start() => StartCoroutine(Juice.Shine(playShine, playButton));

        public void PlayGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void Options()
        {
            optionsPanel.Open();
        }

        public void QuitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
