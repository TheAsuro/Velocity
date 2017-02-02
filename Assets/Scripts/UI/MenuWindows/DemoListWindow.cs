using Demos;
using UnityEngine;
using UnityEngine.UI;
using Util;

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

            t.FindChild("Map").GetComponent<Text>().text = demo.LevelName;
            t.FindChild("Time").GetComponent<Text>().text = demo.TotalTickTime.ToTimeString();
            t.FindChild("Player").GetComponent<Text>().text = demo.PlayerName;

            t.FindChild("Button").GetComponent<Button>().onClick.AddListener(() => WorldInfo.info.PlayDemo(demo, false, false));
            t.FindChild("Remove").GetComponent<Button>().onClick.AddListener(demo.DeleteDemoFile);
        }
    }
}