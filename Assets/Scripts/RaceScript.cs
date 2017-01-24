using UnityEngine;
using System;
using System.Diagnostics;
using Demos;
using Movement;
using Input = UnityEngine.Input;

//Interacts with checkpoint triggers, must be added to every player
public class RaceScript : MonoBehaviour
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

    private Respawn firstSpawn;
    private Respawn currentSpawn;

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
    }

    private void Update()
    {
        //Freeze time is up, start the race!
        if (!started && paused && (Time.time >= UnfreezeTime || Input.GetButtonDown("Skip")))
        {
            StartRace();
        }

        //Check time validity every second
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
        //Kill player if below map
        if (transform.position.y <= WorldInfo.info.WorldData.deathHeight)
            ResetToLastCheckpoint();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Checkpoint"))
        {
            //Get checkpoint info
            Checkpoint cp = other.GetComponent<Checkpoint>();
            int nr = cp.checkpointNumber;
            bool end = cp.isEnd;

            if (end && nr == checkpoint + 1 && !finished) //End
            {
                EndRace();
            }
            else if (nr == checkpoint + 1) //next checkpoint
            {
                checkpoint++;
            }
        }
        else if (other.tag.Equals("Kill"))
        {
            ResetToLastCheckpoint();
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

        // TODO - set player position

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
        demoRecorder.StartDemo(GameInfo.info);

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
            WorldInfo.info.RaceScript.PrepareNewRace();
        else
            throw new NotImplementedException();
    }

    private void InvalidRunCheck()
    {
        if (GameInfo.info.CheatsActive || Physics.gravity != new Vector3(0f, -15f, 0f))
            RunVaild = false;
    }
}