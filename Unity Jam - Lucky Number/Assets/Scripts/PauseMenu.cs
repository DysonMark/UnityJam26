using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UnityJam.LuckyNumber.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pauseObject;
        [SerializeField] private string menuSceneName = "Menu";

        // Private variables
        private InputSystem_Actions actions;
        private bool isPaused;


        private void Awake()
        {
            actions = new InputSystem_Actions();
        }
        private void OnEnable()
        {
            actions.Player.Pause.performed += OnPausePerformed;
            actions.Player.Enable();
        }

        private void OnDisable()
        {
            actions.Player.Pause.performed -= OnPausePerformed;
            actions.Player.Disable();
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        public void TogglePause()
        {
            isPaused = !isPaused;

            Time.timeScale = isPaused ? 0f : 1f;
            pauseObject.SetActive(isPaused);
        }

        public void PauseOptions()
        {

        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}