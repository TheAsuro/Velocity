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
                GameInfo.info.invalidateRun();
                print("Invalidated run. Reason: Too big timing difference.");
            }
            lastSecondGame = Time.time;
            lastSecondComputer = DateTime.Now;
        }

		//Display time
		timeText.text = timeString;

		//Display speed
		speedText.text = GameInfo.info.getPlayerInfo().getCurrentSpeed().ToString() + " m/s";

		//Display player name
		if(!editorMode)
			nameText.text = GameInfo.info.getCurrentSave().getPlayerName();
		else
			nameText.text = "Editor";

		//Display crosshair
		drawCrosshair();

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

			//Save game (pretty sure i can remove this. TODO check if unnecessary)
			GameInfo.info.save();
			
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

	private void drawCrosshair()
	{
		//TODO put gameinfo circle speed in here and make a third circle

		PlayerInfo pi = GameInfo.info.getPlayerInfo();
		float speed = float.Parse(pi.getCurrentSpeed());
		float max1 = GameInfo.info.circleSpeed1;
		float max2 = GameInfo.info.circleSpeed2;
		float max3 = GameInfo.info.circleSpeed3;

		float lvl1Speed = Mathf.Min(speed, max1) / max1;
		pi.getCrosshairCircle().fillAmount = lvl1Speed;

		float lvl2Speed = Mathf.Max(Mathf.Min(speed - max1, (max2 - max1)), 0f) / (max2 - max1);
		pi.getCrosshairCircle2().fillAmount = lvl2Speed;

		float lvl3Speed = Mathf.Max(Mathf.Min(speed - max2, (max3 - max2)), 0f) / (max3 - max2);
		pi.getCrosshairCircle3().fillAmount = lvl3Speed;

		Color c = new Color(1f, 1f, 1f);
		
		if(speed <= max1) { c.r = 1f - lvl1Speed * 0.8f; }
		if(speed > max1)  { c.r = lvl2Speed * 0.8f; }

		if(speed > max1)  { c.g = 1f - lvl2Speed * 0.5f * 0.8f; }
		if(speed > max2)  { c.g = 0.5f - lvl3Speed * 0.5f * 0.8f; }

		c.b = 1f - lvl1Speed;
		
		pi.setCrosshairColor(c);
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