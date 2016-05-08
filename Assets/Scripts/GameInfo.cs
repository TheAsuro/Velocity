using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class GameInfo : MonoBehaviour
{
	public static GameInfo info;
	public delegate string InfoString();
	public GameObject playerTemplate;
	public GUISkin skin;
	public string secretKey = "";
	
	//Gamestates
	private bool gamePaused = false;
	private MenuState menuState = MenuState.closed;
	private bool viewLocked = false;
	public bool menuLocked = false;

	//GUI
	private GameObject escMenu;
	private GameObject endLevel;
	private GameObject myLeaderboardObj;
	private string selectedMap;
	private string selectedAuthor = "?";
    private GameInfoFX fx;

	//Sound
	public List<string> soundNames;
	public List<AudioClip> soundClips;

	//Stuff
	private Demo currentDemo;
	private decimal lastTime = -1;
    public string lastTimeString
    { get { return lastTime.ToString("0.0000"); } }
	private static Vector3 defGravity = new Vector3(0f, -15f, 0f);
	private bool runValid = false;
    public LevelLoadMode loadMode = LevelLoadMode.play;
	
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
	private PlayerInfo myPlayer;
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

    private bool lastRunWasPB;
    public bool LastRunWasPB { get { return lastRunWasPB; } }

    public enum MenuState
	{
		closed,
		escmenu,
		inactive,
		demo,
		leaderboard,
		endlevel,
		othermenu,
		editor,
		editorplay
	}

    public enum LevelLoadMode
    {
        play,
        demo
    }
	
	void Awake()
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
		SetMenuState(MenuState.closed);

        fx = new GameInfoFX(myCanvas.transform.FindChild("FxImage").GetComponent<Image>());
	}

	void Update()
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
			toggleEscMenu();
		}

        if (Input.GetButtonDown("HideUI") && myPlayer != null)
        {
            myPlayer.CanvasEnabled = !myPlayer.CanvasEnabled;
        }

		//Update fps every 0.1 seconds
		if(lastFpsRecordTime + 0.1f < Time.time || lastFpsRecordTime < 0f)
		{
			lastFps = Mathf.RoundToInt(1 / Time.smoothDeltaTime);
			lastFpsRecordTime = Time.time;
		}
		myDebugWindowText.text = lastFps.ToString() + " FPS\n";

		//Draw debug window lines
		if(getPlayerInfo() != null)
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
	void OnLevelWasLoaded(int level)
	{
        Settings.AllSettings.LoadSettings();

        removeAllWindowLines();

        menuLocked = false;
        WorldInfo wInfo = WorldInfo.info;

        //Initialize based on loadMode
        if(loadMode == LevelLoadMode.play)
        {
            if (wInfo != null)
                SetMenuState(wInfo.beginState);
            else
                SetMenuState(MenuState.inactive);
        }
        else if(loadMode == LevelLoadMode.demo)
        {
            SetMenuState(MenuState.demo);
            PlayDemo(currentDemo);
        }

        fx.StartFadeToColor(Color.black, new Color(0f, 0f, 0f, 0f), 0.5f, delegate { });
	}

	//Load a level
	public void loadLevel(string name)
	{
        //Server stuff might be here later
        fx.StartFadeToColor(new Color(0f, 0f, 0f, 0f), Color.black, 0.5f, delegate { SceneManager.LoadScene(name); });
	}

	//Creates a new local player (the one that is controlled by the current user)
	public void spawnNewPlayer(Respawn spawnpoint, bool killOldPlayer = true, bool startInEditorMode = false)
	{
		if(killOldPlayer || getPlayerInfo() == null)
		{
			//Remove old player
			setPlayerInfo(null);

			//Instantiate a new player at the spawnpoint's location
			GameObject newPlayer = (GameObject)GameObject.Instantiate(playerTemplate, Vector3.zero, Quaternion.identity);
			setPlayerInfo(newPlayer.GetComponent<PlayerInfo>());

			//Set up player
			myPlayer.resetPosition(spawnpoint.getSpawnPos(), spawnpoint.getSpawnRot());
			myPlayer.setWorldBackgroundColor(WorldInfo.info.worldBackgroundColor);
		}

		myPlayer.editorMode = startInEditorMode;
	}

	//Player hit the goal
	public void runFinished(TimeSpan time)
	{
		stopDemo();
		cleanUpPlayer();
        lastTime = time.Ticks / (decimal)10000000;

        lastRunWasPB = CurrentSave.SaveIfPersonalBest(lastTime, SceneManager.GetActiveScene().name);

        currentDemo = myPlayer.getDemo();
        sendLeaderboardEntry(lastTime, SceneManager.GetActiveScene().name, currentDemo);
	}

	//Player hit the exit trigger
	public void levelFinished()
	{
        //If we are in editor, stop the test run
        if (myPlayer.editorMode)
        {
            EditorInfo.info.EndTest();
            return;
        }

		SetMenuState(MenuState.endlevel);

        PlayRaceDemo();

		setPlayerInfo(null);
	}

	//Plays a sound at the player position
	public void playSound(string name)
	{
		if(myPlayer != null)
		{
			for(int i = 0; i < soundNames.Count; i++)
			{
				if(soundNames[i] == name)
				{
					myPlayer.playSound(soundClips[i]);
				}
			}
		}
	}

	//Reset everything in the world to its initial state
	public void reset()
	{
		stopDemo();
		cleanUpPlayer();
		WorldInfo.info.reset();
        unlockMenu();
		SetMenuState(MenuState.closed);
		startDemo();
	}

	//Removes all leftover things that could reference the player
	public void cleanUpPlayer()
	{
		removeAllWindowLines();
        currentDemo = null;
	}

	//Leave the game
	public void quit()
	{
		Application.Quit();
	}

	public bool isConsoleOpen()
	{
		return myConsole.isVisible();
	}

	public Rect getConsoleTitleRect()
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
				SetMenuState(MenuState.closed);
				break;
			case "escmenu":
				SetMenuState(MenuState.escmenu);
				break;
			case "inactive":
				SetMenuState(MenuState.inactive);
				break;
			case "demo":
				SetMenuState(MenuState.demo);
				break;
			case "leaderboard":
				SetMenuState(MenuState.leaderboard);
				break;
			case "endlevel":
				SetMenuState(MenuState.endlevel);
				break;
			case "othermenu":
				SetMenuState(MenuState.othermenu);
				break;
			case "editor":
				SetMenuState(MenuState.editor);
				break;
			case "editorplay":
				SetMenuState(MenuState.editorplay);
				break;
		}
	}

	//Menu state manager
	public void SetMenuState(MenuState state)
	{
        if(!menuLocked)
		{
			//Reset all states
			setGamePaused(true);
			escMenu.SetActive(false);
			endLevel.SetActive(false);
			myLeaderboardObj.SetActive(false);
            SetCursorLock(false);

			switch(state)
			{
				case MenuState.closed:
					setGamePaused(false);
                    SetCursorLock(true);
					break;
				case MenuState.escmenu:
					escMenu.SetActive(true);
					break;
				case MenuState.demo:
					setGamePaused(false);
					setMouseView(false);
                    menuLocked = true;
                    SetCursorLock(true);
					break;
				case MenuState.leaderboard:
					setMouseView(false);
					endLevel.SetActive(true);
					myLeaderboardObj.SetActive(true);
                    myLeaderboardObj.GetComponent<LeaderboardDisplay>().LoadMap(SceneManager.GetActiveScene().name);
					menuLocked = true;
					break;
				case MenuState.endlevel:
					setGamePaused(false);
					setMouseView(false);
					endLevel.SetActive(true);
					menuLocked = true;
					break;
				case MenuState.othermenu:
					menuLocked = true;
					break;
				case MenuState.editor:
					menuLocked = true;
					setGamePaused(false);
					break;
				case MenuState.editorplay:
					menuLocked = true;
					setGamePaused(false);
                    SetCursorLock(true);
					break;
			}

			menuState = state;
		}
	}

	private void toggleEscMenu()
	{
		if(menuState == MenuState.closed)
		{
			SetMenuState(MenuState.escmenu);
		}
		else
		{
			SetMenuState(MenuState.closed);
		}
	}

	public void toggleLeaderboard()
	{
		if(myLeaderboardObj.activeSelf)
		{
			SetMenuState(MenuState.endlevel);
			return;
		}
		SetMenuState(MenuState.leaderboard);
	}
    
    private void connected()
    {
        print("connected");
    }

	public MenuState getMenuState()
	{
		return menuState;
	}
	
	//Draws some info in the debug window, add a prefix and a function that returns a string
	public void addWindowLine(string prefix, InfoString stringFunction)
	{
		linePrefixes.Add(prefix);
		windowLines.Add(stringFunction);
	}

	//Remove everything from the debug window
	private void removeAllWindowLines()
	{
		linePrefixes.Clear();
		windowLines.Clear();
	}
	
	private void setGamePaused(bool value)
	{
		gamePaused = value;
	
		if(value)
		{
			setMouseView(false);
			Time.timeScale = 0f;
            if (getPlayerInfo() != null)
                getPlayerInfo().setPause(true);
		}
		else
		{
			setMouseView(true);
			Time.timeScale = 1f;
            if(getPlayerInfo() != null)
                getPlayerInfo().setPause(false);
		}
	}

	public void setConsole(Console pConsole)
	{
		myConsole = pConsole;
	}

	public Console getConsole()
	{
		return myConsole;
	}

	//Write a string to the console
	public void writeToConsole(string text)
	{
		if(myConsole)
			myConsole.writeToConsole(text);
	}
	
	public bool getGamePaused()
	{
		return gamePaused;
	}

	//Sets the reference to the player
	//If info is null, current player will be removed
	public void setPlayerInfo(PlayerInfo info)
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

	public PlayerInfo getPlayerInfo()
	{
		return myPlayer;
	}

	public void startDemo()
	{
		resetRun();

		//check if there is a player and we are not in editor
		if(myPlayer != null && getMenuState() != MenuState.editor && getMenuState() != MenuState.editorplay)
			myPlayer.startDemo(currentSave.Account.Name);
	}

	public void stopDemo()
	{
		if(myPlayer != null)
			myPlayer.stopDemo();
	}

	//Plays a demo and returns to main menu
	public void PlayDemo(Demo demo)
	{
        currentDemo = demo;

        //Reload level if in wrong mode/level (this function will be called again)
        if(currentDemo.getLevelName() != SceneManager.GetActiveScene().name || loadMode != LevelLoadMode.demo)
        {
            loadMode = LevelLoadMode.demo;
            loadLevel(currentDemo.getLevelName());
            return;
        }

        myDemoPlayer.playDemo(currentDemo, delegate { loadMode = LevelLoadMode.play; loadLevel("MainMenu"); });
	}

    //Plays the current demo after a race is finished
	private void PlayRaceDemo()
	{
        if(myDemoPlayer != null && currentDemo != null)
            myDemoPlayer.playDemo(currentDemo, delegate { menuLocked = false; SetMenuState(MenuState.endlevel); }, true);
	}

	public decimal getLastTime()
	{
		return lastTime;
	}

	//Save demo to ".vdem" file, does not work in web player
	public void saveLastDemo()
	{
        if (currentDemo != null)
		    currentDemo.saveToFile(Application.dataPath);
	}

	//Can the player move the camera with the mouse
	//Can be blocked by lockMouseView
	public void setMouseView(bool value)
	{
		if(!viewLocked)
		{
			if(myPlayer != null)
			{
				myPlayer.setMouseView(value);
			}
		}
	}

	//MouseLook is locked to given value, even if menu states change
	//Overrides old locked value
	public void lockMouseView(bool value)
	{
		if(myPlayer != null)
		{
			myPlayer.setMouseView(value);
		}
		viewLocked = true;
	}

	//MouseLook can be changed by menu again
	public void unlockMouseView()
	{
		viewLocked = false;
	}

	//MenuState can not be changed
	public void lockMenu()
	{
		menuLocked = true;
	}

	public void unlockMenu()
	{
		menuLocked = false;
	}
	
	//Map selection in main menu
	public void setSelectedMap(string map, string author = "?")
	{
		selectedMap = map;
		selectedAuthor = author;
	}

	public string getSelectedMap()
	{
		return selectedMap;
	}

	public string getSelectedAuthor()
	{
		return selectedAuthor;
	}

	//Send a leaderboard entry to leaderboard server, with a automatically generated hash.
	//This includes a secret key that will be included in the final game (and not uploaded to github),
	//so nobody can send fake entries.
	private void sendLeaderboardEntry(decimal time, string map, Demo demo)
	{
		invalidRunCheck();
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

        Api.Leaderboard.SendEntry(currentSave.Account.Name, time, map, currentSave.Account.Token, demo);
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
	public void setGravity(float value)
	{
		Physics.gravity = new Vector3(0f, value, 0f);
		invalidateRun("Changed gravity");
	}

	//Run will not be uploaded to leaderboards
	public void invalidateRun(string message = "undefined")
	{
		runValid = false;
        print("Run was invalidated. Reason: " + message);
	}

    public bool IsRunValid()
    {
        return runValid;
    }

	private void invalidRunCheck()
	{
		if(getPlayerInfo().getCheats() || Physics.gravity != defGravity)
			invalidateRun("Cheat check returned positive");
	}

	private void resetRun()
	{
		if(getPlayerInfo().getCheats())
		{
			runValid = false;
		}
		else
		{
			runValid = true;
			invalidRunCheck();
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

class GameInfoFX
{
    public delegate void Callback();
    private delegate void EffectUpdate(Effect effect);

    private Image effectImage;

    private List<Effect> activeEffects;

    struct Effect
    {
        public float startTime;
        public float duration;
        public EffectUpdate update;
        public Callback callback;
        public Color startColor;
        public Color endColor;
    }

    public GameInfoFX(Image image)
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