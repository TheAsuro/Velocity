using System;
using System.Diagnostics;
using Api;
using Demos;
using Game;
using UI;
using UnityEngine;
using Util;

namespace Race
{
    public class GameRaceScript : MonoBehaviour, RaceScript
    {
        public event EventHandler OnStart;
        public event EventHandler OnReset;

        public MovementBehaviour Movement { get; private set; }
        public float UnfreezeTime { get; private set; }
        public bool RunVaild { get; private set; }

        private int checkpoint = -1;
        private bool started = false;
        private bool finished = false;
        private bool paused = false;

        private float freezeDuration = 3f;
        private float lastSecondGame;
        private DateTime lastSecondComputer;

        private Checkpoint firstSpawn;
        private Checkpoint currentSpawn;

        private Stopwatch playTime;
        private DemoRecord demoRecorder;

        public TimeSpan ElapsedTime
        {
            get { return playTime == null ? TimeSpan.MaxValue : playTime.Elapsed; }
        }

        private void Awake()
        {
            UnfreezeTime = float.PositiveInfinity;
            Movement = GetComponent<MovementBehaviour>();
            demoRecorder = GetComponent<DemoRecord>();
            firstSpawn = WorldInfo.info.FirstSpawn;
            currentSpawn = firstSpawn;
            WorldInfo.info.OnCheckpointTrigger += CheckpointHit;
        }

        private void Update()
        {
            //Freeze time is up, start the race!
            if (!started && paused && (Time.time >= UnfreezeTime || Input.GetButtonDown("Skip")))
            {
                StartRace();
            }

            if (Input.GetButtonDown("Respawn"))
            {
                ResetToLastCheckpoint();
            }
            if (Input.GetButtonDown("Reset"))
            {
                PrepareNewRace();
            }

            InvalidRunCheck();
        }

        private void FixedUpdate()
        {
            //Restart if below map
            if (transform.position.y <= WorldInfo.info.WorldData.deathHeight)
                ResetToLastCheckpoint();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag.Equals("Kill"))
            {
                ResetToLastCheckpoint();
            }
        }

        public void CheckpointHit(object sender, EventArgs<Checkpoint> eventArgs)
        {
            int index = eventArgs.Content.Index;
            if (WorldInfo.info.IsLastCheckpoint(index) && index == checkpoint + 1 && !finished) //End
            {
                EndRace();
            }
            else if (index == checkpoint + 1) //next checkpoint
            {
                checkpoint++;
                GameMenu.SingletonInstance.ShowTextBox("#" + checkpoint + ": " + ElapsedTime.ShortText(), Color.blue);
            }
            else
            {
                GameMenu.SingletonInstance.ShowTextBox("Wrong Checkpoint!", Color.red);
            }
        }

        //Starts a new race (resets the current one if there is one)
        public void PrepareNewRace()
        {
            if (OnReset != null)
                OnReset(this, new EventArgs());

            checkpoint = 0;
            started = false;
            finished = false;
            freezeDuration = 3f;
            paused = true;

            currentSpawn = firstSpawn;
            transform.position = firstSpawn.GetSpawnPos();
            transform.rotation = firstSpawn.GetSpawnRot();

            playTime = new Stopwatch();

            if (freezeDuration > 0f)
            {
                Pause();
                UnfreezeTime = Time.time + freezeDuration;
            }
            else
            {
                StartRace();
            }
        }

        private void StartRace()
        {
            started = true;
            UnfreezeTime = Time.time;
            lastSecondComputer = DateTime.Now;
            lastSecondGame = Time.time;

            Unpause();
            demoRecorder.StartDemo(GameInfo.info.CurrentSave.Name);

            if (OnStart != null)
                OnStart(this, new EventArgs());
        }

        private void EndRace()
        {
            playTime.Stop();

            finished = true;
            demoRecorder.StopDemo();

            GameInfo.info.RunFinished(ElapsedTime, demoRecorder.GetDemo());
        }

        public void Pause()
        {
            playTime.Stop();
            paused = true;
            Movement.Freeze();
        }

        public void Unpause()
        {
            if (started)
            {
                playTime.Start();
                paused = false;
                Movement.Unfreeze();
            }
        }

        public void ResetToLastCheckpoint()
        {
            if (currentSpawn == firstSpawn)
                PrepareNewRace();
            else
                throw new NotImplementedException();
        }

        private void InvalidRunCheck()
        {
            if (GameInfo.info.CheatsActive)
                RunVaild = false;

            if (started && Time.time > lastSecondGame + 1f)
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