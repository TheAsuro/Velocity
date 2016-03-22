using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BugReport : MonoBehaviour
{
    const string BUG_REPORT_URL = "http://theasuro.de/asuro/Velocity/Api/bugreport.php";

    [SerializeField]
    private Button sendButton;

    [SerializeField]
    private Text sendInfo;

    [SerializeField]
    private InputField userNameField;

    [SerializeField]
    private InputField bugReportMessageField;

    void Awake()
    {
        sendButton.onClick.AddListener(OnSendClick);
    }

    void OnSendClick()
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

        var data = new Dictionary<string, string>();
        data.Add("user", userNameField.text);
        data.Add("report", bugReportMessageField.text);

        Api.HttpApi.StartRequest(BUG_REPORT_URL, "POST", OnMessageSent, data);
        sendInfo.text = "Sending...";
    }

    void OnMessageSent(Api.HttpApi.ApiResult result)
    {
        if (result.error)
            sendInfo.text = result.errorText;
        else
            sendInfo.text = "Message sent.";
    }
}
