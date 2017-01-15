using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class SettingsWindow : MonoBehaviour, MenuWindow
    {
        public static event System.EventHandler UpdateSettingSliders;

        [SerializeField]
        private GameObject[] settingObjects;

        [SerializeField]
        private Toggle[] settingTitles;

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

        public void Load()
        {
            Settings.AllSettings.LoadSettings();
            if (UpdateSettingSliders != null)
                UpdateSettingSliders(this, null);
        }

        public void Save()
        {
            Settings.AllSettings.SaveSettings();
        }

        public void Activate()
        {
            Load();
        }

        public void SetAsBackground()
        {
            gameObject.SetActive(false);
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}
