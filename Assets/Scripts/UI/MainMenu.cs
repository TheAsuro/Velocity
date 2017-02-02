using Game;
using UI.MenuWindows;

namespace UI
{
    public class MainMenu : DefaultMenuWindow
    {
        public void OnPlayButtonPress()
        {
            PlayerSave sd = PlayerSave.current;
            if(sd == null || sd.Name.Equals("") || sd.IsLoggedIn == false)
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
