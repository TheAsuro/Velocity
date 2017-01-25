using System;
using UnityEngine;
using System.Collections.Generic;
using Demos;
using Race;
using UI;

public class WorldInfo : MonoBehaviour
{
    public static WorldInfo info;

    [SerializeField] private WorldData worldData;

    [SerializeField] private Respawn firstSpawn = null;

    private DemoPlayer demoPlayer;

    public RaceScript RaceScript { get; private set; }

    public Respawn FirstSpawn
    {
        get { return firstSpawn; }
    }

    private List<GameObject> skyboxWatchers = new List<GameObject>();

    private void Awake()
    {
        info = this;
    }

    private void OnDestroy()
    {
        info = null;
    }

    public void PlayDemo(Demo demo, bool loop, bool staticCam)
    {
        if (demoPlayer != null)
            demoPlayer.StopDemoPlayback();

        demoPlayer = Instantiate(worldData.demoPlayerTemplate).GetComponent<DemoPlayer>();
        RaceScript = demoPlayer.GetComponent<DemoRaceScript>();
        demoPlayer.PlayDemo(demo, loop, staticCam);
    }

    public void CreatePlayer(bool startInEditorMode)
    {
        //Instantiate a new player at the spawnpoint's location
        GameObject newPlayer = Instantiate(worldData.playerTemplate, Vector3.zero, Quaternion.identity);
        RaceScript = newPlayer.GetComponent<GameRaceScript>();

        //Set up player
        RaceScript.PrepareNewRace();

        // UI
        RaceScript.OnReset += (s, e) => GameMenu.SingletonInstance.CloseAllWindows();
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