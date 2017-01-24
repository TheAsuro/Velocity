using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Demos
{
    public class DemoPlayer : MonoBehaviour
    {
        public static DemoPlayer SingletonInstance;

        public event EventHandler OnFinishedPlaying;

        public GameObject ghostPrefab;
        public GameObject ghostCamPrefab;

        private Vector3 camDistance;
        private bool playing = false;
        private float startPlayTime;
        private GameObject ghost;
        private GameObject ghostCam;
        private List<DemoTick> tickList;
        private bool looping = false;
        private bool staticCam = false;

        private void Awake()
        {
            if (SingletonInstance == null)
                SingletonInstance = this;
            else
                throw new InvalidOperationException("Two DemoPlayer instances!");
        }

        private void Update()
        {
            //If we are playing and there is a valid ghost
            if (playing && ghost != null)
            {
                float playTime = Time.time - startPlayTime; //Time since we began playing
                float lastFrameTime = -1f; //Last recorded frame
                float nextFrameTime = -1f; //Frame that comes after that
                Vector3 lastPos = Vector3.zero;
                Vector3 nextPos = Vector3.zero;
                Quaternion lastRot = new Quaternion();
                Quaternion nextRot = new Quaternion();

                //Go through all frames
                foreach (DemoTick tick in tickList)
                {
                    //Find the highest one that is smaller than playTime
                    if ((float) tick.GetTime() <= playTime && (float) tick.GetTime() > lastFrameTime)
                    {
                        lastFrameTime = (float) tick.GetTime();
                        lastPos = tick.GetPosition();
                        lastRot = tick.GetRotation();
                    }
                    //Find the one after that
                    else
                    {
                        if ((float) tick.GetTime() > (float) lastFrameTime && nextFrameTime == -1f)
                        {
                            nextFrameTime = (float) tick.GetTime();
                            nextPos = tick.GetPosition();
                            nextRot = tick.GetRotation();
                        }
                    }
                }

                //If demo is running
                if (lastFrameTime > 0f && nextFrameTime > 0f)
                {
                    float frameStep = nextFrameTime - lastFrameTime;
                    float timeToNextFrame = nextFrameTime - playTime;
                    float t = timeToNextFrame / frameStep;

                    Quaternion editedLastRot = Quaternion.Euler(lastRot.eulerAngles.x, lastRot.eulerAngles.y, 0f);
                    Quaternion editedNextRot = Quaternion.Euler(lastRot.eulerAngles.x, nextRot.eulerAngles.y, 0f);

                    Vector3 playerPos = Vector3.Lerp(lastPos, nextPos, t);

                    ghost.transform.position = playerPos;
                    ghost.transform.rotation = Quaternion.Lerp(editedLastRot, editedNextRot, t);

                    //Update first/third person view
                    camDistance = new Vector3(0f, 0.5f, 0f);

                    //make obj at ghost position and child at cam distance
                    if (staticCam)
                        ghostCam.transform.LookAt(nextPos);
                    else
                    {
                        ghostCam.transform.position = ghost.transform.position + (ghost.transform.rotation * camDistance);
                        ghostCam.transform.rotation = ghost.transform.rotation;
                    }
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

        public void PlayDemo(Demo demo, bool doLoop = false, bool staticCam = false)
        {
            looping = doLoop;
            this.staticCam = staticCam;

            //Reset if currently playing
            StopDemoPlayback(true);

            //Load demo ticks
            tickList = demo.GetTickList();

            //Get ghost spawn
            ghost = Instantiate(ghostPrefab);
            ghostCam = Instantiate(ghostCamPrefab);

            //Set up camera
            Camera cam = ghostCam.GetComponent<Camera>();
            cam.backgroundColor = WorldInfo.info.WorldData.backgroundColor;
            cam.fieldOfView = Settings.Game.Fov;

            //Set start time to current time
            startPlayTime = Time.time;

            //Stop playback on world reset
            WorldInfo.info.RaceScript.OnReset += (s, e) => StopDemoPlayback(true);

            playing = true;
            ResetDemo();
        }

        public void StopDemoPlayback(bool interrupt = false)
        {
            playing = false;
            Destroy(ghost);
            Destroy(ghostCam);

            if (OnFinishedPlaying != null && !interrupt)
                OnFinishedPlaying(this, new EventArgs());
        }

        public void ResetDemo()
        {
            if (!playing || ghost == null)
                throw new InvalidOperationException("Can't reset a not playing demo!");

            ghost.transform.position = tickList[0].GetPosition();
            //ghost.transform.rotation = tickList[0].GetRotation();
            ghostCam.transform.position = staticCam ? CalculateCamPosition(tickList) : tickList[0].GetPosition();
            ghostCam.transform.rotation = tickList[0].GetRotation();
            startPlayTime = Time.time;

            playing = true;
        }

        private static Vector3 CalculateCamPosition(IEnumerable<DemoTick> playerPositions)
        {
            Vector3 averagePos = playerPositions.Select(tick => tick.GetPosition()).Aggregate((a, b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z)) / playerPositions.Count();
            return averagePos + new Vector3(0f, 10f, 0f);
        }
    }
}