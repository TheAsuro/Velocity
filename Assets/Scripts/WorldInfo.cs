using System;
using UnityEngine;
using System.Collections.Generic;
using UI;

public class WorldInfo : MonoBehaviour
{
    public static WorldInfo info;

    [SerializeField] private WorldData worldData;

    [SerializeField] private Respawn firstSpawn = null;

    public RaceScript RaceScript
    {
        get { return GetComponent<RaceScript>(); }
    }

    public Respawn FirstSpawn
    {
        get { return firstSpawn; }
    }

    private List<GameObject> skyboxWatchers = new List<GameObject>();

    private void Awake()
    {
        info = this;
        CreatePlayer(firstSpawn);
        RaceScript.OnReset += (s, e) => GameMenu.SingletonInstance.CloseAllWindows();
    }

    private void OnDestroy()
    {
        info = null;
    }

    public void CreatePlayer(Respawn spawnpoint, bool startInEditorMode = false)
    {
        //Instantiate a new player at the spawnpoint's location
        GameObject newPlayer = Instantiate(worldData.playerTemplate, Vector3.zero, Quaternion.identity);
        PlayerBehaviour newPlayerBehaviour = newPlayer.GetComponent<PlayerBehaviour>();

        //Set up player
        newPlayerBehaviour.ResetPosition(spawnpoint.GetSpawnPos(), spawnpoint.GetSpawnRot());
        newPlayerBehaviour.SetWorldBackgroundColor(worldData.backgroundColor);

        // UI
        GameMenu.SingletonInstance.AddWindow(Window.PLAY);
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

    public WorldData WorldData
    {
        get { return worldData; }
    }
}