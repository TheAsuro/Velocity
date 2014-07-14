using UnityEngine;
using System.Collections;

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
	private string countdownText = ":^)";
	
	void Awake()
	{
		GameInfo.info.setPlayerObject(gameObject);
	}

	void Start()
	{
		WorldInfo.info.addResetMethod(reset, "race reset");
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
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag.Equals("Checkpoint"))
		{
			Checkpoint cp = other.GetComponent<Checkpoint>();
			int nr = cp.checkpointNumber;
			bool end = cp.isEnd;
			freezeDuration = cp.freezeTime;
			drawCountdown = cp.countdown;
			
			if(nr == 0) //Start
			{
				startTime = Time.time;
				checkpoint = 0;
				finished = false;
				if(freezeDuration > 0f)
				{
					freeze(freezeDuration);
					startTime += freezeDuration;
				}
				GameInfo.info.StartDemo();
			}
			else if(end && nr == checkpoint + 1 && !finished) //End
			{
				GameInfo.info.StopDemo();
				time = Time.time - startTime;
				finished = true;
				if(freezeDuration > 0f)
				{
					freeze(freezeDuration);
				}
			}
			else if(nr == checkpoint + 1) //Random checkpoint
			{
				checkpoint++;
				if(freezeDuration > 0f)
				{
					freeze(freezeDuration);
				}
			}
		}
	}
	
	private void freeze(float duration)
	{
		frozen = true;
		rigidbody.isKinematic = true;
		unfreezeTime = Time.time + duration;
	}
	
	private void unfreeze()
	{
		frozen = false;
		rigidbody.isKinematic = false;
		//TODO find less hacky way of updating the rigidbody
		rigidbody.useGravity = false;
		rigidbody.useGravity = true;
	}
	
	private string getFrozenString()
	{
		return unfreezeTime.ToString();
	}

	public void reset()
	{
		startTime = -1f;
		time = -1f;
		checkpoint = -1;
		finished = false;
		frozen = false;
	}
	
	void OnGUI()
	{
		string tStr ="Time: " + time.ToString().Substring(0,time.ToString().IndexOf('.') + 2);
		
		if(drawTime && time > 0f && checkpoint != -1 && !finished)
		{
			GameInfo.info.drawTextBox(0f,-0.8f,tStr);
		}
		if(finished)
		{
			GameInfo.info.drawTextBox(0f,-0.8f,time.ToString());
		}
		if(drawCountdown && !GameInfo.info.getGamePaused())
		{
			float remainingFreezeTime = unfreezeTime - Time.time;
			if(remainingFreezeTime > freezeDuration * (2f/3f))
			{
				countdownText = "3";
			}
			else if(remainingFreezeTime > freezeDuration * (1f/3f))
			{
				countdownText = "2";
			}
			else if(remainingFreezeTime > 0f)
			{
				countdownText = "1";
			}
			else if(remainingFreezeTime > freezeDuration * (-1f/3f))
			{
				countdownText = "GO!";
			}
			else
			{
				drawCountdown = false;
			}
			GameInfo.info.drawTextBox(0f, -0.5f, countdownText);
		}
	}
}
