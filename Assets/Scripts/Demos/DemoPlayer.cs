using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demos
{
    public class DemoPlayer : MonoBehaviour
    {
        public event EventHandler OnFinishedPlaying;

        [SerializeField] private GameObject ghostCamPrefab;
        [SerializeField] private Vector3 followCamOffset = new Vector3(0f, 3f, -6f);
        [SerializeField] private float followCamSpeed = 2f;

        private Vector3 camDistance;
        private bool playing = false;
        private float startPlayTime;
        private GameObject ghostCam;
        private List<DemoTick> tickList;
        private bool looping = false;
        private bool followCam = false;

        private void Update()
        {
            //If we are playing and there is a valid ghost
            if (playing)
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
                    if (tick.Time <= playTime && tick.Time > lastFrameTime)
                    {
                        lastFrameTime = tick.Time;
                        lastPos = tick.Position;
                        lastRot = tick.Rotation;
                    }
                    //Find the one after that
                    else
                    {
                        if (tick.Time > lastFrameTime && nextFrameTime == -1f)
                        {
                            nextFrameTime = tick.Time;
                            nextPos = tick.Position;
                            nextRot = tick.Rotation;
                        }
                    }
                }

                //If demo is running
                if (lastFrameTime > 0f && nextFrameTime > 0f)
                {
                    float timeToNextFrame = nextFrameTime - playTime;
                    float frameStep = nextFrameTime - lastFrameTime;
                    float t = timeToNextFrame / frameStep;

                    Quaternion editedLastRot = Quaternion.Euler(lastRot.eulerAngles.x, lastRot.eulerAngles.y, 0f);
                    Quaternion editedNextRot = Quaternion.Euler(lastRot.eulerAngles.x, nextRot.eulerAngles.y, 0f);

                    Vector3 playerPos = Vector3.Lerp(lastPos, nextPos, t);

                    transform.position = playerPos;
                    transform.rotation = Quaternion.Lerp(editedLastRot, editedNextRot, t);

                    //Update first/third person view
                    camDistance = new Vector3(0f, 0.5f, 0f);
                }

                // Demo ended
                if (nextFrameTime < 0f)
                {
                    playing = false;
                    StartCoroutine(EndDemoDelay());
                }

                if (Input.GetButtonDown("Menu"))
                {
                    StopDemoPlayback();
                }
            }

            //make obj at ghost position and child at cam distance
            if (followCam)
            {
                ghostCam.transform.position = Vector3.Lerp(ghostCam.transform.position, transform.TransformPoint(followCamOffset), Time.deltaTime * followCamSpeed);
                ghostCam.transform.rotation = Quaternion.Lerp(ghostCam.transform.rotation, Quaternion.LookRotation(transform.position - ghostCam.transform.position, Vector3.up), Time.deltaTime);
            }
            else
            {
                ghostCam.transform.position = transform.position + (transform.rotation * camDistance);
                ghostCam.transform.rotation = transform.rotation;
            }
        }

        private IEnumerator EndDemoDelay()
        {
            yield return new WaitForSecondsRealtime(3f);
            if (looping)
                ResetDemo();
            else
                StopDemoPlayback();
        }

        public void PlayDemo(Demo demo, bool doLoop = false, bool followCam = false)
        {
            looping = doLoop;
            this.followCam = followCam;

            //Load demo ticks
            tickList = demo.Ticks;

            //Get ghost spawn
            ghostCam = Instantiate(ghostCamPrefab);

            //Set up camera
            Camera cam = ghostCam.GetComponent<Camera>();
            cam.backgroundColor = WorldInfo.info.WorldData.backgroundColor;
            cam.fieldOfView = Settings.GameSettings.SingletonInstance.Fov;

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

            if (OnFinishedPlaying != null && !interrupt)
                OnFinishedPlaying(this, new EventArgs());

            Destroy(ghostCam);
            Destroy(gameObject);
        }

        public void ResetDemo()
        {
            transform.position = tickList[0].Position;
            transform.rotation = tickList[0].Rotation;
            ghostCam.transform.position = followCam ? tickList[0].Position + followCamOffset : tickList[0].Position;
            ghostCam.transform.rotation = tickList[0].Rotation;
            startPlayTime = Time.time;

            playing = true;
        }
    }
}