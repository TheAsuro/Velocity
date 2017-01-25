using System.Collections.Generic;
using Api;
using Game;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class LeaderboardWindow : DefaultMenuWindow
    {
        public InputField mapNameInput;
        public List<LeaderboardPanel> entryPanels;

        private MapData loadedMap;
        private int startIndex = 0;

        private void Awake()
        {
            mapNameInput.onEndEdit.AddListener(ChangeMap);
        }

        private void ChangeMap(string mapName)
        {
            startIndex = 0;
            MapData newMap = GameInfo.info.MapManager.DefaultMaps.Find(map => map.name == mapName);
            if (newMap != null)
                LoadMap(newMap);
            else
                DisplayData(new LeaderboardEntry[0]);
        }

        public void LoadMap(MapData map)
        {
            loadedMap = map;
            StartCoroutine(UnityUtils.RunWhenDone(Leaderboard.GetEntries(map, startIndex, entryPanels.Count), (request) =>
            {
                if (!request.Error)
                    DisplayData(request.Result);
                else
                    print(request.ErrorText);
            }));
        }

        public void AddIndex(int add)
        {
            startIndex += add;
            if (startIndex < 0)
                startIndex = 0;
            LoadMap(loadedMap);
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
                    entryPanels[i].SetButtonActive(false);
                }
                else
                {
                    entryPanels[i].Time = entries[i].time.ToString("0.0000");
                    entryPanels[i].Player = entries[i].playerName;
                    entryPanels[i].Rank = entries[i].rank.ToString();
                    entryPanels[i].SetButtonActive(true);
                }
            }
        }
    }
}