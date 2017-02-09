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

        private bool started = false;
        private bool finished = false;
        private bool paused = false;

        private float freezeDuration = 3f;

        private Checkpoint firstCheckpoint;
        private Checkpoint currentCheckpoint;

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
            firstCheckpoint = WorldInfo.info.FirstSpawn;
            currentCheckpoint = firstCheckpoint;
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
            if (WorldInfo.info.IsEndCheckpoint(eventArgs.Content) && IsNextCheckpoint(eventArgs.Content) && !finished)
            {
                // End of race
                stopwatch.Stop();
                demoRecorder.Finish(stopwatch.ElapsedTicks);
                finished = true;

                GameInfo.info.RunFinished(checkpointTimes.ToArray(), demoRecorder.Demo);

                Destroy(gameObject);
            }
            else if (IsNextCheckpoint(eventArgs.Content))
            {
                // Hit next checkpoint
                long checkpointTime = stopwatch.ElapsedTicks;

                demoRecorder.AddCheckpoint(checkpointTime);
                checkpointTimes.Add(checkpointTime);

                List<DisplayText> lines = new List<DisplayText>();
                lines.Add(new DisplayText("  " + ElapsedTime.ToShortTimeString(), Color.blue));

                long[] pbTime;
                if (PlayerSave.current.GetPersonalBest(GameInfo.info.MapManager.CurrentMap, out pbTime))
                {
                    Assert.IsTrue(pbTime.Length >= currentCheckpoint.Index);

                    string pbComparisonString = "";
                    long tickDifference = checkpointTime - pbTime[currentCheckpoint.Index];
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
                currentCheckpoint = eventArgs.Content;
            }
            else if (eventArgs.Content.Index != currentCheckpoint.Index)
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
            if (currentCheckpoint == firstCheckpoint)
                PrepareNewRun();
            else
            {
                transform.position = currentCheckpoint.GetSpawnPos();
                transform.rotation = currentCheckpoint.GetSpawnRot();
                Movement.ResetVelocity();
            }
        }

        //Starts a new race (resets the current one if there is one)
        public void PrepareNewRun()
        {
            if (OnReset != null)
                OnReset(this, new EventArgs());

            currentCheckpoint = firstCheckpoint;
            started = false;
            finished = false;
            freezeDuration = 3f;
            paused = true;
            checkpointTimes.Clear();

            currentCheckpoint = firstCheckpoint;
            transform.position = firstCheckpoint.GetSpawnPos();
            transform.rotation = firstCheckpoint.GetSpawnRot();

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

        private bool IsNextCheckpoint(Checkpoint cp)
        {
            if (demoRecorder == null)
                return false;

            return currentCheckpoint.Index + 1 == cp.Index;
        }
    }
}