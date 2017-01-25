using System.Collections.Generic;
using System.IO;
using Api;
using Game;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class GameSelectionWindow : DefaultMenuWindow
    {
        [SerializeField] private List<MapData> defaultMapData;

        [SerializeField] private GameObject mapPanelPrefab;
        [SerializeField] private GameObject editPanelPrefab;
        [SerializeField] private RectTransform contentTransform;

        private void Awake()
        {
            LoadPlayableMaps();
        }

        private void LoadPlayableMaps()
        {
            contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * defaultMapData.Count + 10f);

            int counter = 0;
            foreach (MapData data in defaultMapData)
            {
                string pb = "-";
                decimal pbTime = GameInfo.info.CurrentSave.GetPersonalBest(data.name);
                if (pbTime != -1)
                    pb = pbTime.ToString("0.0000");
                CreateMapPanel(counter++, data, pb);
            }
        }

        private void LoadEditableMaps()
        {
            string[] mapFiles = Directory.GetFiles(Application.dataPath, "*.vlvl");
            for (int i = 0; i < mapFiles.Length; i++)
            {
                mapFiles[i] = Path.GetFileNameWithoutExtension(mapFiles[i]);
            }

            contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapFiles.Length + 10f);

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

        private void CreateMapPanel(int slot, MapData map, string pb)
        {
            Transform t = GameMenu.CreatePanel(slot, mapPanelPrefab, contentTransform).transform;

            t.FindChild("Name").GetComponent<Text>().text = name;
            t.FindChild("Author").GetComponent<Text>().text = "Map by " + map.author;
            t.FindChild("Preview").GetComponent<RawImage>().texture = map.previewImage;
            t.FindChild("PB").GetComponent<Text>().text = "PB: " + pb;
            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(() => OnPlayableMapClick(map));

            StartCoroutine(UnityUtils.RunWhenDone(Leaderboard.GetRecord(map), (request) =>
            {
                if (!request.Error)
                    SetWrText(t, request.Result[0]);
            }));
        }

        private void CreateEditPanel(int slot, string fileName)
        {
            Transform t = GameMenu.CreatePanel(slot, editPanelPrefab, contentTransform).transform;

            t.FindChild("Name").GetComponent<Text>().text = fileName;
            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(() => LoadEditorWithLevel(fileName));
        }

        private void OnPlayableMapClick(MapData map)
        {
            GameMenu.SingletonInstance.CloseAllWindows();
            GameInfo.info.PlayLevel(map);
        }

        public void LoadEditorWithLevel(string levelName)
        {
            GameInfo.info.LoadEditor(levelName);
        }
    }
}