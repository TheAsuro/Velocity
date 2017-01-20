using System.Collections.Generic;
using System.IO;
using Api;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class GameSelectionWindow : DefaultMenuWindow
    {
        [SerializeField] public List<string> mapNames = new List<string>();
        [SerializeField] public List<string> mapAuthors = new List<string>();
        [SerializeField] public List<Texture2D> mapPreviews = new List<Texture2D>();

        [SerializeField] private GameObject mapPanelPrefab;
        [SerializeField] private GameObject editPanelPrefab;
        [SerializeField] private RectTransform contentTransform;

        private void Awake()
        {
            LoadPlayableMaps();
        }

        private void LoadPlayableMaps()
        {
            int mapCount = Mathf.Min(mapNames.Count, mapAuthors.Count);
            contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapCount + 10f);

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

        private void CreateMapPanel(int slot, string name, string author, Texture2D preview, string pb)
        {
            Transform t = GameMenu.CreatePanel(slot, mapPanelPrefab, contentTransform).transform;

            t.FindChild("Name").GetComponent<Text>().text = name;
            t.FindChild("Author").GetComponent<Text>().text = "Map by " + author;
            t.FindChild("Preview").GetComponent<RawImage>().texture = preview;
            t.FindChild("PB").GetComponent<Text>().text = "PB: " + pb;
            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(() => OnPlayableMapClick(name));

            Leaderboard.GetRecord(name, (entry) => SetWrText(t, entry));
        }

        private void CreateEditPanel(int slot, string fileName)
        {
            Transform t = GameMenu.CreatePanel(slot, editPanelPrefab, contentTransform).transform;

            t.FindChild("Name").GetComponent<Text>().text = fileName;
            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(() => LoadEditorWithLevel(fileName));
        }

        private void OnPlayableMapClick(string mapName)
        {
            GameMenu.SingletonInstance.CloseAllWindows();
            GameInfo.info.LoadLevel(mapName);
        }

        public void LoadEditorWithLevel(string levelName)
        {
            GameInfo.info.editorLevelName = levelName;
            GameInfo.info.LoadLevel("LevelEditor");
        }
    }
}