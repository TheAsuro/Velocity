using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Demos;
using Game;
using Race;
using UI;
using UnityEngine.Assertions;
using Util;

public class WorldInfo : MonoBehaviour
{
    public static WorldInfo info;

    public event EventHandler<EventArgs<Checkpoint>> OnCheckpointTrigger;

    public RaceScript RaceScript { get; private set; }

    [SerializeField] private Checkpoint firstSpawn;
    public Checkpoint FirstSpawn
    {
        get { return firstSpawn; }
    }

    [SerializeField] private WorldData worldData;
    public WorldData WorldData
    {
        get { return worldData; }
    }

    private DemoPlayer demoPlayer;

    private List<GameObject> skyboxWatchers = new List<GameObject>();
    private SortedList<int, Checkpoint> levelCheckpoints = new SortedList<int, Checkpoint>();

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

    public void StopDemo()
    {
        Assert.IsNotNull(demoPlayer);
        demoPlayer.StopDemoPlayback(true);
        RaceScript = null;
    }

    public void CreatePlayer(bool startInEditorMode)
    {
        //Instantiate a new player at the spawnpoint's location
        GameObject newPlayer = Instantiate(worldData.playerTemplate, Vector3.zero, Quaternion.identity);
        RaceScript = newPlayer.GetComponent<GameRaceScript>();

        //Set up player
        RaceScript.PrepareNewRun();

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

    public void AddCheckpoint(Checkpoint cp)
    {
        if (levelCheckpoints.ContainsKey(cp.Index))
            throw new ArgumentException("Too many checkpoints with #" + cp.Index);

        levelCheckpoints.Add(cp.Index, cp);
        cp.OnPlayerTrigger += (sender, args) =>
        {
            if (OnCheckpointTrigger != null)
                OnCheckpointTrigger(this, new EventArgs<Checkpoint>(cp));
        };
    }

    public bool IsLastCheckpoint(int checkpointIndex)
    {
        return levelCheckpoints.Last().Key == checkpointIndex;
    }
}