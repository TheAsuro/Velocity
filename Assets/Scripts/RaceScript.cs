using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Interacts with checkpoint triggers, must be added to every player
public class RaceScript : MonoBehaviour
{
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

		//Unfreeze if freeze time is up
		if(frozen && Time.time >= unfreezeTime)
		{
			unfreeze();
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
		nameText.text = GameInfo.info.getCurrentSave().getPlayerName();

		//Display crosshair
		drawCrosshair();

		//Skip countdown
		if(Input.GetButtonDown("Jump"))
		{
			startTime = Time.time;
			unfreeze();
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

			//Save game
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
		PlayerInfo pi = GameInfo.info.getPlayerInfo();
		float speed = float.Parse(pi.getCurrentSpeed());

		float lvl1Speed = Mathf.Min(speed, 7f) / 7f;
		pi.getCrosshairCircle().fillAmount = lvl1Speed;

		float lvl2Speed = Mathf.Max(Mathf.Min(speed - 7f, 21f), 0f) / 14f;
		pi.getCrosshairCircle2().fillAmount = lvl2Speed;

		Color c = new Color(1f, 1f, 1f);
		if(speed <= 7f)
		{
			c = new Color(1f - lvl1Speed / 2f, 1f - lvl1Speed / 2f, 1f);
		}
		if(speed >= 7f)
		{
			c = new Color(0.5f + lvl2Speed / 2f, 0.5f, 1f - lvl2Speed / 2f);
		}
		pi.setCrosshairColor(c);
	}

	//Starts a new race (resets the current one if there is one)
	public void startRace(float newFreezeDuration = 0f)
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
}