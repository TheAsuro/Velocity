using System.Linq;
using Api;
using Game;
using UnityEngine;
using UnityEngine.Assertions;
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

        private string pb = "";

        private void Start()
        {
            initialText = TextScript.text;
        }

        private void Update()
        {
            string temp = initialText;
            temp = temp.ReplaceDefaultTemplates();

            if (temp.Contains("$time"))
            {
                temp = temp.Replace("$time", WorldInfo.info.RaceScript.ElapsedTime.ToString());
            }

            if (temp.Contains("$wr"))
            {
                if (wr.Equals("") && !loadingWr)
                    LoadWr();
            }

            if (pb.Equals("") && PlayerSave.current != null)
            {
                long[] pbTime;
                if (PlayerSave.current.GetPersonalBest(GameInfo.info.MapManager.CurrentMap, out pbTime))
                {
                    Assert.IsTrue(pbTime.Length > 0);
                    pb = pbTime.Last().ToTimeString();
                }
                else pb = "-";
            }

            if (temp.Contains("$wr"))
            {
                temp = temp.Replace("$wr", wr);
            }
            if (temp.Contains("$pb"))
            {
                temp = temp.Replace("$pb", pb);
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
                    wr = entry.Time + " by " + entry.PlayerName;
                }
                else
                {
                    wr = "-";
                }
            }));
        }
    }
}