using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        //References to specific things
        public GameObject gameSelectionContentPanel;
        public GameObject mapPanelPrefab;
        public GameObject editPanelPrefab;
        public Text blogText;

        public enum GameSelectionContent
        {
            PLAYABLE_MAP_LIST,
            NEW_MAP_SETTINGS,
            EDITABLE_MAP_LIST
        }

        private void Awake()
        {
            if (!PlayerPrefs.HasKey("lastplayer"))
                GameInfo.info.CurrentSave = new SaveData(PlayerPrefs.GetString("lastplayer"));
        }

        public void OnPlayButtonPress()
        {
            SaveData sd = GameInfo.info.CurrentSave;
            if(sd == null || sd.Account.Name.Equals("") || sd.Account.IsLoggedIn == false)
            {
                GameMenu.SingletonInstance.AddWindow(Window.LOGIN);
            }
            else
            {
                GameMenu.SingletonInstance.AddWindow(Window.LEVEL_SELECT);
            }
        }

        public void DeletePlayer(string name)
        {
            SaveData sd = new SaveData(name);
            sd.DeleteData();

            //Log out from current player if we deleted that one
            if (GameInfo.info.CurrentSave != null && GameInfo.info.CurrentSave.Name == name)
                GameInfo.info.CurrentSave = null;
        }

        public void Quit()
        {
            GameInfo.info.Quit();
        }
    }
}
