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
        public Type settingType;
        public int roundDigits = -1;
        public string[] valueNames;
        public float[] valueMeaning;
        public string prefix = "";
        public string postfix = "";

        void Awake()
        {
            slider.onValueChanged.AddListener(OnValueChanged);
            MainMenu.SettingsOpened += OnSettingsOpened;
        }

        private void OnValueChanged(float value)
        {
            
        }

        public void OnSettingsOpened(object sender, EventArgs e)
        {
            slider.value = Game.GetSettingFloat<typeof(settingType)>(settingName);
        }
    }
}