using System;
using Settings;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class SettingsWindow : DefaultMenuWindow
    {
        public static event EventHandler UpdateSettingSliders;

        [SerializeField] private GameObject[] settingObjects;

        [SerializeField] private Toggle[] settingTitles;

        [SerializeField] private Slider anisoSlider;
        [SerializeField] private Text anisoDisplay;
        [SerializeField] private Slider aaSlider;
        [SerializeField] private Text aaDisplay;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private Text fovDisplay;

        /// <summary>
        /// These will be always applied when leaving. They are stored when opening the window and will only be changed when the user presses OK/Apply.
        /// </summary>
        private GameSettings oldSettings;

        public void SetSettingGroup(int groupId)
        {
            foreach (GameObject obj in settingObjects)
            {
                obj.SetActive(false);
            }

            settingObjects[groupId].SetActive(true);
            settingTitles[groupId].isOn = true;
        }

        public void OnTitleStatusChange(int group)
        {
            if (settingTitles[group].isOn)
            {
                SetSettingGroup(group);
            }
        }

        public void Save()
        {
            // Change settings that will be applied later
            oldSettings = (GameSettings) GameSettings.SingletonInstance.Clone();
        }

        public override void OnActivate()
        {
            base.OnActivate();

            // Save current settings instance for later
            oldSettings = GameSettings.SingletonInstance;
            // Create new settings instance from file
            GameSettings.Reload();
            if (UpdateSettingSliders != null)
                UpdateSettingSliders(this, null);

            GameSettings currentSettings = GameSettings.SingletonInstance;

            CreateSlider(anisoSlider, anisoDisplay, currentSettings.AnisotropicFiltering, setting => GameSettings.SingletonInstance.AnisotropicFiltering = setting);
            CreateSlider(aaSlider, aaDisplay, currentSettings.AntiAliasing, setting => GameSettings.SingletonInstance.AntiAliasing = setting);
            CreateSlider(fovSlider, fovDisplay, currentSettings.Fov, setting => GameSettings.SingletonInstance.Fov = setting);
        }

        private static void CreateSlider<T>(Slider slider, Text display, NameSetting<T> currentSetting, Action<NameSetting<T>> applySetting)
        {
            slider.onValueChanged.AddListener(val =>
            {
                NameSetting<T> setting = NameSetting<T>.FromSlider(val);
                applySetting(setting);
                display.text = setting.name;
            });
            slider.value = currentSetting.sliderValue;
        }

        private static void CreateSlider(Slider slider, Text display, float currentSetting, Action<float> applySetting, string floatFormat = "0.00")
        {
            slider.onValueChanged.AddListener(val =>
            {
                applySetting(val);
                display.text = val.ToString(floatFormat);
            });
            slider.value = currentSetting;
        }

        public override void OnClose()
        {
            base.OnClose();
            // Apply stored settings
            GameSettings.SingletonInstance = oldSettings;
        }
    }
}