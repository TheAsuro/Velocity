using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorMoveEvent : Event
{
	public List<Vector3> positions = new List<Vector3>();
	public float movementTime = 2.5f;
	public bool addToResetList = true;

	private Vector3 initialPos;
	private Vector3 startPos;
	private Vector3 endPos;
	private float startTime;
	private bool running = false;
	private int positionCounter = 0;

	void Start()
	{
		initialPos = transform.position;

		if(addToResetList)
		{
			WorldInfo.info.addResetMethod(reset, "reset door " + GetInstanceID().ToString());
		}
	}

	void Update()
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
					stop();
				}
				else
				{
					updatePositions();
				}
			}
		}
	}

	private void updatePositions()
	{
		startPos = transform.position;
		endPos = initialPos + positions[positionCounter];
		startTime = Time.time;
	}

	private void stop()
	{
		running = false;
		positionCounter = 0;
	}

	public override void fire(params object[] parameters)
	{
		if(!running)
		{
			running = true;
			updatePositions();
		}
	}

	public override void reset()
	{
		stop();
		transform.position = initialPos;
	}
}