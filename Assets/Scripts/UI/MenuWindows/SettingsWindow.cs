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

        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text volumeDisplay;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private Text fovDisplay;

        [SerializeField] private Slider anisoSlider;
        [SerializeField] private Text anisoDisplay;
        [SerializeField] private Slider aaSlider;
        [SerializeField] private Text aaDisplay;
        [SerializeField] private Slider texSizeSlider;
        [SerializeField] private Text texSizeDisplay;
        [SerializeField] private Slider vsyncSlider;
        [SerializeField] private Text vsyncDisplay;

        [SerializeField] private Slider mouseSpeedSlider;
        [SerializeField] private Text mouseSpeedDisplay;
        [SerializeField] private Slider yInvertSlider;
        [SerializeField] private Text yInvertDisplay;
        [SerializeField] private Slider mouseRawSlider;
        [SerializeField] private Text mouseRawDisplay;

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
            GameSettings.Load();
            if (UpdateSettingSliders != null)
                UpdateSettingSliders(this, null);

            GameSettings currentSettings = GameSettings.SingletonInstance;

            CreateSlider(volumeSlider, volumeDisplay, currentSettings.Volume, setting => currentSettings.Volume = setting);
            CreateSlider(fovSlider, fovDisplay, currentSettings.Fov, setting => currentSettings.Fov = setting);

            CreateSlider(anisoSlider, anisoDisplay, currentSettings.AnisotropicFiltering, setting => currentSettings.AnisotropicFiltering = setting);
            CreateSlider(aaSlider, aaDisplay, currentSettings.AntiAliasing, setting => currentSettings.AntiAliasing = setting);
            CreateSlider(texSizeSlider, texSizeDisplay, currentSettings.TextureSize, setting => currentSettings.TextureSize = setting);
            CreateSlider(vsyncSlider, vsyncDisplay, currentSettings.VSyncSetting, setting => currentSettings.VSyncSetting = setting);

            CreateSlider(mouseSpeedSlider, mouseSpeedDisplay, currentSettings.MouseSpeed, setting => currentSettings.MouseSpeed = setting);
            CreateSlider(yInvertSlider, yInvertDisplay, currentSettings.InvertY, setting => currentSettings.InvertY = setting);
            CreateSlider(mouseRawSlider, mouseRawDisplay, currentSettings.RawMouse, setting => currentSettings.RawMouse = setting);
        }

        private static void CreateSlider<T>(Slider slider, Text display, NameSetting<T> currentSetting, Action<NameSetting<T>> applySetting)
        {
            slider.onValueChanged.AddListener(val =>
            {
                NameSetting<T> setting = currentSetting.FromSlider(val);
                applySetting(setting);
                display.text = setting.name;
                GameSettings.SingletonInstance.ApplySettings();
            });
            slider.value = currentSetting.sliderValue;
            slider.onValueChanged.Invoke(slider.value);
        }

        private static void CreateSlider(Slider slider, Text display, float currentSetting, Action<float> applySetting, string floatFormat = "0.00")
        {
            slider.onValueChanged.AddListener(val =>
            {
                applySetting(val);
                display.text = val.ToString(floatFormat);
                GameSettings.SingletonInstance.ApplySettings();
            });
            slider.value = currentSetting;
            slider.onValueChanged.Invoke(slider.value);
        }

        public override void OnClose()
        {
            base.OnClose();
            // Apply stored settings
            GameSettings.SingletonInstance = oldSettings;
            GameSettings.SingletonInstance.ApplySettings();
            GameSettings.SingletonInstance.Save();
        }
    }
}