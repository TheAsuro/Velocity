using System.Collections.Generic;
using Api;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class BugReportWindow : DefaultMenuWindow
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

            ApiRequest rq = new ApiRequest(BUG_REPORT_URL, "POST", data);
            rq.OnDone += OnMessageSent;
            rq.StartRequest();
            sendInfo.text = "Sending...";
        }

        private void OnMessageSent(object sender, RequestFinishedEventArgs<string> args)
        {
            sendInfo.text = args.Error ? args.ErrorText : "Message sent.";
            if (!args.Error)
                GameMenu.SingletonInstance.CloseWindow();
        }
    }
}