using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReplaceUiText : MonoBehaviour
{
    private Text TextScript
    {
        get { return GetComponent<Text>(); }
    }

    private string initialText = "";
    private static SaveData player1, player2, player3;
    private string wr = "";
    private bool loadingWr = false;
    private string pb = "";

    void Start()
    {
        initialText = TextScript.text;
        ReplaceUiText.UpdateSaveInfo();
    }

    public static void UpdateSaveInfo()
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
        if (temp.Contains("$time")) { temp = temp.Replace("$time", (GameInfo.info.lastTimeString).ToString()); }
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
                LoadWr();
        }

        if (pb.Equals("") && playerSave != null)
        {
            decimal pbTime = GameInfo.info.getCurrentSave().getPersonalBest(Application.loadedLevelName);
            if (pbTime <= 0)
                pb = "-";
            else
                pb = pbTime.ToString("0.0000");
        }

        if (temp.Contains("$wr")) { temp = temp.Replace("$wr", wr); }
        if (temp.Contains("$pb")) { temp = temp.Replace("$pb", pb); }

        if (temp.Contains("$currentplayer"))
        {
            if (playerSave != null && !playerSave.getPlayerName().Equals(""))
                temp = temp.Replace("$currentplayer", playerSave.getPlayerName());
            else
                temp = temp.Replace("$currentplayer", "No player selected!");
        }

        if (temp.Contains("$load1"))
        {
            if (player1.getPlayerName().Equals(""))
                temp = temp.Replace("$load1", "New");
            else
                temp = temp.Replace("$load1", "Load");
        }
        if (temp.Contains("$load2"))
        {
            if (player2.getPlayerName().Equals(""))
                temp = temp.Replace("$load2", "New");
            else
                temp = temp.Replace("$load2", "Load");
        }
        if (temp.Contains("$load3"))
        {
            if (player3.getPlayerName().Equals(""))
                temp = temp.Replace("$load3", "New");
            else
                temp = temp.Replace("$load3", "Load");
        }

        TextScript.text = temp;
    }

    private void LoadWr()
    {
        loadingWr = true;
        Api.Leaderboard.GetRecord(Application.loadedLevelName, SetWr);
    }

    private void SetWr(Api.LeaderboardEntry entry)
    {
        loadingWr = false;
        wr = entry.time + " by " + entry.playerName;
    }
}