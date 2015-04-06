using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
	private SaveData currentSave;
	private Demo currentDemo;
	private decimal lastTime = -1;
    public string lastTimeString
    { get { return lastTime.ToString("0.0000"); } }
	private static Vector3 defGravity = new Vector3(0f, -15f, 0f);
	private bool runValid = false;
    public LevelLoadMode loadMode = LevelLoadMode.play;

    //Server
    private ServerConnection serverConnection;
    private GameServer server;
	
	//Debug window (top-left corner, toggle with f8)
	public bool logToConsole = true;
	private float lastFps = 0f;
	private float lastFpsRecordTime = -1f;
	private List<string> linePrefixes = new List<string>();
	private List<InfoString> windowLines = new List<InfoString>();

	//Game settings (saved as floats because PlayerPrefs can't handle bools
	public float mouseSpeed = 1f;
	public float invertYInput = 0f;
	public float fov = 90f;
	public bool showHelp = true;
	public float volume = 0.5f;
	public float anisotropicFiltering = 1f;
	public float antiAliasing = 0f;
	public float textureSize = 0f;
	public float lightingLevel = 0f;
	public float vsyncLevel = 0f;
    public float demoPerspective = 0f;
    public float rawMouse = 1f;

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
	private UnityEngine.UI.Text myDebugWindowText;

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
		myDebugWindowText = myDebugWindow.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
		myLeaderboardObj = myCanvas.transform.Find("Leaderboards").gameObject;
		SetMenuState(MenuState.closed);

        fx = new GameInfoFX(myCanvas.transform.FindChild("FxImage").GetComponent<Image>());
	}

	void Start()
	{
		loadPlayerSettings();
	}

	void Update()
	{
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
		removeAllWindowLines();
		loadPlayerSettings();

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
        fx.StartFadeToColor(new Color(0f, 0f, 0f, 0f), Color.black, 0.5f, delegate { Application.LoadLevel(name); });
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
		
		applySettings();
	}

	//Player hit the goal
	public void runFinished(TimeSpan time)
	{
		stopDemo();
		cleanUpPlayer();
        lastTime = time.Ticks / (decimal)10000000;
		getCurrentSave().saveIfPersonalBest(lastTime, Application.loadedLevelName);
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

		GameInfo.info.SetMenuState(GameInfo.MenuState.endlevel);
		currentDemo = myPlayer.getDemo();

		//If a player save is loaded, play demo and send to leaderboard
		if(getCurrentSave() != null)
		{
			sendLeaderboardEntry(getCurrentSave().getPlayerName(), lastTime, Application.loadedLevelName, currentDemo);
			PlayRaceDemo();
		}

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
                    myLeaderboardObj.GetComponent<LeaderboardDisplay>().LoadMap(Application.loadedLevelName);
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

    public void connectToServer(string ip, int port, string password = "")
    {
        if (serverConnection != null)
            serverConnection.Disconnect();

        serverConnection = new ServerConnection(ip, port, password);
        serverConnection.Connect(connected);
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

	//Load infos like player name, pb's, etc.
	public void setCurrentSave(SaveData data)
	{
		currentSave = data;
        if(data != null)
            PlayerPrefs.SetInt("lastplayer", currentSave.getIndex());
	}

	public SaveData getCurrentSave()
	{
		return currentSave;
	}

	//Apply current data to loaded save file
	public void save()
	{
		if(currentSave != null)
		{
			currentSave.save();
		}
		else
		{
			writeToConsole("Tried to save, but there is no current save file :o");
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

	//Apply loaded settings to the current game
	private void applySettings()
	{
		if(myPlayer != null)
		{
			myPlayer.setMouseSens(mouseSpeed);
			myPlayer.invertYInput = invertYInput == 1f;
			myPlayer.setFov(fov);
			myPlayer.setVolume(volume);
		}

		AnisotropicFiltering filter = AnisotropicFiltering.Disable;
		if(anisotropicFiltering == 1f) { filter = AnisotropicFiltering.ForceEnable; }

		int textureLimit = 2 - (int)textureSize;

		QualitySettings.anisotropicFiltering = filter;
		QualitySettings.antiAliasing = (int)antiAliasing;
		QualitySettings.masterTextureLimit = textureLimit;
		QualitySettings.pixelLightCount = (int)lightingLevel;
		QualitySettings.shadowCascades = (int)lightingLevel;
		QualitySettings.vSyncCount = (int)vsyncLevel;
	}

	//Save current game settings to playerprefs
	public void savePlayerSettings()
	{
		float anisoValue = 0f;
		if(anisotropicFiltering == 1f) { anisoValue = 1f; }

		float invertValue = 0f;
		if(invertYInput == 1f) { invertValue = 1f; }

		PlayerPrefs.SetFloat("fov", fov);
		PlayerPrefs.SetFloat("mouseSpeed", mouseSpeed);
		PlayerPrefs.SetFloat("invertY", invertValue);
		PlayerPrefs.SetFloat("volume", volume);
		PlayerPrefs.SetFloat("aniso", anisoValue);
		PlayerPrefs.SetFloat("aa", antiAliasing);
		PlayerPrefs.SetFloat("textureSize", textureSize);
		PlayerPrefs.SetFloat("lighting", lightingLevel);
		PlayerPrefs.SetFloat("vsync", vsyncLevel);
        PlayerPrefs.SetFloat("demoPerspective", demoPerspective);
        PlayerPrefs.SetFloat("rawmouse", rawMouse);

		applySettings();
	}

	//Load game settings from playerprefs, but don't apply them yet
	public void loadPlayerSettings()
	{
        //Check if keys are available, load defaults otherwise
        if (PlayerPrefs.HasKey("fov"))
            fov = PlayerPrefs.GetFloat("fov");
        else
            fov = 90f;

        if (PlayerPrefs.HasKey("mouseSpeed"))
            mouseSpeed = PlayerPrefs.GetFloat("mouseSpeed");
        else
            mouseSpeed = 1f;

        if (PlayerPrefs.HasKey("invertY"))
            invertYInput = PlayerPrefs.GetFloat("invertY");
        else
            invertYInput = 0f;

        if (PlayerPrefs.HasKey("volume"))
            volume = PlayerPrefs.GetFloat("volume");
        else
            volume = 0.5f;

        if (PlayerPrefs.HasKey("aniso"))
            anisotropicFiltering = PlayerPrefs.GetFloat("aniso");
        else
            anisotropicFiltering = 1f;

        if (PlayerPrefs.HasKey("aa"))
            antiAliasing = PlayerPrefs.GetFloat("aa");
        else
            antiAliasing = 2f;

        if (PlayerPrefs.HasKey("textureSize"))
            textureSize = PlayerPrefs.GetFloat("textureSize");
        else
            textureSize = 2f;

        if (PlayerPrefs.HasKey("lighting"))
            lightingLevel = PlayerPrefs.GetFloat("lighting");
        else
            lightingLevel = 4f;

        if (PlayerPrefs.HasKey("vsync"))
            vsyncLevel = PlayerPrefs.GetFloat("vsync");
        else
            vsyncLevel = 0f;

        if (PlayerPrefs.HasKey("demoPerspective"))
            demoPerspective = PlayerPrefs.GetFloat("demoPerspective");
        else
            demoPerspective = 0f;

        if (PlayerPrefs.HasKey("rawmouse"))
            rawMouse = PlayerPrefs.GetFloat("rawmouse");
        else
            rawMouse = 1f;

		applySettings();
	}

    public void DeletePlayerSettings()
    {
        PlayerPrefs.DeleteKey("fov");
        PlayerPrefs.DeleteKey("mouseSpeed");
        PlayerPrefs.DeleteKey("invertY");
        PlayerPrefs.DeleteKey("volume");
        PlayerPrefs.DeleteKey("aniso");
        PlayerPrefs.DeleteKey("aa");
        PlayerPrefs.DeleteKey("textureSize");
        PlayerPrefs.DeleteKey("lighting");
        PlayerPrefs.DeleteKey("vsync");
        PlayerPrefs.DeleteKey("rawmouse");
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
			myPlayer.startDemo(currentSave.getPlayerName());
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
        if(currentDemo.getLevelName() != Application.loadedLevelName || loadMode != LevelLoadMode.demo)
        {
            loadMode = LevelLoadMode.demo;
            loadLevel(currentDemo.getLevelName());
            return;
        }

        myDemoPlayer.playDemo(currentDemo, delegate { loadMode = LevelLoadMode.play; loadLevel("MainMenu"); });
	}

    //Plays the current demo and returns to end level screen
	public void PlayRaceDemo()
	{
        myDemoPlayer.playDemo(currentDemo, delegate { menuLocked = false; SetMenuState(MenuState.endlevel); });
	}

	public decimal getLastTime()
	{
		return lastTime;
	}

	//Save demo to ".vdem" file, does not work in web player
	public void saveLastDemo()
	{
		#if UNITY_STANDALONE

		currentDemo.saveToFile(Application.dataPath);

		#endif
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
	private void sendLeaderboardEntry(string name, decimal time, string map, Demo demo)
	{
		invalidRunCheck();
		if(runValid)
		{
			string hash = Md5Sum(name + time.ToString() + map + secretKey);
			StartCoroutine(Leaderboard.SendEntry(name, time, map, hash, demo));
		}
		else
		{
			print("Invalid run!");
		}
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