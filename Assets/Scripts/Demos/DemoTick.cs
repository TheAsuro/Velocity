using UnityEngine;
using System.Collections;

[System.Serializable]
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

	public void setTime(float value)
	{
		time = value;
	}

	public float getTime()
	{
		return time;
	}

	public void setPosition(Vector3 value)
	{
		position = value;
	}

	public Vector3 getPosition()
	{
		return position;
	}

	public void setRotation(Quaternion value)
	{
		rotation = value;
	}

	public Quaternion getRotation()
	{
		return rotation;
	}
}
