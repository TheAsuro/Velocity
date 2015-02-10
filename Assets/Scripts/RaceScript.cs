using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Interacts with checkpoint triggers, must be added to every player
public class RaceScript : MonoBehaviour
{
	private bool editorMode = false;

	private float startTime = -1f;
	private float time = -1f;

	private int checkpoint = -1;
	private bool finished = false;

	private bool frozen = false;
	private float freezeDuration = 0f;
	private float unfreezeTime = float.PositiveInfinity;

	private Text timeText;
	private Text speedText;
	private Text nameText;
	private Text countdownText;
	private Text wrText;
	
	//This script actually sets the player as an actual player
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
		//Update time
		if(!finished)
		{
			time = Time.time - startTime;
		}

		//Freeze time is up, start the race!
		if(frozen && Time.time >= unfreezeTime)
		{
			startRace();
		}

		//Checkpoint system
		if(time > 0f && checkpoint != -1 && !finished)
		{
			timeText.gameObject.transform.parent.gameObject.SetActive(true);
			timeText.text = "Time: " + time.ToString().Substring(0,time.ToString().IndexOf('.') + 2);
		}
		else if(finished)
		{
			timeText.gameObject.transform.parent.gameObject.SetActive(true);
			timeText.text = time.ToString();
		}
		else
		{
			timeText.gameObject.transform.parent.gameObject.SetActive(false);
		}

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
		if(Input.GetButtonDown("Skip") && startTime > Time.time)
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
			freezeDuration = cp.freezeTime;

			//Save game (pretty sure i can remove this. TODO check if unnecessary)
			GameInfo.info.save();
			
			if(end && nr == checkpoint + 1 && !finished) //End
			{
				time = Time.time - startTime;
				finished = true;
				GameInfo.info.runFinished(time);
				if(freezeDuration > 0f)
				{
					freeze(freezeDuration);
				}
			}
			else if(nr == checkpoint + 1) //next checkpoint
			{
				checkpoint++;
				if(freezeDuration > 0f)
				{
					freeze(freezeDuration);
				}
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
		time = -1f;
		startTime = Time.time;

		checkpoint = 0;
		finished = false;
		freezeDuration = newFreezeDuration;
		frozen = false;
		if(freezeDuration > 0f)
		{
			freeze(freezeDuration);
			startTime += freezeDuration;
		}

		GameInfo.info.startDemo();
	}

	private void startRace()
	{
		startTime = Time.time;
		WorldInfo.info.DoStart();
		unfreeze();
	}
	
	private void freeze(float duration)
	{
		frozen = true;
		GameInfo.info.getPlayerInfo().freeze();
		unfreezeTime = Time.time + duration;
	}
	
	private void unfreeze()
	{
		frozen = false;
		GameInfo.info.getPlayerInfo().unfreeze();
		unfreezeTime = Time.time;
	}
	
	private string getFrozenString()
	{
		return unfreezeTime.ToString();
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