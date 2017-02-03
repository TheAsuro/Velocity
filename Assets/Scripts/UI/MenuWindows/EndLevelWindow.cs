using Demos;
using Game;
using UnityEngine;

namespace UI.MenuWindows
{
    public class EndLevelWindow : DefaultMenuWindow
    {
        private Demo demo;

        public void Initialize(Demo demo)
        {
            this.demo = demo;
            PlayRaceDemo();
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
