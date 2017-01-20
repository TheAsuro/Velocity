using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    internal class DisplayAction
    {
        public Func<string> Action { get; private set; }
        public GameObject Obj { get; private set; }
        public bool HasObject { get; private set; }

        public DisplayAction(Func<string> action)
        {
            Action = action;
            HasObject = false;
        }

        public DisplayAction(Func<string> action, GameObject obj)
        {
            Action = action;
            Obj = obj;
            HasObject = true;
        }
    }

    public class DebugWindow : MonoBehaviour
    {
        [SerializeField] private Text displayText;

        private float lastFps = 0f;
        private float lastFpsRecordTime = -1f;

        private List<DisplayAction> displayActions = new List<DisplayAction>();

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
            displayActions.RemoveAll(action => action.HasObject && action.Obj == null);
            displayActions.ForEach(action => displayText.text = action.Action() + '\n');
        }

        public void AddDisplayAction(Func<string> action, GameObject obj = null)
        {
            if (obj == null)
                displayActions.Add(new DisplayAction(action));
            else
                displayActions.Add(new DisplayAction(action, obj));
        }
    }
}