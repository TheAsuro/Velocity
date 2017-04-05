using Game;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class NewPlayerWindow : DefaultMenuWindow
    {
        private string currentName;

        [SerializeField]
        private InputField playerNameField;

        [SerializeField]
        private InputField playerPassField;

        [SerializeField]
        private InputField playerMailField;

        public void OnOkClick()
        {
            currentName = playerNameField.text;
            PlayerSave sd = new PlayerSave(currentName);
            sd.SaveFile();
            // TODO remove events when done
            sd.OnAccountRequestFinished += (s, e) => GameInfo.info.RunOnMainThread(() => FinishedAccountRequest(sd, e));
            sd.OnLoginFinished += (s, e) => GameInfo.info.RunOnMainThread(() => FinishedLoginRequest(sd, e));
            sd.StartCreate(playerPassField.text, playerMailField.text);

            SetInteractive(false);
        }

        private void FinishedAccountRequest(PlayerSave sd, EventArgs<string> e)
        {
            if (e.Error)
            {
                SetInteractive(true);

                ErrorWindow window = (ErrorWindow) GameMenu.SingletonInstance.AddWindow(Window.ERROR);
                window.SetText(e.ErrorText);
            }
        }

        private void FinishedLoginRequest(PlayerSave sd, EventArgs<string> e)
        {
            SetInteractive(true);

            // TODO actually log in player? maybe not? idk
            if (e.Error)
            {
                ErrorWindow window = (ErrorWindow) GameMenu.SingletonInstance.AddWindow(Window.ERROR);
                window.SetText(e.ErrorText);
            }
        }
    }
}
