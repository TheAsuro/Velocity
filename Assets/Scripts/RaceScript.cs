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
	private float unfreezeTime = float.PositiveInfinity;
	
	void Awake()
	{
		GameInfo.info.setPlayerObject(gameObject);
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
			float freezeTime = cp.freezeTime;
			
			if(nr == 0)
			{
				startTime = Time.time;
				checkpoint = 0;
				finished = false;
				if(freezeTime > 0f)
				{
					freeze(freezeTime);
					startTime += freezeTime;
				}
			}
			else if(end && nr == checkpoint + 1)
			{
				time = Time.time - startTime;
				finished = true;
				if(freezeTime > 0f)
				{
					freeze(freezeTime);
				}
			}
			else if(nr == checkpoint + 1)
			{
				checkpoint++;
				if(freezeTime > 0f)
				{
					freeze(freezeTime);
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
	
	void OnGUI()
	{
		string tStr ="Time: " + time.ToString().Substring(0,time.ToString().IndexOf('.') + 2);
		
		if(drawTime && checkpoint != -1 && !finished)
		{
			string output = tStr + "\nCP: " + checkpoint.ToString();
			GameInfo.info.drawTextBox(0f,-0.8f,output);
		}
		if(finished)
		{
			GameInfo.info.drawTextBox(0f,-0.8f,time.ToString());
		}
	}
}
