using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DemoRecord : MonoBehaviour
{
	public GameObject ghostPrefab;
	public GameObject ghostCamPrefab;
	public Vector3 camDistance;

	//Record
	private List<DemoTick> tickList;
	private bool recording = false;
	private Demo completeDemo;
	private string playerName;
	private string levelName;

	//Replay
	private bool playing = false;
	private float startPlayTime;
	private GameObject ghost;
	private GameObject ghostCamObj;
	private GameObject ghostCamChild;
	private Demo replayDemo;

	void Update()
	{
		//If we are playing and there is a valid ghost
		if(playing && ghost != null)
		{
			float playTime = Time.time - startPlayTime; //Time since we began playing
			float lastFrameTime = -1f; //Last recorded frame
			float nextFrameTime = -1f; //Frame that comes after that
			Vector3 lastPos = Vector3.zero;
			Vector3 nextPos = Vector3.zero;
			Quaternion lastRot = new Quaternion();
			Quaternion nextRot = new Quaternion();

			//Go through all frames
			foreach(DemoTick tick in tickList)
			{
				//Find the highest one that is smaller than playTime
				if(tick.getTime() <= playTime && tick.getTime() > lastFrameTime)
				{
					lastFrameTime = tick.getTime();
					lastPos = tick.getPosition();
					lastRot = tick.getRotation();
				}
				//Find the one after that
				else
				{
					if(tick.getTime() > lastFrameTime && nextFrameTime == -1f)
					{
						nextFrameTime = tick.getTime();
						nextPos = tick.getPosition();
						nextRot = tick.getRotation();
					}
				}
			}

			if(lastFrameTime > 0f && nextFrameTime > 0f)
			{
				float frameStep = nextFrameTime - lastFrameTime;
				float timeToNextFrame = nextFrameTime - playTime;
				float t = timeToNextFrame / frameStep;

				Quaternion editedLastRot = Quaternion.Euler(0f, lastRot.eulerAngles.y, 0f);
				Quaternion editedNextRot = Quaternion.Euler(0f, nextRot.eulerAngles.y, 0f);

				ghost.transform.position = Vector3.Lerp(lastPos, nextPos, t);
				ghost.transform.rotation = Quaternion.Lerp(editedLastRot, editedNextRot, t);

				//TODO make obj at ghost position and child at cam distance
				ghostCamObj.transform.position = ghost.transform.position;
				ghostCamChild.transform.localPosition = camDistance;
				ghostCamObj.transform.rotation = ghost.transform.rotation;

				ghostCamChild.transform.LookAt(ghost.transform.position);
			}
		}
	}

	void FixedUpdate()
	{
		if(recording)
		{
			Quaternion rot = Camera.main.transform.rotation;
			tickList.Add(new DemoTick(Time.time, transform.position, rot));
		}
	}

	public void StartDemo(string pPlayerName)
	{
		tickList = new List<DemoTick>();
		playerName = pPlayerName;
		levelName = Application.loadedLevelName;
		recording = true;
	}

	public void StopDemo()
	{
		recording = false;
		completeDemo = new Demo(tickList, playerName, levelName);
	}

	public Demo getDemo()
	{
		return completeDemo;
	}

	public void PlayDemo(Demo demo)
	{
		Respawn spawn = WorldInfo.info.getFirstSpawn();
		ghost = (GameObject)GameObject.Instantiate(ghostPrefab, spawn.getSpawnPos(), spawn.getSpawnRot());
		ghostCamObj = (GameObject)GameObject.Instantiate(ghostCamPrefab, spawn.getSpawnPos(), spawn.getSpawnRot());
		ghostCamChild = ghostCamObj.transform.FindChild("CamObj").gameObject;
		replayDemo = demo;
		startPlayTime = Time.time;
		playing = true;
	}
}