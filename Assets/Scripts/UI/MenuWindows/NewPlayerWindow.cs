using System;
using System.Linq;
using Api;
using Game;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField]
        private Text resultText;

        public void OnOkClick()
        {
            currentName = playerNameField.text;
            SaveData sd = new SaveData(currentName);
            // TODO remove events when done
            sd.Account.OnAccountRequestFinished += (s, e) => FinishedAccountRequest(sd, e);
            sd.Account.OnLoginFinished += (s, e) => FinishedLoginRequest(sd, e);
            sd.Account.StartCreate(playerPassField.text, playerMailField.text);

            SetInteractive(false);
        }

        private void FinishedAccountRequest(SaveData sd, EventArgs<string> e)
        {
            if (e.Error)
            {
                resultText.text = e.ErrorText;
                SetInteractive(true);
            }
        }

        private void FinishedLoginRequest(SaveData sd, EventArgs<string> e)
        {
            SetInteractive(true);

            if (!e.Error)
            {
                GameInfo.info.CurrentSave = new SaveData(currentName);
            }
            else
            {
                resultText.text = e.ErrorText;
            }
        }
    }
}
