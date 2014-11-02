using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInfo : MonoBehaviour
{
	public static GameInfo info;
	public delegate string InfoString();
	public GameObject playerTemplate;
	public GUISkin skin;
	public string secretKey = "NotActuallySecret";
	
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
	private float lastTime = -1f;
	
	//Debug window (top-left corner, toggle with f8)
	public bool logToConsole = true;
	private float lastFps = 0f;
	private float lastFpsRecordTime = -1f;
	private List<string> linePrefixes = new List<string>();
	private List<InfoString> windowLines = new List<InfoString>();

	//Game settings
	public float mouseSpeed = 1f;
	public float fov = 90f;
	public bool showHelp = true;
	public float volume = 0.5f;
	public bool anisotropicFiltering = false;
	public float antiAliasing = 0f;
	public float textureSize = 0f;
	public float lightingLevel = 0f;
	public float vsyncLevel = 0f;

	//References
	private PlayerInfo myPlayer;
	private DemoPlay myDemoPlayer;
	private Console myConsole;
	private GameObject myCanvas;
	private GameObject myConsoleWindow;
	private Server myServer;
	private Client myClient;
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
		othermenu
	}

	public enum GameMode
	{
		mainmenu = 1,
		mainmenulobby = 2,
		singleplayer = 3,
		maplobby = 4,
		timeattack = 5
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

		myServer = gameObject.GetComponent<Server>();
		myClient = gameObject.GetComponent<Client>();
		myDemoPlayer = gameObject.GetComponent<DemoPlay>();

		myCanvas = transform.Find("Canvas").gameObject;
		escMenu = myCanvas.transform.Find("EscMenu").gameObject;
		endLevel = myCanvas.transform.Find("EndLevel").gameObject;
		myConsoleWindow = myCanvas.transform.Find("Console").gameObject;
		myDebugWindow = myCanvas.transform.Find("Debug").gameObject;
		myDebugWindowText = myDebugWindow.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
		myLeaderboardObj = myCanvas.transform.Find("Leaderboard").gameObject;
		myLeaderboard = myLeaderboardObj.GetComponent<Leaderboard>();
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

	//Lock cursor after loosing and gaining focus
	void OnApplicationFocus(bool focusStatus)
	{
		if(getMenuState() == MenuState.closed && focusStatus)
		{
			Screen.lockCursor = true;
		}
	}

	//Set menustate according to current level's worldinfo settings
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
		if(myServer.isRunning())
		{
			//TODO
		}
		Application.LoadLevel(name);
	}

	//Creates a new local player (the one that is controlled by the current user)
	public void spawnNewPlayer(Respawn spawnpoint, bool killOldPlayer = true)
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
		
		applySettings();
	}

	//Removes player and plays back the demo
	public void levelFinished()
	{
		sendLeaderboardEntry(getCurrentSave().getPlayerName(), lastTime, Application.loadedLevelName);
		GameInfo.info.setMenuState(GameInfo.MenuState.endlevel);
		lastDemo = myPlayer.getDemo();
		setPlayerInfo(null);
		playLastDemo();
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

	//Start a new multiplayer server
	public void startServer(int port, string password, string map)
	{
		myServer.StartServer(2, port, password);
		Application.LoadLevel(map);
	}

	//Connect to a multiplayer server
	public void connectToServer(string ip, int port, string password)
	{
		if(currentSave != null)
		{
			myClient.ConnectToServer(ip, port, password);
		}
		else
		{
			writeToConsole("You can only connect with a loaded save!");
		}
	}

	//Disconnect from current server
	public void disconnectFromServer()
	{
		myClient.DisconnectFromServer();
	}

	//Stop current server
	public void stopServer()
	{
		myServer.StopServer();
	}

	//Reset everything in the world to its initial state
	public void reset()
	{
		stopDemo();
		removeAllWindowLines();
		WorldInfo.info.reset();
		setMenuState(MenuState.closed);
		startDemo();
	}

	//Leave the game
	public void quit()
	{
		Application.Quit();
	}

	public void runFinished(float time)
	{
		stopDemo();
		getCurrentSave().saveIfPersonalBest(time, Application.loadedLevelName);
		lastTime = time;
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
		}
		else
		{
			setMouseView(true);
			Time.timeScale = 1f;
		}
	}

	public void setCurrentSave(SaveData data)
	{
		currentSave = data;
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

	public void log(string text)
	{
		if(logToConsole)
		{
			writeToConsole(text);
		}
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

		PlayerPrefs.SetFloat("fov", fov);
		PlayerPrefs.SetFloat("mouseSpeed", mouseSpeed);
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
			if(myPlayer != null)
			{
				Destroy(myPlayer.gameObject);
			}
			myPlayer = null;
			return;
		}

		myPlayer = info;
	}

	public PlayerInfo getPlayerInfo()
	{
		return myPlayer;
	}

	public void startDemo()
	{
		if(myPlayer != null)
			myPlayer.startDemo(currentSave.getPlayerName());
	}

	public void stopDemo()
	{
		if(myPlayer != null)
			myPlayer.stopDemo();
	}

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

	public float getLastTime()
	{
		return lastTime;
	}

	private void endLeveldemoPlayEnded()
	{
		setMenuState(MenuState.endlevel);
	}

	private void consoleDemoPlayEnded()
	{
		setMenuState(MenuState.escmenu);
	}

	public void saveLastDemo()
	{
		#if UNITY_STANDALONE

		lastDemo.saveToFile(Application.dataPath);

		#endif
	}

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

	public void lockMenu()
	{
		menuLocked = true;
	}

	public void unlockMenu()
	{
		menuLocked = false;
	}
	
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
	private void sendLeaderboardEntry(string name, float time, string map)
	{
		WWWForm form = new WWWForm();
		string hash = Md5Sum(name + time.ToString() + map + secretKey);

		form.AddField("Player", name);
		form.AddField("Time", time.ToString());
		form.AddField("Map", map);
		form.AddField("Hash", hash);

		WWW www = new WWW("http://localhost/newentry.php", form);
		StartCoroutine(myLeaderboard.SendLeaderboardData(www));
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

	public void setGravity(float value)
	{
		Physics.gravity = new Vector3(0f, value, 0f);
	}
}