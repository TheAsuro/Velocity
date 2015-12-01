using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DemoPlay : MonoBehaviour
{
    public static bool thirdPersonDemoView = false;

	public delegate void FinishedPlaying();
	public GameObject ghostPrefab;
	public GameObject ghostCamPrefab;
    public Vector3 thirdPersonOffset;

	private Vector3 camDistance;
	private bool playing = false;
	private float startPlayTime;
	private GameObject ghost;
	private GameObject ghostCam;
	private List<DemoTick> tickList;

    private Action MyCallback;

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
				if((float)tick.getTime() <= playTime && (float)tick.getTime() > lastFrameTime)
				{
					lastFrameTime = (float)tick.getTime();
					lastPos = tick.getPosition();
					lastRot = tick.getRotation();
				}
				//Find the one after that
				else
				{
					if((float)tick.getTime() > (float)lastFrameTime && nextFrameTime == -1f)
					{
						nextFrameTime = (float)tick.getTime();
						nextPos = tick.getPosition();
						nextRot = tick.getRotation();
					}
				}
			}

            //If demo is running
			if(lastFrameTime > 0f && nextFrameTime > 0f)
			{
				float frameStep = nextFrameTime - lastFrameTime;
				float timeToNextFrame = nextFrameTime - playTime;
				float t = timeToNextFrame / frameStep;

				Quaternion editedLastRot = Quaternion.Euler(lastRot.eulerAngles.x, lastRot.eulerAngles.y, 0f);
				Quaternion editedNextRot = Quaternion.Euler(lastRot.eulerAngles.x, nextRot.eulerAngles.y, 0f);

				ghost.transform.position = Vector3.Lerp(lastPos, nextPos, t);
				ghost.transform.rotation = Quaternion.Lerp(editedLastRot, editedNextRot, t);

                //Update first/third person view
                if (!thirdPersonDemoView)
                    camDistance = new Vector3(0f, 0.5f, 0f); //First person view
                else
                    camDistance = thirdPersonOffset; //Third person view

				//make obj at ghost position and child at cam distance
				ghostCam.transform.position = ghost.transform.position + (ghost.transform.rotation * camDistance);

                //Look at player if in third person
                if (thirdPersonDemoView)
                    ghostCam.transform.LookAt(ghost.transform.position);
                else
                    ghostCam.transform.rotation = ghost.transform.rotation;
			}

            if (Input.GetButtonDown("Menu") || nextFrameTime == -1f)
            {
                StopDemoPlayback();
            }
		}
	}

	public void playDemo(Demo demo, Action Callback)
	{
        //Reset if currently playing
		StopDemoPlayback(true);

		//Load demo ticks
		tickList = demo.getTickList();

		//Get ghost spawn
		ghost = (GameObject)GameObject.Instantiate(ghostPrefab, tickList[0].getPosition(), tickList[0].getRotation());
        ghostCam = (GameObject)GameObject.Instantiate(ghostCamPrefab, tickList[0].getPosition(), tickList[0].getRotation());

		//Set up camera
        Camera cam = ghostCam.GetComponent<Camera>();
		cam.backgroundColor = WorldInfo.info.worldBackgroundColor;
        cam.fieldOfView = Settings.Game.Fov;

		//Set start time to current time
		startPlayTime = Time.time;

		//Stop playback on world reset
		WorldInfo.Reset resetPlay = new WorldInfo.Reset(StopDemoPlayback);
		WorldInfo.info.addResetMethod(resetPlay, "GhostReset");

        //Save callback
        MyCallback = Callback;

		//Start playing
		playing = true;
	}

    public void StopDemoPlayback(bool interrupt)
    {
        playing = false;
        GameObject.Destroy(ghost);
        GameObject.Destroy(ghostCam);

        if (MyCallback != null && !interrupt)
            MyCallback();
    }

	public void StopDemoPlayback()
	{
        StopDemoPlayback(false);
	}
}
