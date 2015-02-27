using UnityEngine;
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
	private Leaderboard myLeaderboard;
	private string selectedMap;
	private string selectedAuthor = "?";

	//Sound
	public List<string> soundNames;
	public List<AudioClip> soundClips;

	//Stuff
	private SaveData currentSave;
	private Demo lastDemo;
	private long lastTime = -1L;
	private static Vector3 defGravity = new Vector3(0f, -15f, 0f);
	private bool runValid = false;

    //Server
    private ServerConnection serverConnection;
    private GameServer server;
	
	//Debug window (top-left corner, toggle with f8)
	public bool logToConsole = true;
	private float lastFps = 0f;
	private float lastFpsRecordTime = -1f;
	private List<string> linePrefixes = new List<string>();
	private List<InfoString> windowLines = new List<InfoString>();

	//Game settings
	public float mouseSpeed = 1f;
	public bool invertYInput = false;
	public float fov = 90f;
	public bool showHelp = true;
	public float volume = 0.5f;
	public bool anisotropicFiltering = false;
	public float antiAliasing = 0f;
	public float textureSize = 0f;
	public float lightingLevel = 0f;
	public float vsyncLevel = 0f;

	//GUI settings
	public float circleSpeed1 = 10f;
	public float circleSpeed2 = 20f;
	public float circleSpeed3 = 30f;

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
		myLeaderboardObj = myCanvas.transform.Find("Leaderboard").gameObject;
		myLeaderboard = myCanvas.GetComponent<Leaderboard>();
		Screen.lockCursor = true;
		setMenuState(MenuState.closed);
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
	}

	//Lock cursor after game window lost and gained focus
	void OnApplicationFocus(bool focusStatus)
	{
		if(getMenuState() == MenuState.closed && focusStatus)
		{
			Screen.lockCursor = true;
		}
	}

	//Prepare for new level
	void OnLevelWasLoaded(int level)
	{
		removeAllWindowLines();
		loadPlayerSettings();
		menuLocked = false;
		WorldInfo wInfo = WorldInfo.info;
		if(wInfo != null)
		{
			setMenuState(wInfo.beginState);
		}
		else
		{
			setMenuState(MenuState.inactive);
		}
	}

	//Load a level, but inform other players if this is a server
	public void loadLevel(string name)
	{
		Application.LoadLevel(name);
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
		getCurrentSave().saveIfPersonalBest(time.Ticks, Application.loadedLevelName);
		lastTime = time.Ticks;
	}

	//Player hit the exit trigger
	public void levelFinished()
	{
		GameInfo.info.setMenuState(GameInfo.MenuState.endlevel);
		lastDemo = myPlayer.getDemo();

		//If we are in editor, stop the test run
		if(myPlayer.editorMode)
		{
			EditorInfo.info.EndTest();
		}

		//If a player save is loaded, play demo and send to leaderboard
		if(getCurrentSave() != null)
		{
			sendLeaderboardEntry(getCurrentSave().getPlayerName(), lastTime, Application.loadedLevelName);
			playLastDemo();
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
		setMenuState(MenuState.closed);
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
	public void setMenuState(string state)
	{
		switch(state)
		{
			case "closed":
				setMenuState(MenuState.closed);
				break;
			case "escmenu":
				setMenuState(MenuState.escmenu);
				break;
			case "inactive":
				setMenuState(MenuState.inactive);
				break;
			case "demo":
				setMenuState(MenuState.demo);
				break;
			case "leaderboard":
				setMenuState(MenuState.leaderboard);
				break;
			case "endlevel":
				setMenuState(MenuState.endlevel);
				break;
			case "othermenu":
				setMenuState(MenuState.othermenu);
				break;
			case "editor":
				setMenuState(MenuState.editor);
				break;
			case "editorplay":
				setMenuState(MenuState.editorplay);
				break;
		}
	}

	//Menu state manager
	public void setMenuState(MenuState state)
	{
		if(!menuLocked)
		{
			//Reset all states
			setGamePaused(true);
			escMenu.SetActive(false);
			endLevel.SetActive(false);
			myLeaderboardObj.SetActive(false);
			Screen.lockCursor = false;

			switch(state)
			{
				case MenuState.closed:
					setGamePaused(false);
					Screen.lockCursor = true;
					break;
				case MenuState.escmenu:
					escMenu.SetActive(true);
					break;
				case MenuState.inactive:
					setGamePaused(false);
					break;
				case MenuState.demo:
					setGamePaused(false);
					setMouseView(false);
					Screen.lockCursor = true;
					break;
				case MenuState.leaderboard:
					setMouseView(false);
					endLevel.SetActive(true);
					myLeaderboardObj.SetActive(true);
					myLeaderboard.getLeaderboardEntries(Application.loadedLevelName);
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
					Screen.lockCursor = true;
					break;
			}

			menuState = state;
		}
	}

	private void toggleEscMenu()
	{
		if(menuState == MenuState.closed)
		{
			setMenuState(MenuState.escmenu);
		}
		else
		{
			setMenuState(MenuState.closed);
		}
	}

	public void toggleLeaderboard()
	{
		if(myLeaderboardObj.activeSelf)
		{
			setMenuState(MenuState.endlevel);
			return;
		}
		setMenuState(MenuState.leaderboard);
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
			myPlayer.invertYInput = invertYInput;
			myPlayer.setFov(fov);
			myPlayer.setVolume(volume);
		}

		AnisotropicFiltering filter = AnisotropicFiltering.Disable;
		if(anisotropicFiltering) { filter = AnisotropicFiltering.ForceEnable; }

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
		if(anisotropicFiltering) { anisoValue = 1f; }

		float invertValue = 0f;
		if(invertYInput) { invertValue = 1f; }

		PlayerPrefs.SetFloat("fov", fov);
		PlayerPrefs.SetFloat("mouseSpeed", mouseSpeed);
		PlayerPrefs.SetFloat("invertY", invertValue);
		PlayerPrefs.SetFloat("volume", volume);
		PlayerPrefs.SetFloat("aniso", anisoValue);
		PlayerPrefs.SetFloat("aa", antiAliasing);
		PlayerPrefs.SetFloat("textureSize", textureSize);
		PlayerPrefs.SetFloat("lighting", lightingLevel);
		PlayerPrefs.SetFloat("vsync", vsyncLevel);

		applySettings();
	}

	//Load game settings from playerprefs, but don't apply them yet
	public void loadPlayerSettings()
	{
		fov = PlayerPrefs.GetFloat("fov");
		mouseSpeed = PlayerPrefs.GetFloat("mouseSpeed");
		invertYInput = (PlayerPrefs.GetFloat("invertY") == 1f);
		volume = PlayerPrefs.GetFloat("volume");
		anisotropicFiltering = (PlayerPrefs.GetFloat("aniso") == 1f);
		antiAliasing = PlayerPrefs.GetFloat("aa");
		textureSize = PlayerPrefs.GetFloat("textureSize");
		lightingLevel = PlayerPrefs.GetFloat("lighting");
		vsyncLevel = PlayerPrefs.GetFloat("vsync");

		if(fov == 0f) { fov = 60f; }
		if(mouseSpeed == 0f) { mouseSpeed = 1f; }

		applySettings();
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

	//Plays a demo from a ".vdem" file, does not work in web player
	public void playDemoFromFile(string fileName)
	{
		#if UNITY_STANDALONE

		stopDemo();

		string fixedFileName = fileName;
		if(!fixedFileName.ToLower().EndsWith(".vdem")) { fixedFileName += ".vdem"; }
		
		Demo myDemo = new Demo(Application.dataPath + "/" + fixedFileName);
		if(!myDemo.didLoadFromFileFail())
		{
			myDemoPlayer.playDemo(myDemo, consoleDemoPlayEnded);
		}

		#endif
	}

	public void playLastDemo()
	{
		myDemoPlayer.playDemo(lastDemo, endLeveldemoPlayEnded);
	}

	public double getLastTime()
	{
		return lastTime;
	}

	//Will be called after demo finished (demo started from endlevel menu)
	private void endLeveldemoPlayEnded()
	{
		setMenuState(MenuState.endlevel);
	}

	//Will be called after demo finished (demo started from console)
	private void consoleDemoPlayEnded()
	{
		setMenuState(MenuState.escmenu);
	}

	//Save demo to ".vdem" file, does not work in web player
	public void saveLastDemo()
	{
		#if UNITY_STANDALONE

		lastDemo.saveToFile(Application.dataPath);

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
	private void sendLeaderboardEntry(string name, long time, string map)
	{
		invalidRunCheck();
		if(runValid)
		{
			WWWForm form = new WWWForm();
			string hash = Md5Sum(name + time.ToString() + map + secretKey);

			form.AddField("Player", name);
			form.AddField("Time", time.ToString());
			form.AddField("Map", map);
			form.AddField("Hash", hash);

			WWW www = new WWW("http://theasuro.de/Velocity/newentry.php", form);
			StartCoroutine(myLeaderboard.SendLeaderboardData(www));
		}
		else
		{
			print("Invalid run!");
		}
	}

	public void loadMapRecord(string map, Leaderboard.processString proc)
	{
		myLeaderboard.getMapRecord(map, proc);
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
		invalidateRun();
	}

	//Run will not be uploaded to leaderboards
	public void invalidateRun()
	{
		runValid = false;
	}

	private void invalidRunCheck()
	{
		if(getPlayerInfo().getCheats() || Physics.gravity != defGravity)
			invalidateRun();
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
}