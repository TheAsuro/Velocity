using System.Collections.Generic;
using Api;
using Demos;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class LeaderboardWindow : DefaultMenuWindow
    {
        public InputField mapNameInput;
        public List<LeaderboardPanel> entryPanels; //Must always have ELEMENTS_PER_SITE elements!

        private string lastLoadedMap = "";
        private int startIndex = 0;

        private void Awake()
        {
            if (mapNameInput)
            {
                mapNameInput.onEndEdit.AddListener(ChangeMap);
            }
            else
            {
                LoadMap(SceneManager.GetActiveScene().name);
            }
        }

        private void LoadMap(string mapName)
        {
            Leaderboard.GetEntries(mapName, startIndex, entryPanels.Count, DisplayData);
            lastLoadedMap = mapName;
        }

        public void AddIndex(int add)
        {
            startIndex += add;
            if (startIndex < 0)
                startIndex = 0;
            LoadMap(lastLoadedMap);
        }

        private void ChangeMap(string mapName)
        {
            startIndex = 0;
            LoadMap(mapName);
        }

        private void DisplayData(LeaderboardEntry[] entries)
        {
            for(int i = 0; i < entryPanels.Count; i++)
            {
                if (entries.Length <= i)
                {
                    entryPanels[i].Time = "";
                    entryPanels[i].Player = "";
                    entryPanels[i].Rank = "";
                    entryPanels[i].SetButtonAction(delegate { });
                    entryPanels[i].SetButtonActive(false);
                }
                else
                {
                    entryPanels[i].Time = entries[i].time.ToString("0.0000");
                    entryPanels[i].Player = entries[i].playerName;
                    entryPanels[i].Rank = entries[i].rank.ToString();
                    int id = entries[i].id;
                    entryPanels[i].SetButtonAction(() => Leaderboard.GetDemo(id, ProcessDownloadedDemo));
                    entryPanels[i].SetButtonActive(true);
                }
            }
        }

        private void ProcessDownloadedDemo(Demo demo)
        {
            print("Player: " + demo.GetPlayerName());
            print("Level: " + demo.GetLevelName());
        }
    }
}