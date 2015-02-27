using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReplaceUiText : MonoBehaviour
{
    private Text textScript
    {
        get { return GetComponent<Text>(); }
    }

    private string initialText = "";
    private SaveData player1, player2, player3;
    private string wr = "";
    private bool loadingWr = false;
    private string pb = "";

    void Start()
    {
        initialText = textScript.text;

        init();
    }

    public void init()
    {
        player1 = new SaveData(1);
        player2 = new SaveData(2);
        player3 = new SaveData(3);
    }

    void Update()
    {
        string temp = initialText;
        SaveData playerSave = GameInfo.info.getCurrentSave();

        if (temp.Contains("$player1")) { temp = temp.Replace("$player1", player1.getPlayerName()); }
        else if (temp.Contains("$player2")) { temp = temp.Replace("$player2", player2.getPlayerName()); }
        else if (temp.Contains("$player3")) { temp = temp.Replace("$player3", player3.getPlayerName()); }
        else if (temp.Contains("$player") && playerSave != null) { temp = temp.Replace("$player", playerSave.getPlayerName()); }
        if (temp.Contains("$time")) { temp = temp.Replace("$time", (GameInfo.info.getLastTime() / 10000000D).ToString()); }
        if (temp.Contains("$map")) { temp = Application.loadedLevelName; }

        if (temp.Contains("$selectedmap")) { temp = temp.Replace("$selectedmap", GameInfo.info.getSelectedMap()); }

        if (temp.Contains("$selectedauthor"))
        {
            string aut = GameInfo.info.getSelectedAuthor();
            if (!aut.Equals("?"))
            {
                temp = temp.Replace("$selectedauthor", "by " + aut);
            }
            else
            {
                temp = temp.Replace("$selectedauthor", "");
            }
        }

        if (temp.Contains("$wr"))
        {
            if (wr.Equals("") && !loadingWr)
                loadWr();
        }

        if (pb.Equals("") && playerSave != null)
        {
            pb = GameInfo.info.getCurrentSave().getPersonalBest(Application.loadedLevelName).ToString();
            if (pb == "0") { pb = "-"; }
        }

        if (temp.Contains("$wr")) { temp = temp.Replace("$wr", wr); }
        if (temp.Contains("$pb")) { temp = temp.Replace("$pb", pb); }

        if (temp.Contains("$currentplayer")) { temp = temp.Replace("$currentplayer", playerSave.getPlayerName()); }

        textScript.text = temp;
    }

    private void loadWr()
    {
        loadingWr = true;
        GameInfo.info.loadMapRecord(Application.loadedLevelName, setWr);
    }

    private void setWr(string text)
    {
        loadingWr = false;
        string temp = text.Split('\n')[0];
        string[] temp2 = temp.Split('|');

        if (temp2.Length == 2)
            wr = temp2[1] + " by " + temp2[0];
    }
}