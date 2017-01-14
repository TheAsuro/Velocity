using System;
using Settings;
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
            SettingsMenu.UpdateSettingSliders += OnLoadSettings;
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
            Game.SetSettingFloat(settingName, value);
        }

        private void DisplaySetting()
        {
            slider.value = Game.GetSettingFloat(settingName);
            display.text = Game.GetSettingValueName(settingName);
        }
    }
}