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
	private Text wrText;

    private Stopwatch playTime;
    private TimeSpan elapsedTime
    {
        get
        {
            if (playTime == null)
                return TimeSpan.MaxValue;
            else
                return playTime.Elapsed;
        }
    }
    private string timeString
    {
        get { return (new DateTime(elapsedTime.Ticks)).ToString("mm:ss.ffff"); }
    }
	
	//Handle player GUI
	void Awake()
	{
		Transform canvas = gameObject.transform.parent.Find("Canvas");
		timeText = canvas.Find("Time").Find("Text").GetComponent<Text>();
		speedText = canvas.Find("Speed").Find("Text").GetComponent<Text>();
		nameText = canvas.Find("Player").Find("Text").GetComponent<Text>();
		countdownText = canvas.Find("Countdown").Find("Text").GetComponent<Text>();
		wrText = canvas.Find("WR").Find("Text").GetComponent<Text>();
	}

	void Update()
	{
		//Freeze time is up, start the race!
		if(!started && paused && Time.time >= unfreezeTime)
		{
			startRace();
		}

        //Check time validity every second
        if(started && Time.time > lastSecondGame + 1f)
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
		timeText.text = timeString;
        if (GameInfo.info.IsRunValid())
            timeText.color = Color.white;
        else
            timeText.color = Color.red;

		//Display speed
		speedText.text = GameInfo.info.getPlayerInfo().getCurrentSpeed().ToString() + " m/s";

		//Display player name
		if(!editorMode)
			nameText.text = GameInfo.info.CurrentSave.Account.Name;
		else
			nameText.text = "Editor";

		//Skip countdown with use key
		if(Input.GetButtonDown("Skip") && !started)
		{
			startRace();
		}

		//countdown
		float remainingFreezeTime = unfreezeTime - Time.time;
		if(remainingFreezeTime > 0f)
		{
			countdownText.gameObject.transform.parent.gameObject.SetActive(true);
			wrText.gameObject.transform.parent.gameObject.SetActive(true);
			countdownText.text = Mathf.Ceil(remainingFreezeTime).ToString();
		}
		else if(remainingFreezeTime > -1f)
		{
			countdownText.gameObject.transform.parent.gameObject.SetActive(true);
			wrText.gameObject.transform.parent.gameObject.SetActive(true);
			countdownText.text = "GO!";
		}
		else
		{
			countdownText.gameObject.transform.parent.gameObject.SetActive(false);
			wrText.gameObject.transform.parent.gameObject.SetActive(false);
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag.Equals("Checkpoint"))
		{
			//Get checkpoint info
			Checkpoint cp = other.GetComponent<Checkpoint>();
			int nr = cp.checkpointNumber;
			bool end = cp.isEnd;
			
			if(end && nr == checkpoint + 1 && !finished) //End
			{
                endRace();
			}
			else if(nr == checkpoint + 1) //next checkpoint
			{
				checkpoint++;
			}
		}
	}

	//Starts a new race (resets the current one if there is one)
	public void prepareRace(float newFreezeDuration = 0f)
	{
		checkpoint = 0;
        started = false;
		finished = false;
		freezeDuration = newFreezeDuration;
        paused = true;

        playTime = new Stopwatch();

		if(freezeDuration > 0f)
		{
            pause();
            unfreezeTime = Time.time + freezeDuration;
		}
        else
        {
            startRace();
        }

		GameInfo.info.startDemo();
	}

	private void startRace()
	{
        started = true;
        WorldInfo.info.DoStart();
        unpause();
        unfreezeTime = Time.time;
        lastSecondComputer = DateTime.Now;
        lastSecondGame = Time.time;
	}

    private void endRace()
    {
        playTime.Stop();
        finished = true;
        GameInfo.info.runFinished(elapsedTime);
    }

    public void pause()
    {
        playTime.Stop();
        paused = true;
        GameInfo.info.getPlayerInfo().freeze();
    }

    public void unpause()
    {
        if(started)
        {
            playTime.Start();
            paused = false;
            GameInfo.info.getPlayerInfo().unfreeze();
        }
    }

	public void setEditorMode(bool value)
	{
		editorMode = value;
	}

	public bool getEditorMode()
	{
		return editorMode;
	}
}