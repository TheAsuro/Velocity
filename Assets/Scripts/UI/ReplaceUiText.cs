using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReplaceUiText : MonoBehaviour
{
    private Text TextScript
    {
        get { return GetComponent<Text>(); }
    }

    private string initialText = "";
    private string wr = "";
    private bool loadingWr = false;
    private string pb = "";

    void Start()
    {
        initialText = TextScript.text;
    }

    void Update()
    {
        string temp = initialText;
        SaveData playerSave = GameInfo.info.CurrentSave;
        
        if (temp.Contains("$player") && playerSave != null) { temp = temp.Replace("$player", playerSave.Account.Name); }
        if (temp.Contains("$time")) { temp = temp.Replace("$time", (GameInfo.info.lastTimeString).ToString()); }
        if (temp.Contains("$map")) { temp = SceneManager.GetActiveScene().name; }

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
            decimal pbTime = playerSave.GetPersonalBest(SceneManager.GetActiveScene().name);
            if (pbTime <= 0)
                pb = "-";
            else
                pb = pbTime.ToString("0.0000");
        }

        if (temp.Contains("$wr")) { temp = temp.Replace("$wr", wr); }
        if (temp.Contains("$pb")) { temp = temp.Replace("$pb", pb); }

        if (temp.Contains("$currentplayer"))
        {
            if (playerSave != null && !playerSave.Account.Name.Equals(""))
                temp = temp.Replace("$currentplayer", playerSave.Account.Name);
            else
                temp = temp.Replace("$currentplayer", "No player selected!");
        }

        TextScript.text = temp;
    }

    private void LoadWr()
    {
        loadingWr = true;
        Api.Leaderboard.GetRecord(SceneManager.GetActiveScene().name, SetWr);
    }

    private void SetWr(Api.LeaderboardEntry entry)
    {
        loadingWr = false;
        if (entry != null)
            wr = entry.time + " by " + entry.playerName;
        else
            wr = "-";
    }
}