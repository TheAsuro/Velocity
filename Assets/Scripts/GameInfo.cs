using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Api;
using Demos;
using UI;
using UI.MenuWindows;

public class GameInfo : MonoBehaviour
{
    public static GameInfo info;

    public delegate string InfoString();

    public GameObject playerTemplate;
    public GUISkin skin;
    public string secretKey = "";

    //Gamestates
    private bool gamePaused = false;
    private bool viewLocked = false;

    //GUI
    private string selectedMap;
    private string selectedAuthor = "?";
    private GameInfoFx fx;

    //Sound
    public List<string> soundNames;
    public List<AudioClip> soundClips;

    //Stuff
    private Demo currentDemo;
    private decimal lastTime = -1;
    public string LastTimeString
    {
        get { return lastTime.ToString("0.0000"); }
    }

    private static Vector3 defGravity = new Vector3(0f, -15f, 0f);
    private bool runValid = false;
    public LevelLoadMode loadMode = LevelLoadMode.PLAY;

    //Debug window (top-left corner, toggle with f8)
    public bool logToConsole = true;
    private float lastFps = 0f;
    private float lastFpsRecordTime = -1f;
    private List<string> linePrefixes = new List<string>();
    private List<InfoString> windowLines = new List<InfoString>();

    //GUI settings
    public float circleSpeed1 = 10f;
    public float circleSpeed2 = 20f;
    public float circleSpeed3 = 30f;

    //Editor
    public string editorLevelName = "";

    //References
    private PlayerBehaviour myPlayer;
    private DemoPlay myDemoPlayer;
    private ConsoleWindow myConsoleWindow;
    private GameObject myCanvas;
    private GameObject myConsoleWindowObject;
    private GameObject myDebugWindow;
    private Text myDebugWindowText;

    //Load infos like player name, pb's, etc.
    private SaveData currentSave;
    public SaveData CurrentSave
    {
        get { return currentSave; }
        set
        {
            currentSave = value;
            if (value != null)
            {
                PlayerPrefs.SetString("lastplayer", currentSave.Name);
            }
        }
    }

    public bool LastRunWasPb { get; private set; }

    public enum LevelLoadMode
    {
        PLAY,
        DEMO
    }

    private void Awake()
    {
        if (GameInfo.info == null)
        {
            info = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        myDemoPlayer = gameObject.GetComponent<DemoPlay>();

        myCanvas = transform.Find("Canvas").gameObject;
        myDebugWindow = myCanvas.transform.Find("Debug").gameObject;
        myDebugWindowText = myDebugWindow.transform.Find("Text").GetComponent<Text>();

        fx = new GameInfoFx(myCanvas.transform.FindChild("FxImage").GetComponent<Image>());
    }

    private void Update()
    {
        Settings.Input.ExecuteBoundActions();
        HttpApi.ConsumeCallbacks();


        //TODO put into binds
        if (Input.GetButtonDown("Debug"))
        {
            myDebugWindow.SetActive(!myDebugWindow.activeSelf);
        }

        //Update fps every 0.1 seconds
        if (lastFpsRecordTime + 0.1f < Time.time || lastFpsRecordTime < 0f)
        {
            lastFps = Mathf.RoundToInt(1 / Time.smoothDeltaTime);
            lastFpsRecordTime = Time.time;
        }
        myDebugWindowText.text = lastFps.ToString() + " FPS\n";

        //Draw debug window lines
        if (GetPlayerInfo() != null)
        {
            string str = "";

            for (int i = 0; i < windowLines.Count; i++)
            {
                str += linePrefixes[i] + windowLines[i]() + "\n";
            }

            myDebugWindowText.text += str;
        }
        else
        {
            myDebugWindowText.text += "No player";
        }

        //Update effects
        fx.Update();
    }

    //Prepare for new level
    private void OnLevelWasLoaded(int level)
    {
        Settings.AllSettings.LoadSettings();
        RemoveAllWindowLines();

        //Initialize based on loadMode
        if (loadMode == LevelLoadMode.DEMO)
        {
            PlayDemo(currentDemo);
        }

        fx.StartFadeToColor(Color.black, new Color(0f, 0f, 0f, 0f), 0.5f);
    }

    //Load a level
    public void LoadLevel(string levelName)
    {
        //Server stuff might be here later
        fx.StartFadeToColor(new Color(0f, 0f, 0f, 0f), Color.black, 0.5f, delegate { SceneManager.LoadScene(levelName); });
    }

    //Creates a new local player (the one that is controlled by the current user)
    public void SpawnNewPlayer(Respawn spawnpoint, bool killOldPlayer = true, bool startInEditorMode = false)
    {
        if (killOldPlayer || GetPlayerInfo() == null)
        {
            //Remove old player
            SetPlayerInfo(null);

            //Instantiate a new player at the spawnpoint's location
            GameObject newPlayer = Instantiate(playerTemplate, Vector3.zero, Quaternion.identity);
            SetPlayerInfo(newPlayer.GetComponent<PlayerBehaviour>());

            //Set up player
            myPlayer.ResetPosition(spawnpoint.GetSpawnPos(), spawnpoint.GetSpawnRot());
            myPlayer.SetWorldBackgroundColor(WorldInfo.info.worldBackgroundColor);
        }

        myPlayer.EditorMode = startInEditorMode;
    }

    //Player hit the goal
    public void RunFinished(TimeSpan time)
    {
        StopDemo();
        CleanUpPlayer();
        lastTime = time.Ticks / (decimal) 10000000;

        LastRunWasPb = CurrentSave.SaveIfPersonalBest(lastTime, SceneManager.GetActiveScene().name);

        currentDemo = myPlayer.GetDemo();
        SendLeaderboardEntry(lastTime, SceneManager.GetActiveScene().name, currentDemo);
    }

    //Player hit the exit trigger
    public void LevelFinished()
    {
        PlayRaceDemo();
        SetPlayerInfo(null);
    }

    //Plays a sound at the player position
    public void PlaySound(string soundName)
    {
        if (myPlayer != null)
        {
            for (int i = 0; i < soundNames.Count; i++)
            {
                if (soundNames[i] == soundName)
                {
                    myPlayer.PlaySound(soundClips[i]);
                }
            }
        }
    }

    //Reset everything in the world to its initial state
    public void Reset()
    {
        StopDemo();
        CleanUpPlayer();
        WorldInfo.info.ResetWorld();
        GameMenu.SingletonInstance.CloseAllWindows();
        StartDemo();
    }

    //Removes all leftover things that could reference the player
    public void CleanUpPlayer()
    {
        RemoveAllWindowLines();
        currentDemo = null;
    }

    //Leave the game
    public void Quit()
    {
        Application.Quit();
    }

    //Menu state manager
    public void SetMenuState()
    {
        /*
        switch (state)
        {
            case MenuState.CLOSED:
                SetGamePaused(false);
                SetCursorLock(true);
                break;
            case MenuState.ESCMENU:
                SetMouseView(false);
                EscWindow escWindow = Instantiate(menuProperties.escWindowPrefab, myCanvas.transform).GetComponent<EscWindow>();
                escWindow.OnActivate();
                menuStack.Push(escWindow);
                break;
            case MenuState.DEMO:
                SetGamePaused(false);
                SetMouseView(false);
                menuLocked = true;
                SetCursorLock(true);
                break;
            case MenuState.LEADERBOARD:
                SetMouseView(false);
                endLevel.SetActive(true);
                MenuWindow leaderboard = WindowManager.CreateWindow<LeaderboardWindow>(menuProperties.leaderboardDisplayPrefab, myCanvas.transform);
                leaderboard.OnActivate();
                menuStack.Push(leaderboard);
                menuLocked = true;
                break;
            case MenuState.ENDLEVEL:
                SetGamePaused(false);
                SetMouseView(false);
                endLevel.SetActive(true);
                menuLocked = true;
                break;
            case MenuState.OTHERMENU:
                menuLocked = true;
                break;
            case MenuState.EDITOR:
                menuLocked = true;
                SetGamePaused(false);
                break;
            case MenuState.EDITORPLAY:
                menuLocked = true;
                SetGamePaused(false);
                SetCursorLock(true);
                break;
        }*/
    }

    //Draws some info in the debug window, add a prefix and a function that returns a string
    public void AddWindowLine(string prefix, InfoString stringFunction)
    {
        linePrefixes.Add(prefix);
        windowLines.Add(stringFunction);
    }

    //Remove everything from the debug window
    private void RemoveAllWindowLines()
    {
        linePrefixes.Clear();
        windowLines.Clear();
    }

    private void SetGamePaused(bool value)
    {
        gamePaused = value;

        if (value)
        {
            SetMouseView(false);
            Time.timeScale = 0f;
            if (GetPlayerInfo() != null)
                GetPlayerInfo().SetPause(true);
        }
        else
        {
            SetMouseView(true);
            Time.timeScale = 1f;
            if (GetPlayerInfo() != null)
                GetPlayerInfo().SetPause(false);
        }
    }

    public bool GetGamePaused()
    {
        return gamePaused;
    }

    //Sets the reference to the player
    //If info is null, current player will be removed
    public void SetPlayerInfo(PlayerBehaviour behaviour)
    {
        if (behaviour == null)
        {
            //Destroy the player if there still is one
            if (myPlayer != null)
            {
                Destroy(myPlayer.gameObject);
            }
        }

        myPlayer = behaviour;
    }

    public PlayerBehaviour GetPlayerInfo()
    {
        return myPlayer;
    }

    public void StartDemo()
    {
        ResetRun();

        //check if there is a player and we are not in editor
        if (myPlayer != null)
            myPlayer.StartDemo(currentSave.Account.Name);
    }

    public void StopDemo()
    {
        if (myPlayer != null)
            myPlayer.StopDemo();
    }

    //Plays a demo and returns to main menu
    public void PlayDemo(Demo demo)
    {
        currentDemo = demo;

        //Reload level if in wrong mode/level (this function will be called again)
        if (currentDemo.GetLevelName() != SceneManager.GetActiveScene().name || loadMode != LevelLoadMode.DEMO)
        {
            loadMode = LevelLoadMode.DEMO;
            LoadLevel(currentDemo.GetLevelName());
            return;
        }

        myDemoPlayer.PlayDemo(currentDemo, delegate
        {
            loadMode = LevelLoadMode.PLAY;
            LoadLevel("MainMenu");
        });
    }

    //Plays the current demo after a race is finished
    private void PlayRaceDemo()
    {
        if (myDemoPlayer != null && currentDemo != null)
            myDemoPlayer.PlayDemo(currentDemo, delegate { GameMenu.SingletonInstance.AddWindow(Window.ENDLEVEL); }, true);
    }

    public decimal GetLastTime()
    {
        return lastTime;
    }

    //Save demo to ".vdem" file, does not work in web player
    public void SaveLastDemo()
    {
        if (currentDemo != null)
            currentDemo.SaveToFile(Application.dataPath);
    }

    //Can the player move the camera with the mouse
    //Can be blocked by lockMouseView
    public void SetMouseView(bool value)
    {
        if (!viewLocked)
        {
            if (myPlayer != null)
            {
                myPlayer.SetMouseView(value);
            }
        }
    }

    //MouseLook is locked to given value, even if menu states change
    //Overrides old locked value
    public void LockMouseView(bool value)
    {
        if (myPlayer != null)
        {
            myPlayer.SetMouseView(value);
        }
        viewLocked = true;
    }

    //MouseLook can be changed by menu again
    public void UnlockMouseView()
    {
        viewLocked = false;
    }

    //Map selection in main menu
    public void SetSelectedMap(string map, string author = "?")
    {
        selectedMap = map;
        selectedAuthor = author;
    }

    public string GetSelectedMap()
    {
        return selectedMap;
    }

    public string GetSelectedAuthor()
    {
        return selectedAuthor;
    }

    //Send a leaderboard entry to leaderboard server, with a automatically generated hash.
    //This includes a secret key that will be included in the final game (and not uploaded to github),
    //so nobody can send fake entries.
    private void SendLeaderboardEntry(decimal time, string map, Demo demo)
    {
        InvalidRunCheck();
        if (!runValid)
        {
            print("Invalid run!");
            return;
        }
        if (currentSave == null)
        {
            print("Invalid save!");
            return;
        }
        if (!currentSave.Account.IsLoggedIn)
        {
            print("Account not logged in!");
            return;
        }

        Leaderboard.SendEntry(currentSave.Account.Name, time, map, currentSave.Account.Token, demo);
    }

    //Create a md5 hash from a string
    public string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        //encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        //Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        foreach (byte hashByte in hashBytes)
        {
            hashString += Convert.ToString(hashByte, 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    //Setting gravity directly, this is the only game variable that is not set in playerinfo
    public void SetGravity(float value)
    {
        Physics.gravity = new Vector3(0f, value, 0f);
        InvalidateRun("Changed gravity");
    }

    //Run will not be uploaded to leaderboards
    public void InvalidateRun(string message = "undefined")
    {
        runValid = false;
        print("Run was invalidated. Reason: " + message);
    }

    public bool IsRunValid()
    {
        return runValid;
    }

    private void InvalidRunCheck()
    {
        if (GetPlayerInfo().GetCheats() || Physics.gravity != defGravity)
            InvalidateRun("Cheat check returned positive");
    }

    private void ResetRun()
    {
        if (GetPlayerInfo().GetCheats())
        {
            runValid = false;
        }
        else
        {
            runValid = true;
            InvalidRunCheck();
        }
    }

    public void SetCursorLock(bool value)
    {
        CursorLockMode mode = CursorLockMode.None;
        if (value)
            mode = CursorLockMode.Locked;
        Cursor.lockState = mode;
        Cursor.visible = !value;
    }
}

internal class GameInfoFx
{
    public delegate void Callback();

    private delegate void EffectUpdate(Effect effect);

    private Image effectImage;

    private List<Effect> activeEffects;

    private struct Effect
    {
        public float startTime;
        public float duration;
        public EffectUpdate update;
        public Callback callback;
        public Color startColor;
        public Color endColor;
    }

    public GameInfoFx(Image image)
    {
        activeEffects = new List<Effect>();

        effectImage = image;
    }

    //Call this in the script's update function
    public void Update()
    {
        //Go through active effects and update them
        for (int i = 0; i < activeEffects.Count; i++)
        {
            activeEffects[i].update(activeEffects[i]);
        }

        Settings.Input.ExecuteBoundActions();
    }

    public void StartFadeToColor(Color start, Color end, float duration, Callback callback = null)
    {
        Effect e = new Effect
        {
            startTime = Time.unscaledTime,
            duration = duration,
            startColor = start,
            endColor = end,
            update = FadeToColor,
            callback = callback
        };
        activeEffects.Add(e);
    }

    private void FadeToColor(Effect effect)
    {
        //Fade
        float progress = Interpolate(effect.startTime, Time.unscaledTime, effect.startTime + effect.duration);
        effectImage.color = Color.Lerp(effect.startColor, effect.endColor, progress);

        //Check if we are done
        if (progress >= 1f)
        {
            if (effect.callback != null)
                effect.callback();
            activeEffects.Remove(effect);
        }
    }

    //Returns 0 if current == start; returns 1 if current == end
    private static float Interpolate(float start, float current, float end)
    {
        return (current - start) / (end - start);
    }
}