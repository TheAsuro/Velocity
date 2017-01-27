using Api;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;

namespace UI
{
    public class ReplaceUiText : MonoBehaviour
    {
        private Text TextScript
        {
            get { return GetComponent<Text>(); }
        }

        private string initialText = "";

        private string wr = "";
        private bool loadingWr = false;

        private string bestEntry = "";
        private bool loadingBestEntry = false;

        private string pb = "";

        private void Start()
        {
            initialText = TextScript.text;
        }

        private void Update()
        {
            string temp = initialText;
            SaveData playerSave = GameInfo.info.CurrentSave;

            if (temp.Contains("$player") && playerSave != null)
            {
                temp = temp.Replace("$player", playerSave.Account.Name);
            }
            if (temp.Contains("$time"))
            {
                temp = temp.Replace("$time", WorldInfo.info.RaceScript.ElapsedTime.ToString());
            }
            if (temp.Contains("$map"))
            {
                temp = SceneManager.GetActiveScene().name;
            }

            if (temp.Contains("$wr"))
            {
                if (wr.Equals("") && !loadingWr)
                    LoadWr();
            }

            if (pb.Equals("") && playerSave != null)
            {
                decimal pbTime = playerSave.GetPersonalBest(SceneManager.GetActiveScene().name);
                pb = pbTime <= 0 ? "-" : pbTime.ToString("0.0000");
            }

            if (temp.Contains("$wr"))
            {
                temp = temp.Replace("$wr", wr);
            }
            if (temp.Contains("$pb"))
            {
                temp = temp.Replace("$pb", pb);
            }

            if (temp.Contains("$ispb"))
            {
                temp = temp.Replace("$ispb", GameInfo.info.LastRunWasPb ? "New personal record!" : "");
            }

            if (temp.Contains("$rank"))
            {
                if (!loadingBestEntry && bestEntry == "")
                {
                    // TODO
                    loadingBestEntry = true;
                }
            }

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
            StartCoroutine(UnityUtils.RunWhenDone(Leaderboard.GetRecord(GameInfo.info.MapManager.CurrentMap), (request) =>
            {
                loadingWr = false;
                if (!request.Error && request.Result.Length > 0)
                {
                    LeaderboardEntry entry = request.Result[0];
                    wr = entry.time + " by " + entry.playerName;
                }
                else
                {
                    wr = "-";
                }
            }));
        }
    }
}