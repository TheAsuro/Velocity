using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Demos;
using UI;
using Console = UI.Console;

public class GameInfo : MonoBehaviour
{
	public static GameInfo info;
	public delegate string InfoString();
	public GameObject playerTemplate;
	public GUISkin skin;
	public string secretKey = "";
	
	//Gamestates
	private bool gamePaused = false;
	private MenuState menuState = MenuState.CLOSED;
	private bool viewLocked = false;
	public bool menuLocked = false;

	//GUI
	private GameObject escMenu;
	private GameObject endLevel;
	private GameObject myLeaderboardObj;
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
    { get { return lastTime.ToString("0.0000"); } }
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
	private Console myConsole;
	private GameObject myCanvas;
	private GameObject myConsoleWindow;
	private GameObject myDebugWindow;
	private Text myDebugWindowText;

    //Load infos like player name, pb's, etc.
    private SaveData currentSave;
    public SaveData CurrentSave
    {
        get
        {
            return currentSave;
        }
        set
        {
            currentSave = value;
            if (value != null)
            {
                PlayerPrefs.SetString("lastplayer", currentSave.Name);
            }
        }
    }

    private bool lastRunWasPb;
    public bool LastRunWasPb { get { return lastRunWasPb; } }

    public enum MenuState
	{
		CLOSED,
		ESCMENU,
		INACTIVE,
		DEMO,
		LEADERBOARD,
		ENDLEVEL,
		OTHERMENU,
		EDITOR,
		EDITORPLAY
	}

    public enum LevelLoadMode
    {
        PLAY,
        DEMO
    }

    private void Awake()
	{
		if(GameInfo.info == null)
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
		escMenu = myCanvas.transform.Find("EscMenu").gameObject;
		endLevel = myCanvas.transform.Find("EndLevel").gameObject;
		myConsoleWindow = myCanvas.transform.Find("Console").gameObject;
		myDebugWindow = myCanvas.transform.Find("Debug").gameObject;
		myDebugWindowText = myDebugWindow.transform.Find("Text").GetComponent<Text>();
		myLeaderboardObj = endLevel.transform.Find("Leaderboards").gameObject;
		SetMenuState(MenuState.CLOSED);

        fx = new GameInfoFx(myCanvas.transform.FindChild("FxImage").GetComponent<Image>());
	}

    private void Update()
	{
        Settings.Input.ExecuteBoundActions();
        Api.HttpApi.ConsumeCallbacks();

        //TODO put into binds
		if(Input.GetButtonDown("Debug"))
		{
			myDebugWindow.SetActive(!myDebugWindow.activeSelf);
		}
		
		if(Input.GetButtonDown("Menu"))
		{
			ToggleEscMenu();
		}
        
		//Update fps every 0.1 seconds
		if(lastFpsRecordTime + 0.1f < Time.time || lastFpsRecordTime < 0f)
		{
			lastFps = Mathf.RoundToInt(1 / Time.smoothDeltaTime);
			lastFpsRecordTime = Time.time;
		}
		myDebugWindowText.text = lastFps.ToString() + " FPS\n";

		//Draw debug window lines
		if(GetPlayerInfo() != null)
		{
			string str = "";
		
			for(int i = 0; i < windowLines.Count; i++)
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

        menuLocked = false;
        WorldInfo wInfo = WorldInfo.info;

        //Initialize based on loadMode
        if(loadMode == LevelLoadMode.PLAY)
        {
            if (wInfo != null)
                SetMenuState(wInfo.beginState);
            else
                SetMenuState(MenuState.INACTIVE);
        }
        else if(loadMode == LevelLoadMode.DEMO)
        {
            SetMenuState(MenuState.DEMO);
            PlayDemo(currentDemo);
        }

        fx.StartFadeToColor(Color.black, new Color(0f, 0f, 0f, 0f), 0.5f, delegate { });
	}

	//Load a level
	public void LoadLevel(string name)
	{
        //Server stuff might be here later
        fx.StartFadeToColor(new Color(0f, 0f, 0f, 0f), Color.black, 0.5f, delegate { SceneManager.LoadScene(name); });
	}

	//Creates a new local player (the one that is controlled by the current user)
	public void SpawnNewPlayer(Respawn spawnpoint, bool killOldPlayer = true, bool startInEditorMode = false)
	{
		if(killOldPlayer || GetPlayerInfo() == null)
		{
			//Remove old player
			SetPlayerInfo(null);

			//Instantiate a new player at the spawnpoint's location
			GameObject newPlayer = (GameObject)GameObject.Instantiate(playerTemplate, Vector3.zero, Quaternion.identity);
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
        lastTime = time.Ticks / (decimal)10000000;

        lastRunWasPb = CurrentSave.SaveIfPersonalBest(lastTime, SceneManager.GetActiveScene().name);

        currentDemo = myPlayer.GetDemo();
        SendLeaderboardEntry(lastTime, SceneManager.GetActiveScene().name, currentDemo);
	}

	//Player hit the exit trigger
	public void LevelFinished()
	{
        SetMenuState(MenuState.ENDLEVEL);

        PlayRaceDemo();

		SetPlayerInfo(null);
	}

	//Plays a sound at the player position
	public void PlaySound(string name)
	{
		if(myPlayer != null)
		{
			for(int i = 0; i < soundNames.Count; i++)
			{
				if(soundNames[i] == name)
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
        UnlockMenu();
		SetMenuState(MenuState.CLOSED);
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

	public bool IsConsoleOpen()
	{
		return myConsole.IsVisible();
	}

	public Rect GetConsoleTitleRect()
	{
		Canvas c = myCanvas.GetComponent<Canvas>();
		RectTransform titleTransform = myConsoleWindow.transform.Find("Title").gameObject.GetComponent<RectTransform>();
		Rect r = new Rect(titleTransform.position.x * c.scaleFactor - (titleTransform.rect.width / 2f),
		                  titleTransform.position.y * c.scaleFactor - (titleTransform.rect.height / 2f),
		                  titleTransform.rect.width,
		                  titleTransform.rect.height);
		return r;
	}

	//Version for new gui
	public void SetMenuState(string state)
	{
		switch(state)
		{
			case "closed":
				SetMenuState(MenuState.CLOSED);
				break;
			case "escmenu":
				SetMenuState(MenuState.ESCMENU);
				break;
			case "inactive":
				SetMenuState(MenuState.INACTIVE);
				break;
			case "demo":
				SetMenuState(MenuState.DEMO);
				break;
			case "leaderboard":
				SetMenuState(MenuState.LEADERBOARD);
				break;
			case "endlevel":
				SetMenuState(MenuState.ENDLEVEL);
				break;
			case "othermenu":
				SetMenuState(MenuState.OTHERMENU);
				break;
			case "editor":
				SetMenuState(MenuState.EDITOR);
				break;
			case "editorplay":
				SetMenuState(MenuState.EDITORPLAY);
				break;
		}
	}

	//Menu state manager
	public void SetMenuState(MenuState state)
	{
        if(!menuLocked)
		{
			//Reset all states
			SetGamePaused(true);
			escMenu.SetActive(false);
			endLevel.SetActive(false);
			myLeaderboardObj.SetActive(false);
            SetCursorLock(false);

			switch(state)
			{
				case MenuState.CLOSED:
					SetGamePaused(false);
                    SetCursorLock(true);
					break;
				case MenuState.ESCMENU:
					escMenu.SetActive(true);
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
					myLeaderboardObj.SetActive(true);
                    myLeaderboardObj.GetComponent<LeaderboardDisplay>().LoadMap(SceneManager.GetActiveScene().name);
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
			}

			menuState = state;
		}
	}

	private void ToggleEscMenu()
	{
		if(menuState == MenuState.CLOSED)
		{
			SetMenuState(MenuState.ESCMENU);
		}
		else
		{
			SetMenuState(MenuState.CLOSED);
		}
	}

	public void ToggleLeaderboard()
	{
		if(myLeaderboardObj.activeSelf)
		{
			SetMenuState(MenuState.ENDLEVEL);
			return;
		}
		SetMenuState(MenuState.LEADERBOARD);
	}
    
    private void Connected()
    {
        print("connected");
    }

	public MenuState GetMenuState()
	{
		return menuState;
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
	
		if(value)
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
            if(GetPlayerInfo() != null)
                GetPlayerInfo().SetPause(false);
		}
	}

	public void SetConsole(Console pConsole)
	{
		myConsole = pConsole;
	}

	public Console GetConsole()
	{
		return myConsole;
	}

	//Write a string to the console
	public void WriteToConsole(string text)
	{
		if(myConsole)
			myConsole.WriteToConsole(text);
	}
	
	public bool GetGamePaused()
	{
		return gamePaused;
	}

	//Sets the reference to the player
	//If info is null, current player will be removed
	public void SetPlayerInfo(PlayerBehaviour info)
	{
		if(info == null)
		{
			//Destroy the player if there still is one
			if(myPlayer != null)
			{
				Destroy(myPlayer.gameObject);
			}
		}

		myPlayer = info;
	}

	public PlayerBehaviour GetPlayerInfo()
	{
		return myPlayer;
	}

	public void StartDemo()
	{
		ResetRun();

		//check if there is a player and we are not in editor
		if(myPlayer != null && GetMenuState() != MenuState.EDITOR && GetMenuState() != MenuState.EDITORPLAY)
			myPlayer.StartDemo(currentSave.Account.Name);
	}

	public void StopDemo()
	{
		if(myPlayer != null)
			myPlayer.StopDemo();
	}

	//Plays a demo and returns to main menu
	public void PlayDemo(Demo demo)
	{
        currentDemo = demo;

        //Reload level if in wrong mode/level (this function will be called again)
        if(currentDemo.GetLevelName() != SceneManager.GetActiveScene().name || loadMode != LevelLoadMode.DEMO)
        {
            loadMode = LevelLoadMode.DEMO;
            LoadLevel(currentDemo.GetLevelName());
            return;
        }

        myDemoPlayer.PlayDemo(currentDemo, delegate { loadMode = LevelLoadMode.PLAY; LoadLevel("MainMenu"); });
	}

    //Plays the current demo after a race is finished
	private void PlayRaceDemo()
	{
        if(myDemoPlayer != null && currentDemo != null)
            myDemoPlayer.PlayDemo(currentDemo, delegate { menuLocked = false; SetMenuState(MenuState.ENDLEVEL); }, true);
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
		if(!viewLocked)
		{
			if(myPlayer != null)
			{
				myPlayer.SetMouseView(value);
			}
		}
	}

	//MouseLook is locked to given value, even if menu states change
	//Overrides old locked value
	public void LockMouseView(bool value)
	{
		if(myPlayer != null)
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

	//MenuState can not be changed
	public void LockMenu()
	{
		menuLocked = true;
	}

	public void UnlockMenu()
	{
		menuLocked = false;
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
            print("Invalid save.");
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
	
		for(int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
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
		if(GetPlayerInfo().GetCheats() || Physics.gravity != defGravity)
			InvalidateRun("Cheat check returned positive");
	}

	private void ResetRun()
	{
		if(GetPlayerInfo().GetCheats())
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

    public void StartFadeToColor(Color start, Color end, float duration, Callback callback)
    {
        Effect e = new Effect();
        e.startTime = Time.unscaledTime;
        e.duration = duration;
        e.startColor = start;
        e.endColor = end;
        e.update = FadeToColor;
        e.callback = callback;
        activeEffects.Add(e);
    }

    private void FadeToColor(Effect effect)
    {
        //Fade
        float progress = Interpolate(effect.startTime, Time.unscaledTime, effect.startTime + effect.duration);
        effectImage.color = Color.Lerp(effect.startColor, effect.endColor, progress);

        //Check if we are done
        if(progress >= 1f)
        {
            effect.callback();
            activeEffects.Remove(effect);
        }
    }

    //Returns 0 if current == start; returns 1 if current == end
    private float Interpolate(float start, float current, float end)
    {
        return (current - start) / (end - start);
    }
}