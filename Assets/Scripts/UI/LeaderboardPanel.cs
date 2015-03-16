using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class LeaderboardPanel : MonoBehaviour
{
    private Text timeObj;
    private Text playerObj;
    private Text rankObj;
    private Button donwloadButtonObj;

    void Awake()
    {
        timeObj = transform.FindChild("Time").GetComponent<Text>();
        playerObj = transform.FindChild("Player").GetComponent<Text>();
        rankObj = transform.FindChild("Rank").GetComponent<Text>();
        donwloadButtonObj = transform.FindChild("GetDemo").GetComponent<Button>();
    }

    public string time
    {
        get { return timeObj.text; }
        set { timeObj.text = value; }
    }
    public string player
    {
        get { return playerObj.text; }
        set { playerObj.text = value; }
    }
    public string rank
    {
        get { return rankObj.text; }
        set { rankObj.text = value; }
    }
    
    public void SetButtonAction(UnityAction action)
    {
        donwloadButtonObj.onClick.AddListener(action);
    }

    public void SetButtonActive(bool value)
    {
        donwloadButtonObj.interactable = value;
    }
}
