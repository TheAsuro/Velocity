using System.Collections.Generic;
using System.IO;
using Game;
using UI.Elements;
using UnityEngine;
using UnityEngine.Assertions;
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
            foreach (MapData map in defaultMapData)
            {
                string pb = "-";
                long pbTime;
                Assert.IsNotNull(PlayerSave.current);
                if (PlayerSave.current.GetPersonalBest(map, out pbTime))
                    pb = pbTime.ToTimeString();
                GameObject panel = GameMenu.CreatePanel(counter, mapPanelPrefab, contentTransform);
                panel.GetComponent<MapPanel>().Set(counter++, map, pb);
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

        private void CreateEditPanel(int slot, string fileName)
        {
            Transform t = GameMenu.CreatePanel(slot, editPanelPrefab, contentTransform).transform;

            t.FindChild("Name").GetComponent<Text>().text = fileName;
            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(() => LoadEditorWithLevel(fileName));
        }

        public void LoadEditorWithLevel(string levelName)
        {
            GameInfo.info.LoadEditor(levelName);
        }
    }
}