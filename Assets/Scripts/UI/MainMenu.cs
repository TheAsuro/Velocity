using Game;
using UI.MenuWindows;

namespace UI
{
    public class MainMenu : DefaultMenuWindow
    {
        public void OnPlayButtonPress()
        {
            SaveData sd = GameInfo.info.CurrentSave;
            if(sd == null || sd.Account.Name.Equals("") || sd.Account.IsLoggedIn == false)
            {
                GameMenu.SingletonInstance.AddWindow(Window.LOGIN);
            }
            else
            {
                GameMenu.SingletonInstance.AddWindow(Window.LEVEL_SELECT);
            }
        }

        public void Quit()
        {
            GameInfo.info.Quit();
        }
    }
}
