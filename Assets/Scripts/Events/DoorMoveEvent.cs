using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class DoorMoveEvent : Event
    {
        public List<Vector3> positions = new List<Vector3>();
        public float movementTime = 2.5f;
        public bool addToResetList = true;
        public bool repeat = false;

        private Vector3 initialPos;
        private Vector3 startPos;
        private Vector3 endPos;
        private float startTime;
        private bool running = false;
        private int positionCounter = 0;

        private void Awake()
        {
            initialPos = transform.position;
        }

        private void Start()
        {
            if(addToResetList)
            {
                WorldInfo.info.RaceScript.OnReset += (s, e) => Reset();
            }
        }

        private void Update()
        {
            if(running)
            {
                float completion = (Time.time - startTime) / movementTime;
                transform.position = Vector3.Lerp(startPos, endPos, completion);

                if(Time.time > startTime + movementTime)
                {
                    positionCounter++;
                    if(positionCounter >= positions.Count)
                    {
                        if(repeat)
                        {
                            Reset();
                            Fire(null);
                        }
                        else
                        {
                            Stop();
                        }
                    }
                    else
                    {
                        UpdatePositions();
                    }
                }
            }
        }

        private void UpdatePositions()
        {
            startPos = transform.position;
            endPos = initialPos + positions[positionCounter];
            startTime = Time.time;
        }

        private void Stop()
        {
            running = false;
            positionCounter = 0;
        }

        public override void Fire(params object[] parameters)
        {
            if(!running)
            {
                running = true;
                UpdatePositions();
            }
        }

        public override void Reset()
        {
            Stop();
            transform.position = initialPos;
        }
    }
}