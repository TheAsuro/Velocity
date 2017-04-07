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

    public Material skybox;

    [SerializeField] private float deathHeight;

    public float DeathHeight
    {
        get { return deathHeight; }
    }

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

    [SerializeField] private List<Camera> replayCams;
    [SerializeField] private Camera previewCam;

    private DemoPlayer demoPlayer;

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
        GameMenu.SingletonInstance.CloseAllWindows();
        GameMenu.SingletonInstance.AddWindow(Window.PLAY);
    }

    public void RemovePlayer()
    {
        RaceScript = null;
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

    public bool IsEndCheckpoint(Checkpoint cp)
    {
        return levelCheckpoints.Last().Value == cp;
    }

    public enum CameraMode
    {
        PREVIEW,
        TOP,
        FIRST_PERSON,
    }

    private CameraMode activeCameraMode = CameraMode.PREVIEW;
    public CameraMode ActiveCameraMode
    {
        get { return activeCameraMode; }
        set
        {
            activeCameraMode = value;
            previewCam.enabled = false;
            replayCams.ForEach(cam => cam.enabled = false);
            switch (value)
            {
                case CameraMode.PREVIEW:
                    previewCam.enabled = true;
                    break;
                case CameraMode.TOP:
                    Assert.IsTrue(replayCams.Count > 0);
                    replayCams[0].enabled = true;
                    break;
                case CameraMode.FIRST_PERSON:
                    break;
                default:
                    Assert.IsTrue(true, "Unknown camera mode!");
                    break;
            }
        }
    }
}