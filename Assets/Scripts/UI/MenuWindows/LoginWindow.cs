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
            if (!PlayerSave.PlayerFileExists(nameField.text))
            {
                errorTextField.text = "No save file for this player!";
                return;
            }

            SetInteractive(false);

            PlayerSave.current = PlayerSave.LoadFromFile(nameField.text);
            PlayerSave.current.StartLogin(passField.text);
            PlayerSave.current.OnLoginFinished += (sender, e) =>
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
            if (nameField.text == "")
            {
                errorTextField.text = "Please enter a name for the offline player.";
                return;
            }

            if (PlayerSave.PlayerFileExists(nameField.text))
                PlayerSave.current = PlayerSave.LoadFromFile(nameField.text);
            else
                PlayerSave.current = new PlayerSave(nameField.text);

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
