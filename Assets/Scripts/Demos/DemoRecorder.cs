using System;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demos
{
    public class DemoRecorder : MonoBehaviour
    {
        public bool IsValid { get; private set; }
        public List<long> CheckpointTicks { get; private set; }
        public long FinalTimeTicks { get; private set; }
        public Demo Demo { get; private set; }

        private float lastSecondGame;
        private DateTime lastSecondComputer;

        private List<DemoTick> tickList;
        private string playerName;
        private string levelName;

        public void StartRecording(string playerName)
        {
            this.playerName = playerName;
            tickList = new List<DemoTick>();
            levelName = SceneManager.GetActiveScene().name;
            CheckpointTicks = new List<long>();
            lastSecondComputer = DateTime.Now;
            lastSecondGame = Time.time;
        }

        public void AddCheckpoint(long elapsedTicks)
        {
            CheckpointTicks.Add(elapsedTicks);
        }

        public void AddTick(long elapsedTicks)
        {
            tickList.Add(new DemoTick(elapsedTicks, transform.position, transform.FindChild("Camera").rotation));
        }

        public void Finish(long elapsedTicks)
        {
            FinalTimeTicks = elapsedTicks;
            Demo = new Demo(tickList, playerName, levelName);
        }

        public void InvalidRunCheck()
        {
            if (GameInfo.info.CheatsActive)
                IsValid = false;

            if (Time.time > lastSecondGame + 1f)
            {
                TimeSpan difference = DateTime.Now - lastSecondComputer;
                double offset = 1000 - difference.TotalMilliseconds;
                if (offset > 15 || offset < -15)
                {
                    // TODO this is broken (kinda)
                    //GameInfo.info.invalidateRun("Local time sync failed");
                }
                lastSecondGame = Time.time;
                lastSecondComputer = DateTime.Now;
            }
        }
    }
}