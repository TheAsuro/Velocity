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

        [SerializeField]
        private Text resultText;

        public void OnOkClick()
        {
            currentName = playerNameField.text;
            PlayerSave sd = new PlayerSave(currentName);
            // TODO remove events when done
            sd.OnAccountRequestFinished += (s, e) => FinishedAccountRequest(sd, e);
            sd.OnLoginFinished += (s, e) => FinishedLoginRequest(sd, e);
            sd.StartCreate(playerPassField.text, playerMailField.text);

            SetInteractive(false);
        }

        private void FinishedAccountRequest(PlayerSave sd, EventArgs<string> e)
        {
            if (e.Error)
            {
                resultText.text = e.ErrorText;
                SetInteractive(true);
            }
        }

        private void FinishedLoginRequest(PlayerSave sd, EventArgs<string> e)
        {
            SetInteractive(true);

            if (e.Error)
            {
                resultText.text = e.ErrorText;
            }
        }
    }
}
