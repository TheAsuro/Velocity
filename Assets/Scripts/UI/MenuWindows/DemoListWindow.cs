using Demos;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class DemoListWindow : DefaultMenuWindow
    {
        [SerializeField] private GameObject demoContentPanel;
        [SerializeField] private GameObject demoPanelPrefab;

        private Demo loadingDemo;

        public override void OnActivate()
        {
            base.OnActivate();
            Load();
        }

        private void Load()
        {
            //Clear all children
            foreach (Transform child in demoContentPanel.transform)
            {
                Destroy(child.gameObject);
            }

            //Create a list of all playable maps
            Demo[] allDemos = Demo.GetAllDemos();
            ((RectTransform) demoContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * allDemos.Length + 10f);

            for (int i = 0; i < allDemos.Length; i++)
            {
                CreateDemoPanel(i, allDemos[i]);
            }
        }

        private void CreateDemoPanel(int slot, Demo demo)
        {
            Transform t = GameMenu.CreatePanel(slot, demoPanelPrefab, demoContentPanel.transform).transform;
            MapData demoMap = GameInfo.info.MapManager.GetMapById(demo.MapID);

            t.FindChild("Map").GetComponent<Text>().text = demoMap.name;
            t.FindChild("Time").GetComponent<Text>().text = demo.TotalTickTime.ToTimeString();
            t.FindChild("Player").GetComponent<Text>().text = demo.PlayerName;

            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                loadingDemo = demo;
                GameMenu.SingletonInstance.AddWindow(Window.LOADING);
                SceneManager.sceneLoaded += LoadedDemoMap;
                GameInfo.info.MapManager.LoadMap(demoMap);
            });
            t.FindChild("Remove").GetComponent<Button>().onClick.AddListener(demo.DeleteDemoFile);
        }

        private void LoadedDemoMap(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= LoadedDemoMap;
            GameMenu.SingletonInstance.CloseAllWindows();
            WorldInfo.info.PlayDemo(loadingDemo, true, false);
        }
    }
}