﻿using System.Collections;
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
        [SerializeField] private Text timeText;
        [SerializeField] private Text pbText;
        [SerializeField] private Text rankText;
        [SerializeField] private AnimationCurve textFadeCurve;
        [SerializeField] private float fadeDuration = 5f;
        [SerializeField] private float textMoveDistance = 100f;

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
            StartCoroutine(FadeText(1f, fadeDuration, textMoveDistance, timeText));
            if (isPb)
            {
                Instantiate(pbMedalPrefab, medalTransform);
                StartCoroutine(FadeText(3f, fadeDuration, textMoveDistance, pbText));
            }
        }

        public void NewOnlineRank(int rank)
        {
            replaceTargets.ForEach(textDisplay => textDisplay.text = textDisplay.text.Replace("$rank", rank.ToString()));
            StartCoroutine(FadeText(5f, fadeDuration, textMoveDistance, rankText));
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

        private IEnumerator FadeText(float delay, float visibleTime, float moveDistance, Text text)
        {
            yield return new WaitForSeconds(delay);
            float startTime = Time.time;
            Vector3 startPosition = text.transform.localPosition;
            while (Time.time < startTime + visibleTime)
            {
                float completionPercent = (Time.time - startTime) / visibleTime;
                float alpha = textFadeCurve.Evaluate(completionPercent);
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                text.transform.localPosition = startPosition + new Vector3(completionPercent * moveDistance, 0f, 0f);
                yield return false;
            }
            text.transform.localPosition = startPosition;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
        }
    }
}
