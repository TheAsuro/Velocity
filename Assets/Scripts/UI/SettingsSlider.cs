using System;
using UnityEngine;
using UnityEngine.UI;

namespace Settings
{
    public class SettingsSlider : MonoBehaviour
    {
        public Slider slider;
        public Text display;
        public string settingName;

        void Awake()
        {
            slider.onValueChanged.AddListener(OnValueChanged);
            MainMenu.SettingsOpened += OnSettingsOpened;
        }

        private void OnValueChanged(float value)
        {
            display.text = value.ToString();
            Game.SetSettingFloat(settingName, value);
        }

        public void OnSettingsOpened(object sender, EventArgs e)
        {
            slider.value = Game.GetSettingFloat(settingName);
        }
    }
}