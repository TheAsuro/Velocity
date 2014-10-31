using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldInfo : MonoBehaviour
{
	public static WorldInfo info;
	public GameInfo.MenuState beginState = GameInfo.MenuState.closed;
	public Color worldBackgroundColor = Color.black;
	public delegate void Reset();

	private Dictionary<string,Reset> resetList = new Dictionary<string,Reset>();

	//Respawn with r
	private Respawn currentSpawn = null;
	private Respawn firstSpawn = null;

	void Awake()
	{
		info = this;
	}

	public void addResetMethod(Reset reset, string id)
	{
		if(resetList.ContainsKey(id))
		{
			resetList.Remove(id);
		}
		resetList.Add(id, reset);
	}

	public void setSpawn(Respawn spawn)
	{
		if(firstSpawn == null)
		{
			firstSpawn = spawn;
		}
		currentSpawn = spawn;
		addResetMethod(resetSpawn, "spawn reset");
	}

	private void resetSpawn()
	{
		currentSpawn = firstSpawn;
	}

	public Respawn getCurrentSpawn()
	{
		return currentSpawn;
	}

	public Respawn getFirstSpawn()
	{
		return firstSpawn;
	}

	public void reset()
	{
		foreach(Reset r in resetList.Values)
		{
			r();
		}
	}
}
