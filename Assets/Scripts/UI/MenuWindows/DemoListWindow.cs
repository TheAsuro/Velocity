using System.IO;
using Demos;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class DemoListWindow : DefaultMenuWindow
    {
        [SerializeField] private GameObject demoContentPanel;
        [SerializeField] private GameObject demoPanelPrefab;

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
            Demo[] allDemos = DemoInfo.GetAllDemos();
            string[] allDemoFiles = DemoInfo.GetDemoNames();
            ((RectTransform) demoContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * allDemos.Length + 10f);

            for (int i = 0; i < allDemos.Length; i++)
            {
                CreateDemoPanel(i, allDemos[i].GetLevelName(), allDemos[i].GetTime().ToString(), allDemos[i].GetPlayerName(), allDemoFiles[i]);
            }
        }

        private void CreateDemoPanel(int slot, string map, string time, string player, string fileName)
        {
            Transform t = GameMenu.CreatePanel(slot, demoPanelPrefab, demoContentPanel.transform).transform;

            t.FindChild("Map").GetComponent<Text>().text = map;
            t.FindChild("Time").GetComponent<Text>().text = time;
            t.FindChild("Player").GetComponent<Text>().text = player;

            t.FindChild("Button")
                .GetComponent<Button>()
                .onClick.AddListener(() =>
                {
                    Demo demo = new Demo(Path.Combine(Application.dataPath, fileName));
                    GameInfo.info.PlayDemo(demo);
                });
            t.FindChild("Remove").GetComponent<Button>().onClick.AddListener(() => DemoInfo.DeleteDemoFile(fileName));
        }
    }
}