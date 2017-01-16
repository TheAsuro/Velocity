using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class LoginWindow : DefaultMenuWindow
    {
        [SerializeField]
        private InputField nameField;

        [SerializeField]
        private InputField passField;

        [SerializeField]
        private Text errorTextField;

        public void OnLoginClick()
        {
            SetInteractive(false);

            GameInfo.info.CurrentSave = new SaveData(nameField.text);
            GameInfo.info.CurrentSave.Account.StartLogin(passField.text);
            GameInfo.info.CurrentSave.Account.OnLoginFinished += (sender, e) =>
            {
                SetInteractive(true);

                if (!e.Error)
                    LoginFinished();
                else
                    errorTextField.text = e.ErrorText;
            };
        }

        public void OnOfflineClick()
        {
            GameInfo.info.CurrentSave = new SaveData(nameField.text);
            LoginFinished();
        }

        public void OnRegisterClick()
        {
            GameMenu.SingletonInstance.AddWindow(Window.NEW_PLAYER);
        }

        private void LoginFinished()
        {
            GameMenu.SingletonInstance.CloseWindow();
            GameMenu.SingletonInstance.AddWindow(Window.GAME_SELECTION);
        }
    }
}
