using System;
using System.Collections.Generic;
using  System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DebugWindow : MonoBehaviour
    {
        [SerializeField] private Text displayText;

        private float lastFps = 0f;
        private float lastFpsRecordTime = -1f;

        private List<Func<string>> displayActions = new List<Func<string>>();

        private void Start()
        {
            AddDisplayAction(() =>
            {
                //Update fps every 0.1 seconds
                if (lastFpsRecordTime + 0.1f < Time.time || lastFpsRecordTime < 0f)
                {
                    lastFps = Mathf.RoundToInt(1 / Time.smoothDeltaTime);
                    lastFpsRecordTime = Time.time;
                }
                return lastFps.ToString("0") + " FPS";
            });
        }

        private void Update()
        {
            displayText.text = "";
            displayActions.ForEach(action => displayText.text = action() + '\n');
        }

        public void AddDisplayAction(Func<string> action)
        {
            displayActions.Add(action);
        }
    }
}
