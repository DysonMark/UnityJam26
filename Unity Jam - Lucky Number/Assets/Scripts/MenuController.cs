using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityJam.LuckyNumber
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "Main";

        public void PlayGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void Options()
        {

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
