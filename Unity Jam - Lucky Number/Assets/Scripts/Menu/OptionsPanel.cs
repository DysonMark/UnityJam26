using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

namespace UnityJam.LuckyNumber
{
    public class OptionsPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;    // the Options panel to show/hide
       // [SerializeField] private GameObject menuButtons;  // optional: the button stack to hide while open

        [Header("Audio")]
        [SerializeField] private AudioMixer mixer;        // exposed params: MasterVol, MusicVol, SFXVol
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text masterLabel;
        [SerializeField] private TMP_Text musicLabel;
        [SerializeField] private TMP_Text sfxLabel;

        [Header("Fullscreen")]
        [SerializeField] private Toggle fullscreenToggle;

        const string P_Master = "MasterVol", P_Music = "MusicVol", P_SFX = "SFXVol";
        const string K_Master = "vol_master", K_Music = "vol_music", K_SFX = "vol_sfx", K_Full = "fullscreen";

        private void Awake()
        {
            // load saved settings
            masterSlider.normalizedValue = PlayerPrefs.GetFloat(K_Master, 0.8f);
            musicSlider.normalizedValue = PlayerPrefs.GetFloat(K_Music, 0.6f);
            sfxSlider.normalizedValue = PlayerPrefs.GetFloat(K_SFX, 0.7f);
            fullscreenToggle.isOn = PlayerPrefs.GetInt(K_Full, 1) == 1;

            // applying them
            ApplyMaster(0); 
            ApplyMusic(0); 
            ApplySFX(0);
            ApplyFullscreen(fullscreenToggle.isOn);

            // react to changes
            masterSlider.onValueChanged.AddListener(ApplyMaster);
            musicSlider.onValueChanged.AddListener(ApplyMusic);
            sfxSlider.onValueChanged.AddListener(ApplySFX);
            fullscreenToggle.onValueChanged.AddListener(ApplyFullscreen);

            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Open()
        {
            if (panelRoot != null) 
                panelRoot.SetActive(true);
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        //    if (menuButtons != null) menuButtons.SetActive(true);
            PlayerPrefs.Save();
        }

        private void ApplyMaster(float _) => SetBus(P_Master, K_Master, masterSlider, masterLabel);
        private void ApplyMusic(float _) => SetBus(P_Music, K_Music, musicSlider, musicLabel);
        private void ApplySFX(float _) => SetBus(P_SFX, K_SFX, sfxSlider, sfxLabel);

        private void SetBus(string param, string key, Slider slider, TMP_Text label)
        {
            float v = slider.normalizedValue;
            float dB = v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;
            if (mixer != null) mixer.SetFloat(param, dB);
            PlayerPrefs.SetFloat(key, v);
            if (label != null) label.text = Mathf.RoundToInt(v * 100f).ToString();
        }

        private void ApplyFullscreen(bool on)
        {
            Screen.fullScreen = on;
            PlayerPrefs.SetInt(K_Full, on ? 1 : 0);
        }
    }
}
