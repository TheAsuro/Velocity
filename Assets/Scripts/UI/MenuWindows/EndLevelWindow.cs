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
            WorldInfo.info.RaceScript.PrepareNewRace();
        }

        public void PlayRaceDemo()
        {
            DemoPlayer.SingletonInstance.PlayDemo(demo);
        }

        //Save demo to ".vdem" file, does not work in web player
        public void SaveLastDemo()
        {
            demo.SaveToFile(Application.dataPath);
        }

        public void ToMainMenu()
        {
            GameMenu.SingletonInstance.CloseAllWindows();
            GameInfo.info.LoadLevel("MainMenu");
        }
    }
}
