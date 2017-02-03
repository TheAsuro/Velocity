using System;
using Game;
using Race;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class PlayWindow : DefaultMenuWindow
    {
        private Text timeText;
        private Text speedText;
        private Text nameText;
        private Text countdownText;
        private GameObject wrDisplay;

        private void Awake()
        {
            timeText = transform.Find("Time").Find("Text").GetComponent<Text>();
            speedText = transform.Find("Speed").Find("Text").GetComponent<Text>();
            nameText = transform.Find("Player").Find("Text").GetComponent<Text>();
            countdownText = transform.Find("Countdown").Find("Text").GetComponent<Text>();
            wrDisplay = transform.Find("WR").gameObject;
        }

        private void Update()
        {
            Assert.IsNotNull(Rs);
            Assert.IsNotNull(Rs.Movement);

            //Display time
            timeText.text = TimeString;
            timeText.color = Rs.RunValid ? Color.white : Color.red;

            //Display speed+
            speedText.text = Rs.Movement.GetXzVelocityString() + " m/s";

            //Display player name
            nameText.text = GameInfo.info.InEditor ? "-" : PlayerSave.current.Name;

            //countdown
            float remainingFreezeTime = Rs.UnfreezeTime - Time.time;
            if (remainingFreezeTime > 0f)
            {
                countdownText.gameObject.transform.parent.gameObject.SetActive(true);
                wrDisplay.SetActive(true);
                countdownText.text = Mathf.Ceil(remainingFreezeTime).ToString();
            }
            else if (remainingFreezeTime > -1f)
            {
                countdownText.gameObject.transform.parent.gameObject.SetActive(true);
                wrDisplay.SetActive(true);
                countdownText.text = "GO!";
            }
            else
            {
                countdownText.gameObject.transform.parent.gameObject.SetActive(false);
                wrDisplay.SetActive(false);
            }
        }

        private static RaceScript Rs
        {
            get { return WorldInfo.info.RaceScript; }
        }

        private static string TimeString
        {
            get { return (new DateTime(Rs.ElapsedTime.Ticks)).ToString("mm:ss.ffff"); }
        }
    }
}