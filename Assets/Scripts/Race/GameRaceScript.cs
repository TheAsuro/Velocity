using System;
using System.Collections.Generic;
using System.Diagnostics;
using Demos;
using Game;
using UI;
using UnityEngine;
using UnityEngine.Assertions;
using Util;

namespace Race
{
    public class GameRaceScript : MonoBehaviour, RaceScript
    {
        public event EventHandler OnStart;
        public event EventHandler OnReset;

        public MovementBehaviour Movement { get; private set; }
        public float UnfreezeTime { get; private set; }

        public bool RunValid
        {
            get { return demoRecorder.IsValid; }
        }

        private int currentCheckpoint = 0;
        private bool started = false;
        private bool finished = false;
        private bool paused = false;

        private float freezeDuration = 3f;

        private Checkpoint firstSpawn;
        private Checkpoint currentSpawn;

        private Stopwatch stopwatch;
        private List<long> checkpointTimes;
        private DemoRecorder demoRecorder;

        public TimeSpan ElapsedTime
        {
            get { return stopwatch == null ? TimeSpan.MaxValue : stopwatch.Elapsed; }
        }

        private void Awake()
        {
            UnfreezeTime = float.PositiveInfinity;
            Movement = GetComponent<MovementBehaviour>();
            demoRecorder = GetComponent<DemoRecorder>();
            firstSpawn = WorldInfo.info.FirstSpawn;
            currentSpawn = firstSpawn;
            checkpointTimes = new List<long>();
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
                PrepareNewRun();
            }

            if (started)
                demoRecorder.InvalidRunCheck();
        }

        private void FixedUpdate()
        {
            if (started && !paused && !finished)
                demoRecorder.AddTick(stopwatch.ElapsedTicks);
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
            if (WorldInfo.info.IsLastCheckpoint(index) && IsNextCheckpoint(index) && !finished)
            {
                // End of race
                stopwatch.Stop();
                demoRecorder.Finish(stopwatch.ElapsedTicks);
                finished = true;

                GameInfo.info.RunFinished(checkpointTimes.ToArray(), demoRecorder.Demo);

                Destroy(gameObject);
            }
            else if (IsNextCheckpoint(index))
            {
                // Hit next checkpoint
                long checkpointTime = stopwatch.ElapsedTicks;

                demoRecorder.AddCheckpoint(checkpointTime);
                checkpointTimes.Add(checkpointTime);

                long[] pbTime;
                List<DisplayText> lines = new List<DisplayText>();
                lines.Add(new DisplayText("#" + index + ": " + ElapsedTime.ToShortTimeString(), Color.blue));


                if (PlayerSave.current.GetPersonalBest(GameInfo.info.MapManager.CurrentMap, out pbTime))
                {
                    Assert.IsTrue(pbTime.Length >= currentCheckpoint);

                    string pbComparisonString = "";
                    long tickDifference = checkpointTime - pbTime[currentCheckpoint];
                    Color textColor;

                    if (tickDifference > 0)
                    {
                        pbComparisonString += "+ ";
                        textColor = Color.red;
                    }
                    else
                    {
                        pbComparisonString += "- ";
                        textColor = Color.blue;
                    }

                    pbComparisonString += Math.Abs(tickDifference).ToShortTimeString();
                    lines.Add(new DisplayText(pbComparisonString, textColor));
                }

                GameMenu.SingletonInstance.ShowTextBox(lines, 1.5f);
                currentCheckpoint++;
            }
            else if (index != currentCheckpoint)
            {
                GameMenu.SingletonInstance.ShowTextBox("Wrong Checkpoint!", Color.red);
            }
        }

        public void Pause()
        {
            stopwatch.Stop();
            paused = true;
            Movement.Freeze();
        }

        public void Unpause()
        {
            if (started)
            {
                stopwatch.Start();
                paused = false;
                Movement.Unfreeze();
            }
        }

        public void ResetToLastCheckpoint()
        {
            if (currentSpawn == firstSpawn)
                PrepareNewRun();
            else
                throw new NotImplementedException();
        }

        //Starts a new race (resets the current one if there is one)
        public void PrepareNewRun()
        {
            if (OnReset != null)
                OnReset(this, new EventArgs());

            currentCheckpoint = 0;
            started = false;
            finished = false;
            freezeDuration = 3f;
            paused = true;
            checkpointTimes.Clear();

            currentSpawn = firstSpawn;
            transform.position = firstSpawn.GetSpawnPos();
            transform.rotation = firstSpawn.GetSpawnRot();

            stopwatch = new Stopwatch();

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

            Unpause();
            demoRecorder.StartRecording(PlayerSave.current.Name);

            if (OnStart != null)
                OnStart(this, new EventArgs());
        }

        private bool IsNextCheckpoint(int index)
        {
            if (demoRecorder == null)
                return false;

            return currentCheckpoint + 1 == index;
        }
    }
}