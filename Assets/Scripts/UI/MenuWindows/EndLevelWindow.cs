using Demos;
using UnityEngine;

namespace UI.MenuWindows
{
    public class EndLevelWindow : DefaultMenuWindow
    {
        private Demo demo;
        private DemoPlay demoPlayer;

        public EndLevelWindow(Demo demo, DemoPlay demoPlayer)
        {
            this.demo = demo;
            this.demoPlayer = demoPlayer;
            PlayRaceDemo();
        }

        public void RestartRun()
        {
            GameInfo.info.Reset();
        }

        public void PlayRaceDemo()
        {
            if (demoPlayer != null && demo != null)
                demoPlayer.PlayDemo(demo, delegate { GameMenu.SingletonInstance.AddWindow(Window.END_LEVEL); }, true);
        }

        //Save demo to ".vdem" file, does not work in web player
        public void SaveLastDemo()
        {
            if (demo != null)
                demo.SaveToFile(Application.dataPath);
        }

        public void ToMainMenu()
        {
            GameMenu.SingletonInstance.CloseAllWindows();
            GameInfo.info.LoadLevel("MainMenu");
        }
    }
}
