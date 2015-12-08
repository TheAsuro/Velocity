using UnityEngine;
using UnityEngine.UI;
using Api;
using System;

public class NewPlayerMenu : MonoBehaviour
{
    public EventHandler<EventArgs<int>> OnCreatedNewPlayer;

    public int currentIndex = 0;

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
        SaveData sd = new SaveData(currentIndex, playerNameField.text);
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
                OnCreatedNewPlayer(this, new EventArgs<int>(currentIndex));
        }
        else
        {
            resultText.text = e.ErrorText;
        }
    }
}
