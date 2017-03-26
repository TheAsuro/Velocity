using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Demos
{
    public class DemoPlayer : MonoBehaviour
    {
        public event EventHandler OnFinishedPlaying;

        [SerializeField] private GameObject ghostCamPrefab;

        private Vector3 firstPersonCamOffset = new Vector3(0f, 0.5f, 0f);
        private bool playing = false;
        private float startPlayTime;
        private GameObject ghostCam;
        private List<DemoTick> tickList;
        private bool looping = false;
        private bool topView = false;

        private void Update()
        {
            Quaternion viewRotation = Quaternion.identity;

            //If we are playing and there is a valid ghost
            if (playing)
            {
                float playTime = Time.time - startPlayTime;
                Vector3 lastTickPos = Vector3.zero;
                Vector3 nextTickPos = Vector3.zero;
                Quaternion lastTickRot = new Quaternion();
                Quaternion nextTickRot = new Quaternion();
                float crouchPercentage = 0f;
                float framePercentage = -1f;
                int nextTickIndex = -1;

                //Go through all frames
                bool found = false;
                for (int i = 0; i + 1 < tickList.Count; i++)
                {
                    float nextFrameSeconds = (float)new TimeSpan(tickList[i + 1].Time).TotalSeconds;
                    if (nextFrameSeconds >= playTime)
                    {
                        var lastTickTime = (float)new TimeSpan(tickList[i].Time).TotalSeconds;
                        lastTickPos = tickList[i].Position;
                        lastTickRot = tickList[i].Rotation;

                        float nextTickTime = nextFrameSeconds;
                        nextTickPos = tickList[i].Position;
                        nextTickRot = tickList[i].Rotation;

                        framePercentage = (nextTickTime - lastTickTime) / (playTime - lastTickTime);
                        nextTickIndex = i;

                        float crouchedLastFrame = tickList[i].Crouched ? 1f : 0f;
                        float crouchedNextFrame = tickList[i + 1].Crouched ? 1f : 0f;
                        crouchPercentage = Mathf.Lerp(crouchedLastFrame, crouchedNextFrame, framePercentage);

                        found = true;
                        break;
                    }
                }

                //If demo is running set player position
                if (found)
                {
                    Assert.AreNotApproximatelyEqual(framePercentage, -1f, "Frame percentage was not set!");

                    Quaternion editedLastRot = Quaternion.Euler(lastTickRot.eulerAngles.x, lastTickRot.eulerAngles.y, 0f);
                    Quaternion editedNextRot = Quaternion.Euler(lastTickRot.eulerAngles.x, nextTickRot.eulerAngles.y, 0f);
                    viewRotation = Quaternion.Lerp(editedLastRot, editedNextRot, framePercentage);

                    transform.position = Vector3.Lerp(lastTickPos, nextTickPos, framePercentage) + new Vector3(0f, crouchPercentage * -0.5f, 0f);
                    transform.localScale = new Vector3(1f, 1f - 0.5f * crouchPercentage, 1f);
                }
                else
                {
                    playing = false;
                    StartCoroutine(EndDemoDelay());
                }
            }

            if (!topView)
            {
                ghostCam.transform.position = transform.position + (viewRotation * firstPersonCamOffset);
                ghostCam.transform.rotation = viewRotation;
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

        public void PlayDemo(Demo demo, bool doLoop = false, bool topView = false)
        {
            looping = doLoop;
            this.topView = topView;
            tickList = demo.Ticks;

            Vector3[] lineRenderTicks = new Vector3[tickList.Count / 10];
            for (int i = 0; i < lineRenderTicks.Length; i++)
            {
                lineRenderTicks[i] = tickList[i * 10].Position;
            }
            GetComponent<LineRenderer>().numPositions = lineRenderTicks.Length;
            GetComponent<LineRenderer>().SetPositions(lineRenderTicks);

            //Set up camera
            if (topView)
            {
                Assert.IsTrue(WorldInfo.info.ReplayCams.Count > 0);
                WorldInfo.info.ReplayCams[0].enabled = true;
            }
            else
            {
                ghostCam = Instantiate(ghostCamPrefab);
                Camera cam = ghostCam.GetComponent<Camera>();
                cam.fieldOfView = Settings.GameSettings.SingletonInstance.Fov;
            }

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

            if (!topView)
            {
                ghostCam.transform.position = tickList[0].Position;
                ghostCam.transform.rotation = tickList[0].Rotation;
            }

            startPlayTime = Time.time;
            playing = true;
        }
    }
}