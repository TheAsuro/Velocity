using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
                float playTime = Time.time - startPlayTime;
                float lastTickTime = -1f;
                float nextTickTime = -1f;
                Vector3 lastTickPos = Vector3.zero;
                Vector3 nextTickPos = Vector3.zero;
                Quaternion lastTickRot = new Quaternion();
                Quaternion nextTickRot = new Quaternion();
                float crouchPercentage = 0f;
                float framePercentage = -1f;

                //Go through all frames
                bool found = false;
                for (int i = 0; i + 1 < tickList.Count; i++)
                {
                    float nextFrameSeconds = (float)new TimeSpan(tickList[i + 1].Time).TotalSeconds;
                    if (nextFrameSeconds >= playTime)
                    {
                        lastTickTime = (float)new TimeSpan(tickList[i].Time).TotalSeconds;
                        lastTickPos = tickList[i].Position;
                        lastTickRot = tickList[i].Rotation;

                        nextTickTime = nextFrameSeconds;
                        nextTickPos = tickList[i].Position;
                        nextTickRot = tickList[i].Rotation;

                        framePercentage = (nextTickTime - lastTickTime) / (playTime - lastTickTime);

                        float crouchedLastFrame = tickList[i].Crouched ? 1f : 0f;
                        float crouchedNextFrame = tickList[i + 1].Crouched ? 1f : 0f;
                        crouchPercentage = Mathf.Lerp(crouchedLastFrame, crouchedNextFrame, framePercentage);

                        found = true;
                        break;
                    }
                }

                //If demo is running
                if (found)
                {
                    Assert.IsTrue(framePercentage >= 0f);

                    Quaternion editedLastRot = Quaternion.Euler(lastTickRot.eulerAngles.x, lastTickRot.eulerAngles.y, 0f);
                    Quaternion editedNextRot = Quaternion.Euler(lastTickRot.eulerAngles.x, nextTickRot.eulerAngles.y, 0f);


                    transform.position = Vector3.Lerp(lastTickPos, nextTickPos, framePercentage) + new Vector3(0f, crouchPercentage * -0.5f, 0f);
                    transform.rotation = Quaternion.Lerp(editedLastRot, editedNextRot, framePercentage);
                    transform.localScale = new Vector3(1f, 1f - 0.5f * crouchPercentage, 1f);

                    //Update first/third person view
                    camDistance = new Vector3(0f, 0.5f, 0f);
                }
                else
                {
                    playing = false;
                    StartCoroutine(EndDemoDelay());
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
            cam.fieldOfView = Settings.GameSettings.SingletonInstance.Fov;

            //Set start time to current time
            startPlayTime = Time.time;

            //Stop playback on world reset
            WorldInfo.info.RaceScript.OnReset += (s, e) => StopDemoPlayback(true);

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