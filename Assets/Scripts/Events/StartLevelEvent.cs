using UnityEngine;

namespace Events
{
    public class StartLevelEvent : Event
    {
        public bool inEditor = false;
        public float introLength = 0f;
        public float startFreeze = 0f;
        public GameObject introDisplay;

        private float introStartTime;
        private bool waitForIntro = false;

        private void Start()
        {
            WorldInfo.info.AddResetMethod(Reset, "start level reset");
            if(!inEditor && !(GameInfo.info.loadMode == GameInfo.LevelLoadMode.DEMO))
                Fire(null);
        }

        public override void Fire(params object[] parameters)
        {
            if(introLength > 0f)
            {
                introStartTime = Time.time;
                waitForIntro = true;
                introDisplay.SetActive(true);
                StartIn(startFreeze + introLength);
            }
            else
            {
                StartIn(startFreeze);
            }
        }

        private void Update()
        {
            if(waitForIntro && Time.time > introStartTime + introLength)
            {
                waitForIntro = false;
                introDisplay.SetActive(false);
            }
        }

        public override void Reset()
        {
            StartIn(startFreeze);
        }

        private void StartIn(float delay)
        {
            GameInfo.info.SpawnNewPlayer(WorldInfo.info.GetFirstSpawn(), true, inEditor);
            GameInfo.info.GetPlayerInfo().PrepareRace(delay);
        }
    }
}
