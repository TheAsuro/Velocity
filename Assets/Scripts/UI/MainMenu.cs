using System;
using System.Collections.Generic;
using System.IO;
using Api;
using Demos;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        //General stuff
        public List<string> mapNames = new List<string>();
        public List<string> mapAuthors = new List<string>();
        public List<Texture2D> mapPreviews = new List<Texture2D>();
        public GameObject[] menuObjects;

        //References to specific things
        public GameObject gameSelectionContentPanel;
        public GameObject mapPanelPrefab;
        public GameObject editPanelPrefab;
        public Text blogText;

        public MenuState CurrentState { get; private set; }

        private Action<object, EventArgs> returnToMenu;

        public enum MenuState
        {
            MAIN_MENU,
            GAME_SELECTION,
            PLAYER_SELECTION,
            NEW_PLAYER,
            BUG_REPORT
        }

        public enum GameSelectionContent
        {
            PLAYABLE_MAP_LIST,
            NEW_MAP_SETTINGS,
            EDITABLE_MAP_LIST
        }

        private void Awake()
        {
            SetMenuState(MenuState.MAIN_MENU);
            LoadLastPlayer();

            returnToMenu = (s, e) => SetMenuState(MenuState.MAIN_MENU);
            MainSubMenu.GoToMainMenu += returnToMenu.Invoke;
        }

        private void OnDestroy()
        {
            MainSubMenu.GoToMainMenu -= returnToMenu.Invoke;
        }

        //Load the last player that was logged in, returns false if loading failed
        private bool LoadLastPlayer()
        {
            if (!PlayerPrefs.HasKey("lastplayer"))
                return false;

            LoadPlayer(PlayerPrefs.GetString("lastplayer"));
            return true;
        }

        private void LoadPlayer(string name)
        {
            GameInfo.info.CurrentSave = new SaveData(name);
        }

        public void OnPlayButtonPress()
        {
            SaveData sd = GameInfo.info.CurrentSave;
            if(sd == null || sd.Account.Name.Equals("") || sd.Account.IsLoggedIn == false)
            {
                //Open login window if no logged in player exists
                SetMenuState(MenuState.PLAYER_SELECTION);
            }
            else
            {
                SetMenuState(MenuState.GAME_SELECTION);
            }
        }

        public void OnPlayerCreated(string playerName)
        {
            LoadPlayer(playerName);
            SetMenuState(MenuState.MAIN_MENU);
        }

        public void DeletePlayer(string name)
        {
            SaveData sd = new SaveData(name);
            sd.DeleteData(mapNames);

            //Log out from current player if we deleted that one
            if (GameInfo.info.CurrentSave != null && GameInfo.info.CurrentSave.Name == name)
                GameInfo.info.CurrentSave = null;
        }

        public void SetMenuState(int stateId)
        {
            SetMenuState((MenuState)stateId);
        }

        public void SetMenuState(MenuState newState)
        {
            //Disable all menu groups
            foreach (GameObject menuObj in menuObjects)
            {
                menuObj.SetActive(false);
            }

            //Enable the selected group
            menuObjects[(int)newState].SetActive(true);

            //Do menu-specific preparations
            switch(newState)
            {
                case MenuState.GAME_SELECTION:
                    SetGameSelectionContent(GameSelectionContent.PLAYABLE_MAP_LIST);
                    break;
            }

            CurrentState = newState;
        }

        public void SetGameSelectionContent(int contentId)
        {
            SetGameSelectionContent((GameSelectionContent)contentId);
        }

        private void SetGameSelectionContent(GameSelectionContent newContent)
        {
            //Clear all children
            foreach(Transform child in gameSelectionContentPanel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            //Create new children
            switch (newContent)
            {
                case GameSelectionContent.PLAYABLE_MAP_LIST:
                    LoadPlayableMaps(); break;
                case GameSelectionContent.EDITABLE_MAP_LIST:
                    LoadEditableMaps(); break;
                default:
                    print("todo"); break;
            }
        }

        private void LoadPlayableMaps()
        {
            int mapCount = Mathf.Min(mapNames.Count, mapAuthors.Count);
            ((RectTransform)gameSelectionContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapCount + 10f);

            for (int i = 0; i < mapCount; i++)
            {
                string pb = "-";
                decimal pbTime = GameInfo.info.CurrentSave.GetPersonalBest(mapNames[i]);
                if (pbTime != -1)
                    pb = pbTime.ToString("0.0000");
                CreateMapPanel(i, mapNames[i], mapAuthors[i], mapPreviews[i], pb);
            }
        }

        private void LoadEditableMaps()
        {
            string[] mapFiles = Directory.GetFiles(Application.dataPath, "*.vlvl");
            for (int i = 0; i < mapFiles.Length; i++)
            {
                mapFiles[i] = Path.GetFileNameWithoutExtension(mapFiles[i]);
            }

            ((RectTransform)gameSelectionContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapFiles.Length + 10f);

            for (int i = 0; i < mapFiles.Length; i++)
            {
                CreateEditPanel(i, mapFiles[i]);
            }
        }

        private void SetWrText(Transform panelTransform, LeaderboardEntry entry)
        {
            if (panelTransform == null || entry == null)
                return;

            Transform wrObj = panelTransform.FindChild("WR");
            if (wrObj == null || wrObj.GetComponent<Text>() == null)
                return;

            wrObj.GetComponent<Text>().text = "WR: " + entry.time + " by " + entry.playerName;
        }



        private void SetObjectText(Transform objTrans, string value)
        {
            objTrans.GetComponent<Text>().text = value;
        }

        private void CreateMapPanel(int slot, string name, string author, Texture2D preview, string pb)
        {
            Transform t = GameMenu.CreatePanel(slot, mapPanelPrefab, gameSelectionContentPanel.transform).transform;

            t.FindChild("Name").GetComponent<Text>().text = name;
            t.FindChild("Author").GetComponent<Text>().text = "Map by " + author;
            t.FindChild("Preview").GetComponent<RawImage>().texture = preview;
            t.FindChild("PB").GetComponent<Text>().text = "PB: " + pb;
            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { OnPlayableMapClick(name); });

            Leaderboard.GetRecord(name, (entry) => SetWrText(t, entry));
        }

        private void CreateEditPanel(int slot, string fileName)
        {
            Transform t = GameMenu.CreatePanel(slot, editPanelPrefab, gameSelectionContentPanel.transform).transform;

            t.FindChild("Name").GetComponent<Text>().text = fileName;
            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { LoadEditorWithLevel(fileName); });
        }

        private void OnPlayableMapClick(string mapName)
        {
            GameInfo.info.LoadLevel(mapName);
        }

        public void LoadEditorWithLevel(string levelName)
        {
            GameInfo.info.editorLevelName = levelName;
            GameInfo.info.LoadLevel("LevelEditor");
        }

        public void Quit()
        {
            GameInfo.info.Quit();
        }
    }
}
