using System.Collections.Generic;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BugReportWindow : MonoBehaviour, MenuWindow
    {
        private const string BUG_REPORT_URL = "https://theasuro.de/Velocity/Api/bugreport.php";

        [SerializeField] private Button sendButton;

        [SerializeField] private Text sendInfo;

        [SerializeField] private InputField userNameField;

        [SerializeField] private InputField bugReportMessageField;

        private void Awake()
        {
            sendButton.onClick.AddListener(OnSendClick);
        }

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

        private void OnSendClick()
        {
            if (userNameField.text == "")
            {
                sendInfo.text = "Username missing!";
                return;
            }

            if (bugReportMessageField.text == "")
            {
                sendInfo.text = "Bug Report missing!";
                return;
            }

            var data = new Dictionary<string, string> {{"user", userNameField.text}, {"report", bugReportMessageField.text}};

            Api.HttpApi.StartRequest(BUG_REPORT_URL, "POST", OnMessageSent, data);
            sendInfo.text = "Sending...";
        }

        private void OnMessageSent(Api.HttpApi.ApiResult result)
        {
            sendInfo.text = result.error ? result.errorText : "Message sent.";
            if (!result.error)
                GameMenu.SingletonInstance.CloseWindow();
        }
    }
}