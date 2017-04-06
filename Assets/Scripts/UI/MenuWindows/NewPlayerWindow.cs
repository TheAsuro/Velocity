using Game;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class NewPlayerWindow : DefaultMenuWindow
    {
        private PlayerSave player;

        [SerializeField]
        private InputField playerNameField;

        [SerializeField]
        private InputField playerPassField;

        [SerializeField]
        private InputField playerMailField;

        public void OnOkClick()
        {
            player = new PlayerSave(playerNameField.text);
            player.SaveFile();

            player.OnAccountRequestFinished += (s, e) => GameInfo.info.RunOnMainThread(() => FinishedAccountRequest(e));
            player.OnLoginFinished += (s, e) => GameInfo.info.RunOnMainThread(() => FinishedLoginRequest(e));
            player.StartCreate(playerPassField.text, playerMailField.text);

            SetInteractive(false);
        }

        private void FinishedAccountRequest(EventArgs<string> e)
        {
            if (e.Error)
            {
                SetInteractive(true);

                ErrorWindow window = (ErrorWindow) GameMenu.SingletonInstance.AddWindow(Window.ERROR);
                window.SetText(e.ErrorText);
            }
        }

        private void FinishedLoginRequest(EventArgs<string> e)
        {
            SetInteractive(true);

            if (e.Error)
            {
                ErrorWindow window = (ErrorWindow) GameMenu.SingletonInstance.AddWindow(Window.ERROR);
                window.SetText(e.ErrorText);
            }
            else
            {
                PlayerSave.current = player;
                GameMenu.SingletonInstance.CloseWindow();
                GameMenu.SingletonInstance.CloseWindow();
            }
        }
    }
}
