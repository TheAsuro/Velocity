using System;
using System.Collections.Generic;
using UnityEngine;

namespace Demos
{
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
        private bool looping = false;

        private Action myCallback;

        private void Update()
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
                    if((float)tick.GetTime() <= playTime && (float)tick.GetTime() > lastFrameTime)
                    {
                        lastFrameTime = (float)tick.GetTime();
                        lastPos = tick.GetPosition();
                        lastRot = tick.GetRotation();
                    }
                    //Find the one after that
                    else
                    {
                        if((float)tick.GetTime() > (float)lastFrameTime && nextFrameTime == -1f)
                        {
                            nextFrameTime = (float)tick.GetTime();
                            nextPos = tick.GetPosition();
                            nextRot = tick.GetRotation();
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

                if (nextFrameTime == -1f)
                {
                    if (looping)
                        ResetDemo();
                    else
                        StopDemoPlayback();
                }

                if (Input.GetButtonDown("Menu"))
                {
                    StopDemoPlayback();
                }
            }
        }

        public void PlayDemo(Demo demo, Action callback, bool doLoop = false)
        {
            looping = doLoop;

            //Reset if currently playing
            StopDemoPlayback(true);

            //Load demo ticks
            tickList = demo.GetTickList();

            //Get ghost spawn
            ghost = (GameObject)GameObject.Instantiate(ghostPrefab, tickList[0].GetPosition(), tickList[0].GetRotation());
            ghostCam = (GameObject)GameObject.Instantiate(ghostCamPrefab, tickList[0].GetPosition(), tickList[0].GetRotation());

            //Set up camera
            Camera cam = ghostCam.GetComponent<Camera>();
            cam.backgroundColor = WorldInfo.info.worldBackgroundColor;
            cam.fieldOfView = Settings.Game.Fov;

            //Set start time to current time
            startPlayTime = Time.time;

            //Stop playback on world reset
            WorldInfo.Reset resetPlay = new WorldInfo.Reset(StopDemoPlayback);
            WorldInfo.info.AddResetMethod(resetPlay, "GhostReset");

            //Save callback
            myCallback = callback;

            //Start playing
            playing = true;
        }

        public void StopDemoPlayback(bool interrupt)
        {
            playing = false;
            GameObject.Destroy(ghost);
            GameObject.Destroy(ghostCam);

            if (myCallback != null && !interrupt)
                myCallback();
        }

        public void StopDemoPlayback()
        {
            StopDemoPlayback(false);
        }

        public void ResetDemo()
        {
            if (!playing || ghost == null)
                throw new InvalidOperationException("Can't reset a not playing demo!");

            ghost.transform.position = tickList[0].GetPosition();
            ghost.transform.rotation = tickList[0].GetRotation();
            ghostCam.transform.position = tickList[0].GetPosition();
            ghostCam.transform.rotation = tickList[0].GetRotation();
            startPlayTime = Time.time;

            playing = true;
        }
    }
}
