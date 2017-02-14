using System;
using Game;
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
            LoadPlayerFile(nameField.text);

            PlayerSave.current.OnLoginFinished += (sender, e) =>
            {
                SetInteractive(true);

                if (!e.Error)
                    LoginFinished();
                else
                    errorTextField.text = e.ErrorText;
            };
            errorTextField.text = "Logging in...";
            PlayerSave.current.StartLogin(passField.text);
        }

        public void OnOfflineClick()
        {
            if (nameField.text == "")
            {
                errorTextField.text = "Please enter a name for the offline player.";
                return;
            }

            LoadPlayerFile(nameField.text);
            LoginFinished();
        }

        private void LoadPlayerFile(string playerName)
        {
            if (PlayerSave.PlayerFileExists(playerName))
                PlayerSave.current = PlayerSave.LoadFromFile(playerName);
            else
            {
                PlayerSave.current = new PlayerSave(playerName);
                PlayerSave.current.SaveFile();
            }
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
