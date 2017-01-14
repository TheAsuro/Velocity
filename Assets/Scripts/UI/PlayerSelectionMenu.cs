using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerSelectionMenu : MainSubMenu
    {
        public EventHandler loginFinished;
        public EventHandler openRegisterMenu;

        [SerializeField]
        private InputField nameField;

        [SerializeField]
        private InputField passField;

        [SerializeField]
        private Text errorTextField;

        public void OnLoginClick()
        {
            GameInfo.info.CurrentSave = new SaveData(nameField.text);
            GameInfo.info.CurrentSave.Account.StartLogin(passField.text);
            GameInfo.info.CurrentSave.Account.OnLoginFinished += (sender, e) =>
            {
                if (!e.Error)
                    loginFinished(sender, e);
                else
                    errorTextField.text = e.ErrorText;
            };
        }

        public void OnOfflineClick()
        {
            GameInfo.info.CurrentSave = new SaveData(nameField.text);
            if (loginFinished != null)
                loginFinished(this, null);
            else
                throw new InvalidOperationException("MainMenu should always register on the LoginFinished event");
        }

        public void OnRegisterClick()
        {
            openRegisterMenu(this, null);
        }
    }
}
