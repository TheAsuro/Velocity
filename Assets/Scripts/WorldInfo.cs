using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;

public class WorldInfo : MonoBehaviour
{
	public static WorldInfo info;
	
	public Window beginWindow = Window.NONE;
	public Color worldBackgroundColor = Color.black;
    public Material worldSkybox;
	public float deathHeight = -100f;

	public delegate void Start();
	public delegate void Reset();

	private Dictionary<string,Start> startList = new Dictionary<string,Start>();
	private Dictionary<string,Reset> resetList = new Dictionary<string,Reset>();

    private List<GameObject> skyboxWatchers = new List<GameObject>();

	//Respawn with r
	private Respawn currentSpawn = null;
	private Respawn firstSpawn = null;

    private void Awake()
	{
		info = this;
	    GameMenu.SingletonInstance.AddWindow(beginWindow);
	}

	public void AddStartMethod(Start start, string id)
	{
		if(startList.ContainsKey(id))
		{
			startList.Remove(id);
		}
		startList.Add(id, start);
	}

	public void AddResetMethod(Reset reset, string id)
	{
		if(resetList.ContainsKey(id))
		{
			resetList.Remove(id);
		}
		resetList.Add(id, reset);
	}

	public void SetSpawn(Respawn spawn)
	{
		if(firstSpawn == null)
		{
			firstSpawn = spawn;
		}
		currentSpawn = spawn;
		AddResetMethod(ResetSpawn, "spawn reset");
	}

	private void ResetSpawn()
	{
		currentSpawn = firstSpawn;
	}

	public Respawn GetCurrentSpawn()
	{
		return currentSpawn;
	}

	public Respawn GetFirstSpawn()
	{
		return firstSpawn;
	}

	public void DoStart()
	{
		foreach(Start s in startList.Values)
		{
			s();
		}
	}

	public void ResetWorld()
	{
		foreach(Reset r in resetList.Values)
		{
			r();
		}
	}

    public void AddSkyboxWatcher(GameObject watcher)
    {
        skyboxWatchers.Add(watcher);
    }

    public void UpdateCameraSkyboxes()
    {
        foreach (GameObject watcher in skyboxWatchers)
        {
            watcher.GetComponent<CamSkybox>().UpdateSkybox();
        }
    }
}
