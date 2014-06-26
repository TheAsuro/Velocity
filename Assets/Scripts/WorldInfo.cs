using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldInfo : MonoBehaviour
{
	public static WorldInfo info;
	public delegate void Reset();

	private List<Reset> resetList = new List<Reset>();

	void Awake()
	{
		info = this;
	}

	public void addResetMethod(Reset reset)
	{
		resetList.Add(reset);
	}

	public void reset()
	{
		foreach(Reset r in resetList)
		{
			r();
		}
	}
}
