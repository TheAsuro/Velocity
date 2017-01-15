using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class LoginWindow : MonoBehaviour, MenuWindow
    {
        [SerializeField]
        private InputField nameField;

        [SerializeField]
        private InputField passField;

        [SerializeField]
        private Text errorTextField;

        public void OnActivate()
        {
            gameObject.SetActive(true);
        }

        public void OnSetAsBackground()
        {
            gameObject.SetActive(false);
        }

        public void OnClose()
        {
            Destroy(gameObject);
        }

        public void OnLoginClick()
        {
            GameInfo.info.CurrentSave = new SaveData(nameField.text);
            GameInfo.info.CurrentSave.Account.StartLogin(passField.text);
            GameInfo.info.CurrentSave.Account.OnLoginFinished += (sender, e) =>
            {
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
            GameMenu.SingletonInstance.CloseWindow();
            // TODO GameMenu.SingletonInstance.AddWindow(Window.REGISTER);
        }

        private void LoginFinished()
        {
            GameMenu.SingletonInstance.CloseWindow();
            // TODO GameMenu.SingletonInstance.AddWindow(Window.something);
        }
    }
}
