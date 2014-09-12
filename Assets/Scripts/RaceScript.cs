using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//Interacts with checkpoint triggers, must be added to every player
public class RaceScript : MonoBehaviour
{
	//Draw the timer
	public bool drawTime = true;

	private float startTime = -1f;
	private float time = -1f;

	private int checkpoint = -1;
	private bool finished = false;

	private bool frozen = false;
	private float freezeDuration = 0f;
	private float unfreezeTime = float.PositiveInfinity;

	private bool drawCountdown = false;
	private string countdownText;

	private Text timeTextObj;
	private Text countdownTextObj;
	
	//This script actually sets the player as an actual player
	void Awake()
	{
		GameInfo.info.setPlayerObject(gameObject);
		
		Transform canvas = gameObject.transform.parent.Find("Canvas");
		timeTextObj = canvas.Find("Time").Find("Text").GetComponent<Text>();
		countdownTextObj = canvas.Find("Countdown").Find("Text").GetComponent<Text>();
	}

	void Update()
	{
		if(!finished)
		{
			time = Time.time - startTime;
		}
		if(frozen && Time.time >= unfreezeTime)
		{
			unfreeze();
		}

		if(time > 0f && checkpoint != -1 && !finished)
		{
			timeTextObj.gameObject.transform.parent.gameObject.SetActive(true);
			timeTextObj.text = "Time: " + time.ToString().Substring(0,time.ToString().IndexOf('.') + 2);
		}
		else if(finished)
		{
			timeTextObj.gameObject.transform.parent.gameObject.SetActive(true);
			timeTextObj.text = time.ToString();
		}
		else
		{
			timeTextObj.gameObject.transform.parent.gameObject.SetActive(false);
		}

		float remainingFreezeTime = unfreezeTime - Time.time;
		if(remainingFreezeTime > freezeDuration * (2f/3f))
		{
			countdownTextObj.gameObject.transform.parent.gameObject.SetActive(true);
			countdownTextObj.text = "3";
		}
		else if(remainingFreezeTime > freezeDuration * (1f/3f))
		{
			countdownTextObj.gameObject.transform.parent.gameObject.SetActive(true);
			countdownTextObj.text = "2";
		}
		else if(remainingFreezeTime > 0f)
		{
			countdownTextObj.gameObject.transform.parent.gameObject.SetActive(true);
			countdownTextObj.text = "1";
		}
		else if(remainingFreezeTime > freezeDuration * (-1f/3f))
		{
			countdownTextObj.gameObject.transform.parent.gameObject.SetActive(true);
			countdownTextObj.text = "GO!";
		}
		else
		{
			countdownTextObj.gameObject.transform.parent.gameObject.SetActive(false);
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
			drawCountdown = cp.countdown;

			//Save game
			GameInfo.info.save();
			
			if(end && nr == checkpoint + 1 && !finished) //End
			{
				GameInfo.info.stopDemo();
				time = Time.time - startTime;
				finished = true;
				GameInfo.info.getCurrentSave().saveIfPersonalBest(time, Application.loadedLevelName);
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
			if(freezeDuration == 3f)
			{
				drawCountdown = true;
			}
		}
		GameInfo.info.startDemo();
	}
	
	private void freeze(float duration)
	{
		frozen = true;
		Movement.movement.freeze();
		unfreezeTime = Time.time + duration;
	}
	
	private void unfreeze()
	{
		frozen = false;
		Movement.movement.unfreeze();
	}
	
	private string getFrozenString()
	{
		return unfreezeTime.ToString();
	}
}