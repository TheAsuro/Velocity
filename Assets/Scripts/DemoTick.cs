using UnityEngine;
using System.Collections;

public class DemoTick
{
	private float time;
	private Vector3 position;
	private Quaternion rotation;

	public DemoTick(float recordtime, Vector3 pos, Quaternion rot)
	{
		time = recordtime;
		position = pos;
		rotation = rot;
	}

	public float getTime()
	{
		return time;
	}

	public Vector3 getPosition()
	{
		return position;
	}

	public Quaternion getRotation()
	{
		return rotation;
	}
}
