using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demos
{
    public class DemoRecord : MonoBehaviour
    {
        private List<DemoTick> tickList;
        private bool recording = false;
        private Demo completeDemo;
        private string playerName;
        private string levelName;
        private float startPlayTime;

        private void FixedUpdate()
        {
            if(recording)
            {
                DemoTick tick = new DemoTick((decimal)(Time.time - startPlayTime), transform.position, transform.FindChild("Camera").rotation);
                tickList.Add(tick);
            }
        }

        public void StartDemo(string pPlayerName)
        {
            startPlayTime = Time.time;
            tickList = new List<DemoTick>();
            playerName = pPlayerName;
            levelName = SceneManager.GetActiveScene().name;
            recording = true;
        }

        public void StopDemo()
        {
            recording = false;
            completeDemo = new Demo(tickList, playerName, levelName);
        }

        public Demo GetDemo()
        {
            return completeDemo;
        }
    }
}