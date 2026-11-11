using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UnityJam.LuckyNumber.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pauseObject;
        [SerializeField] private OptionsPanel options;
        [SerializeField] private string menuSceneName = "Menu";

        // Private variables
        private InputSystem_Actions actions;
        private bool isPaused;
        private bool isOptionsOpen;

        public static bool IsPaused { get; private set; }

        private void OnEnable()
        {
            actions ??= new InputSystem_Actions(); // here, not Awake: a script reload in Play mode clears it and skips Awake
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
            if (isOptionsOpen)
                CloseOptions();
            else
                TogglePause();
        }

        public void TogglePause()
        {
            SetPaused(!isPaused);
        }

        private void SetPaused(bool paused)
        {
            isPaused = paused;
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            pauseObject.SetActive(paused);
            if (!paused)
                CloseOptions();
        }

        public void Resume()
        {
            SetPaused(false);
        }
        public void PauseOptions()
        {
            isOptionsOpen = true;
            pauseObject.SetActive(false);      // hide the pause buttons behind the panel
            options.Open();
        }

        public void CloseOptions()
        {
            if (!isOptionsOpen)
                return;
            isOptionsOpen = false;
            options.Close();
            if (isPaused)
                pauseObject.SetActive(true);
        }
        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            SceneManager.LoadScene(menuSceneName);
        }
    }
}