using System;
using UnityEngine;

namespace Race
{
    public class DemoRaceScript : MonoBehaviour, RaceScript
    {
        public event EventHandler OnStart;
        public event EventHandler OnReset;

        public MovementBehaviour Movement { get; private set; }
        public bool RunValid { get; private set; }
        public float UnfreezeTime { get; private set; }
        public TimeSpan ElapsedTime { get; private set; }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Unpause()
        {
            throw new NotImplementedException();
        }

        public void PrepareNewRun()
        {
            Destroy(gameObject);
        }
    }
}