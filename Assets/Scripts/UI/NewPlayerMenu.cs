using UnityEngine;
using UnityEngine.UI;
using Api;
using System;

public class NewPlayerMenu : MainSubMenu
{
    public EventHandler<EventArgs<string>> OnCreatedNewPlayer;

    private string currentName;

    [SerializeField]
    private InputField playerNameField;

    [SerializeField]
    private InputField playerPassField;

    [SerializeField]
    private InputField playerMailField;

    [SerializeField]
    private Text resultText;

    [SerializeField]
    private Button okButton;

    void Awake()
    {
        okButton.onClick.AddListener(OnOkClick);
    }

    private void OnOkClick()
    {
        currentName = playerNameField.text;
        SaveData sd = new SaveData(currentName);
        // TODO remove events when done
        sd.Account.OnAccountRequestFinished += (s, e) => FinishedAccountRequest(sd, e);
        sd.Account.OnLoginFinished += (s, e) => FinishedLoginRequest(sd, e);
        sd.Account.StartCreate(playerPassField.text, playerMailField.text);

        SetInteractive(false);
    }

    private void SetInteractive(bool value)
    {
        playerNameField.interactable = value;
        playerPassField.interactable = value;
        playerMailField.interactable = value;
        okButton.interactable = value;
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
            if (OnCreatedNewPlayer != null)
                OnCreatedNewPlayer(this, new EventArgs<string>(currentName));
        }
        else
        {
            resultText.text = e.ErrorText;
        }
    }
}
