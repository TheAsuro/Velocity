using System.Collections.Generic;
using Demos;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class EndLevelWindow : DefaultMenuWindow
    {
        [SerializeField] private List<Text> replaceTargets;
        [SerializeField] private Transform medalTransform;
        [SerializeField] private GameObject pbMedalPrefab;

        private Demo demo;

        public void Initialize(Demo demo, bool isPb)
        {
            this.demo = demo;
            PlayRaceDemo();
            replaceTargets.ForEach(textDisplay =>
            {
                textDisplay.text = textDisplay.text
                    .Replace("$time", demo.TotalTickTime.ToTimeString())
                    .Replace("$level", SceneManager.GetActiveScene().name);
            });
            if (isPb)
            {
                Instantiate(pbMedalPrefab, medalTransform);
            }
        }

        public void NewOnlineRank(int rank)
        {
            replaceTargets.ForEach(textDisplay => textDisplay.text = textDisplay.text.Replace("$rank", rank.ToString()));
        }

        public void RestartRun()
        {
            GameMenu.SingletonInstance.CloseAllWindows();
            WorldInfo.info.CreatePlayer(false);
        }

        public void PlayRaceDemo()
        {
            WorldInfo.info.PlayDemo(demo, true, true);
        }

        public void SaveLastDemo()
        {
            demo.SaveToFile();
        }

        public void ToMainMenu()
        {
            GameInfo.info.LoadMainMenu();
        }

        public override void OnClose()
        {
            base.OnClose();
            WorldInfo.info.StopDemo();
        }
    }
}
