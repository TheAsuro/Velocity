using UnityEngine;
using System.Collections;

public class DoorMoveEvent : Event
{
	public Vector3 relativeMovement;
	public float duration = 5f;

	private Vector3 startPos;
	private Vector3 endPos;
	private float startTime;
	private bool running = false;

	void Awake()
	{
		startPos = transform.position;
	}

	void Update()
	{
		if(running)
		{
			float completion = (Time.time - startTime) / duration;
			transform.position = Vector3.Lerp(startPos, endPos, completion);

			if(Time.time > startTime + duration)
			{
				running = false;
			}
		}
	}

	public override void fire(params object[] parameters)
	{
		if(!running)
		{
			running = true;
			endPos = startPos + relativeMovement;
			startTime = Time.time;
		}
	}
}
