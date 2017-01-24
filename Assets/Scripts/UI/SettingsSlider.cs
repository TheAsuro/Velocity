using System;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsSlider : MonoBehaviour
    {
        public Slider slider;
        public Text display;
        public string settingName;

        private void Awake()
        {
            slider.onValueChanged.AddListener(OnValueChanged);
            SettingsWindow.UpdateSettingSliders += OnLoadSettings;
            OnLoadSettings(this, null);
        }

        private void OnValueChanged(float value)
        {
            SetSetting(value);
            DisplaySetting();
        }

        public void OnLoadSettings(object sender, EventArgs e)
        {
            DisplaySetting();
        }

        private void SetSetting(float value)
        {
            Settings.Game.SetSettingFloat(settingName, value);
        }

        private void DisplaySetting()
        {
            slider.value = Settings.Game.GetSettingFloat(settingName);
            display.text = Settings.Game.GetSettingValueName(settingName);
        }
    }
}