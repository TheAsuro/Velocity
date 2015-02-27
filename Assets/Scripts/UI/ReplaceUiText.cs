using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReplaceUiText : MonoBehaviour
{
    private string originalText;
    private string text
    {
        get { return GetComponent<Text>().text; }
        set { GetComponent<Text>().text = value; }
    }

    void Awake()
    {
        originalText = text;
    }

    void Update()
    {
        string tempText = originalText;

        //$currentplayer
        SaveData sd = GameInfo.info.getCurrentSave();
        if (sd == null)
            tempText.Replace("$currentplayer", "No player selected!");
        else
            tempText.Replace("$currentplayer", sd.getPlayerName());

        //$playerX
        if(PlayerPrefs.HasKey("PlayerName1"))
            tempText.Replace("$player1", PlayerPrefs.GetString("PlayerName1"));
        if (PlayerPrefs.HasKey("PlayerName2"))
            tempText.Replace("$player2", PlayerPrefs.GetString("PlayerName2"));
        if (PlayerPrefs.HasKey("PlayerName3"))
            tempText.Replace("$player3", PlayerPrefs.GetString("PlayerName3"));

        //Apply
        text = tempText;
    }
}
