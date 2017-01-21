using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Diagnostics;

//Interacts with checkpoint triggers, must be added to every player
public class RaceScript : MonoBehaviour
{
    private bool editorMode = false;

    private int checkpoint = -1;
    private bool started = false;
    private bool finished = false;
    private bool paused = false;

    private float freezeDuration = 3f;
    private float unfreezeTime = float.PositiveInfinity;
    private float lastSecondGame;
    private DateTime lastSecondComputer;

    private Text timeText;
    private Text speedText;
    private Text nameText;
    private Text countdownText;
    private GameObject wrDisplay;

    private Stopwatch playTime;

    private TimeSpan ElapsedTime
    {
        get { return playTime == null ? TimeSpan.MaxValue : playTime.Elapsed; }
    }

    private string TimeString
    {
        get { return (new DateTime(ElapsedTime.Ticks)).ToString("mm:ss.ffff"); }
    }

    //Handle player GUI
    private void Awake()
    {
        Transform canvas = gameObject.transform.parent.Find("Canvas");
        timeText = canvas.Find("Time").Find("Text").GetComponent<Text>();
        speedText = canvas.Find("Speed").Find("Text").GetComponent<Text>();
        nameText = canvas.Find("Player").Find("Text").GetComponent<Text>();
        countdownText = canvas.Find("Countdown").Find("Text").GetComponent<Text>();
        wrDisplay = canvas.Find("WR").gameObject;
    }

    private void Update()
    {
        //Freeze time is up, start the race!
        if (!started && paused && Time.time >= unfreezeTime)
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

        //Display time
        timeText.text = TimeString;
        timeText.color = GameInfo.info.IsRunValid() ? Color.white : Color.red;

        //Display speed
        speedText.text = GameInfo.info.GetPlayerInfo().GetCurrentSpeed().ToString() + " m/s";

        //Display player name
        nameText.text = editorMode ? "Editor" : GameInfo.info.CurrentSave.Account.Name;

        //Skip countdown with use key
        if (Input.GetButtonDown("Skip") && !started)
        {
            StartRace();
        }

        //countdown
        float remainingFreezeTime = unfreezeTime - Time.time;
        if (remainingFreezeTime > 0f)
        {
            countdownText.gameObject.transform.parent.gameObject.SetActive(true);
            wrDisplay.SetActive(true);
            countdownText.text = Mathf.Ceil(remainingFreezeTime).ToString();
        }
        else if (remainingFreezeTime > -1f)
        {
            countdownText.gameObject.transform.parent.gameObject.SetActive(true);
            wrDisplay.SetActive(true);
            countdownText.text = "GO!";
        }
        else
        {
            countdownText.gameObject.transform.parent.gameObject.SetActive(false);
            wrDisplay.SetActive(false);
        }
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
    }

    //Starts a new race (resets the current one if there is one)
    public void PrepareRace(float newFreezeDuration = 0f)
    {
        checkpoint = 0;
        started = false;
        finished = false;
        freezeDuration = newFreezeDuration;
        paused = true;

        playTime = new Stopwatch();

        if (freezeDuration > 0f)
        {
            Pause();
            unfreezeTime = Time.time + freezeDuration;
        }
        else
        {
            StartRace();
        }
    }

    private void StartRace()
    {
        started = true;
        WorldInfo.info.DoStart();
        Unpause();
        unfreezeTime = Time.time;
        lastSecondComputer = DateTime.Now;
        lastSecondGame = Time.time;
    }

    private void EndRace()
    {
        playTime.Stop();
        finished = true;
        GameInfo.info.RunFinished(ElapsedTime);
    }

    public void Pause()
    {
        playTime.Stop();
        paused = true;
        GameInfo.info.GetPlayerInfo().Freeze();
    }

    public void Unpause()
    {
        if (started)
        {
            playTime.Start();
            paused = false;
            GameInfo.info.GetPlayerInfo().Unfreeze();
        }
    }

    public void SetEditorMode(bool value)
    {
        editorMode = value;
    }

    public bool GetEditorMode()
    {
        return editorMode;
    }
}