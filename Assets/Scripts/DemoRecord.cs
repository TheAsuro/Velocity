using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DemoRecord : MonoBehaviour
{
	public GameObject ghostPrefab;

	//Record
	private Dictionary<float,Vector3> posList;
	private bool recording = false;
	private Demo completeDemo;
	private string playerName;
	private string levelName;

	//Replay
	private bool playing = false;
	private float startPlayTime;
	private GameObject ghost;
	private Camera ghostCam;
	private Demo replayDemo;

	void Update()
	{
		//If we are playing and there is a valid ghost
		if(playing && ghost != null)
		{
			float playTime = Time.time - startPlayTime; //Time since we began playing
			float lastFrameTime = -1f; //Last recorded frame
			float nextFrameTime = -1f; //Frame that comes after that

			//Go through all frames
			foreach(KeyValuePair<float,Vector3> pair in replayDemo.getPosList())
			{
				//Find the highest one that is smaller than playTime
				if(pair.Key <= playTime && pair.Key > lastFrameTime)
				{
					lastFrameTime = pair.Key;
				}
				//Find the one after that
				else
				{
					if(pair.Key > lastFrameTime && nextFrameTime == -1f)
					{
						nextFrameTime = pair.Key;
					}
				}
			}

			Vector3 oldPos; //Position of last frame
			Vector3 newPos; //Position of the frame after that
			if(replayDemo.getPosList().TryGetValue(lastFrameTime, out oldPos) && replayDemo.getPosList().TryGetValue(nextFrameTime, out newPos))
			{
				float frameStep = nextFrameTime - lastFrameTime;
				float timeToNextFrame = nextFrameTime - playTime;
				float t = timeToNextFrame / frameStep;

				ghost.transform.position = Vector3.Lerp(oldPos, newPos, t);
				ghostCam.transform.LookAt(newPos);
			}
		}
	}

	void FixedUpdate()
	{
		if(recording)
		{
			posList.Add(Time.time, transform.position);
		}
	}

	public void StartDemo(string pPlayerName)
	{
		posList = new Dictionary<float,Vector3>();
		playerName = pPlayerName;
		levelName = Application.loadedLevelName;
		recording = true;
	}

	public void StopDemo()
	{
		recording = false;
		completeDemo = new Demo(posList, playerName, levelName);
	}

	public Demo getDemo()
	{
		return completeDemo;
	}

	public void PlayDemo(Demo demo)
	{
		Respawn spawn = WorldInfo.info.getFirstSpawn();
		ghost = (GameObject)GameObject.Instantiate(ghostPrefab, spawn.getSpawnPos(), spawn.getSpawnRot());
		ghostCam = ghost.transform.FindChild("Cam").GetComponent<Camera>();
		replayDemo = demo;
		startPlayTime = Time.time;
		playing = true;
	}
}