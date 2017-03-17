using System.Collections.Generic;
using Api;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class BugReportWindow : DefaultMenuWindow
    {
        // TODO: remove php
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

            StringRequestData data = new StringRequestData {{"user", userNameField.text}, {"report", bugReportMessageField.text}};

            ApiRequest rq = new ApiRequest(BUG_REPORT_URL, "POST", data);
            rq.StartRequest();
            StartCoroutine(UnityUtils.RunWhenDone(rq, OnMessageSent));
            sendInfo.text = "Sending...";
        }

        private void OnMessageSent(ApiRequest request)
        {
            sendInfo.text = request.Error ? request.ErrorText : "Message sent.";
            if (!request.Error)
                GameMenu.SingletonInstance.CloseWindow();
        }
    }
}