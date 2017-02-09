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

        private Demo demo;

        public void Initialize(Demo demo, bool isPb)
        {
            this.demo = demo;
            PlayRaceDemo();
            timeText.text = timeText.text.Replace("$time", demo.TotalTickTime.ToTimeString());
            timeText.text = timeText.text.Replace("$ispb", isPb ? ", a new personal record!" : "");
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
