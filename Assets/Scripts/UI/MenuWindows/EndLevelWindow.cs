using Demos;
using Game;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class EndLevelWindow : DefaultMenuWindow
    {
        [SerializeField] private Text timeText;
        [SerializeField] private Transform medalTransform;
        [SerializeField] private GameObject pbMedalPrefab;

        private Demo demo;

        public void Initialize(Demo demo, bool isPb)
        {
            this.demo = demo;
            PlayRaceDemo();
            timeText.text = timeText.text.Replace("$time", demo.TotalTickTime.ToTimeString());
            if (isPb)
            {
                Instantiate(pbMedalPrefab, medalTransform);
            }
        }

        public void RestartRun()
        {
            GameMenu.SingletonInstance.CloseAllWindows();
            WorldInfo.info.CreatePlayer(false);
        }

        public void PlayRaceDemo()
        {
            WorldInfo.info.PlayDemo(demo, true, false);
        }

        public void SaveLastDemo()
        {
            demo.SaveToFile();
        }

        public void ToMainMenu()
        {
            GameInfo.info.LoadMainMenu();
        }

        public override void OnClose()
        {
            base.OnClose();
            WorldInfo.info.StopDemo();
        }
    }
}
