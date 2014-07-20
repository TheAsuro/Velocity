using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInfo : MonoBehaviour
{
	public static GameInfo info;
	public delegate string InfoString();
	public GUISkin skin;
	
	//Gamestates
	private bool showDebug = false;
	private bool gamePaused = false;
	private bool showIntro = false;
	private bool showEscMenu = false;
	private bool showSettings = false;
	private MenuState menuState = MenuState.closed;
	private bool viewLocked = false;
	private bool menuLocked = false;

	//Save file stuff
	private SaveData currentSave;
	
	//Debug window (top-left corner, toggle with f8)
	private List<string> linePrefixes = new List<string>();
	private List<InfoString> windowLines = new List<InfoString>();

	//Game settings
	private float mouseSpeed = 1f;
	private float fov = 90f;
	public bool showHelp = true;

	//References
	private GameObject playerObj;
	private DemoRecord recorder;
	private MouseLook mouseLook;

	public enum MenuState
	{
		closed = 0,
		escmenu = 1,
		intro = 2,
		settings = 3,
		inactive = 4,
		demo = 5
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
		
		Screen.lockCursor = true;
		setMenuState(MenuState.intro);
	}

	void Start()
	{
		loadPlayerSettings();
	}

	//Don't do movement stuff here, do it in FixedUpdate()
	void Update()
	{
		if(Input.GetButtonDown("Debug"))
		{
			showDebug = !showDebug;
		}
		
		if(Input.GetButtonDown("Menu"))
		{
			toggleEscMenu();
		}
	}
	
	//Draw the HUD
	void OnGUI()
	{
		//Debug info in the top-left corner
		if(showDebug)
		{
			Rect rect = new Rect(0f, 0f, 150f, 200f);
			GUI.Box(rect, "", skin.box);

			GUILayout.BeginArea(rect);

			for(int i = 0; i < windowLines.Count; i++)
			{
				GUILayout.Label(linePrefixes[i] + windowLines[i](), skin.label);
			}

			GUILayout.EndArea();
		}
		
		//Pause notification
		if(gamePaused)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 50f, Screen.height / 6f, 100f, 30f));
			GUILayout.Box("Paused", skin.box);
			GUILayout.EndArea();
		}
		
		//Esc Menu buttons
		if(showEscMenu)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 50f, Screen.height / 2f - 50f, 100f, 100f));

			if(GUILayout.Button("Continue", skin.button)) { setMenuState(MenuState.closed); }
			if(GUILayout.Button("Help", skin.button)) { setMenuState(MenuState.intro); }
			if(GUILayout.Button("Settings", skin.button)) { setMenuState(MenuState.settings); }
			if(GUILayout.Button("Quit", skin.button)) { Application.Quit(); }

			GUILayout.EndArea();
		}
		
		//Into text
		if(showIntro)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 100f, Screen.height / 2f - 50f, 200f, 100f));

			string infoText = "Press ESC to toggle the menu.\nPress F8 to toggle debug info.\nPress (or hold) space to jump.\nPress E to grab.\nPress R to respawn.\nPress F1 to reset.";
			GUILayout.Box(infoText, skin.box);

			GUILayout.EndArea();
		}

		//Game settings dialog
		if(showSettings)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 200f, Screen.height / 2f - 50f, 400f, 100f));

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
			GUILayout.Label("FOV", skin.label);
			GUILayout.Label("Mouse Sensitivity", skin.label);
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			fov = GUILayout.HorizontalSlider(fov, 60f, 120f);
			fov = Mathf.RoundToInt(fov);
			mouseSpeed = GUILayout.HorizontalSlider(mouseSpeed, 0.5f, 10f);
			mouseSpeed = floor(mouseSpeed, 1);
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			GUILayout.Label(fov.ToString(), skin.label);
			GUILayout.Label(mouseSpeed.ToString(), skin.label);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			if(GUILayout.Button("OK", skin.button)) { savePlayerSettings(); setMenuState(MenuState.escmenu); }
			if(GUILayout.Button("Cancel", skin.button)) { setMenuState(MenuState.escmenu); }

			GUILayout.EndArea();
		}
	}

	//Lock cursor after loosing and gaining focus
	void OnApplicationFocus(bool focusStatus)
	{
		if(!showEscMenu && focusStatus)
		{
			Screen.lockCursor = true;
		}
	}

	//Set menustate according to current level's worldinfo settings
	void OnLevelWasLoaded(int level)
	{
		loadPlayerSettings();
		setMenuLocked(false);
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

	//Menu can't be accessed until unlocked
	public void setMenuLocked(bool value)
	{
		if(value)
		{
			setMenuState(MenuState.inactive);
		}
		menuLocked = value;
	}

	//Menu state manager
	public void setMenuState(MenuState state)
	{
		if(!menuLocked)
		{
			//Reset all states
			setGamePaused(true);
			showIntro = false;
			showEscMenu = false;
			showSettings = false;
			Screen.lockCursor = false;

			switch(state)
			{
				case MenuState.closed:
					setGamePaused(false);
					Screen.lockCursor = true;
					break;
				case MenuState.escmenu:
					showEscMenu = true;
					break;
				case MenuState.intro:
					showIntro = true;
					break;
				case MenuState.settings:
					showSettings = true;
					break;
				case MenuState.inactive:
					setGamePaused(false);
					break;
				case MenuState.demo:
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

	public void loadCurrentSave()
	{
		if(currentSave != null)
		{
			string level = currentSave.getPlayerLevel();
			if(level == "") { level = "BasicTutorial"; }
			Application.LoadLevel(level);
		}
		else
		{
			print("Tried to load, but there is no current save file :o");
		}
	}

	public void save()
	{
		if(currentSave != null)
		{
			currentSave.save();
		}
		else
		{
			print("Tried to save, but there is no current save file :o");
		}
	}

	private void applySettings()
	{
		if(mouseLook != null)
		{
			mouseLook.sensitivityX = mouseSpeed;
			mouseLook.sensitivityY = mouseSpeed;
		}
		
		foreach(Camera cam in Camera.allCameras)
		{
			cam.fieldOfView = fov;
		}
	}

	public void savePlayerSettings()
	{
		PlayerPrefs.SetFloat("fov", fov);
		PlayerPrefs.SetFloat("mouseSpeed", mouseSpeed);

		applySettings();
	}

	public void loadPlayerSettings()
	{
		fov = PlayerPrefs.GetFloat("fov");
		mouseSpeed = PlayerPrefs.GetFloat("mouseSpeed");

		if(fov == 0f) { fov = 60f; }
		if(mouseSpeed == 0f) { mouseSpeed = 1f; }

		applySettings();
	}
	
	public bool getGamePaused()
	{
		return gamePaused;
	}

	public void setPlayerObject(GameObject player)
	{
		playerObj = player;
		recorder = playerObj.GetComponent<DemoRecord>();
		mouseLook = playerObj.GetComponentInChildren<MouseLook>();
	}

	public GameObject getPlayerObject()
	{
		return playerObj;
	}

	public void startDemo()
	{
		recorder.startDemo(currentSave.getPlayerName());
	}

	public void stopDemo()
	{
		recorder.stopDemo();
	}

	public void playLastDemo()
	{
		recorder.playDemo(recorder.getDemo());
	}

	public void saveLastDemo()
	{
		#if UNITY_STANDALONE_WIN
		recorder.getDemo().saveToFile(Application.dataPath);
		#endif
	}

	public void setMouseView(bool value)
	{
		if(!viewLocked)
		{
			if(mouseLook != null)
			{
				mouseLook.enabled = value;
			}
		}
	}

	//MouseLook is locked to given value, even if menu states change
	public void lockMouseView(bool value)
	{
		if(mouseLook != null)
		{
			mouseLook.enabled = value;
		}
		viewLocked = true;
	}

	//MouseLook can be changed by menu again
	public void unlockMouseView()
	{
		viewLocked = false;
	}

	//Returns rounded value of a float
	private float floor(float input, int decimalsAfterPoint)
	{
		string floatText = input.ToString();
		if(floatText.ToLower().Contains("e"))
		{
			return 0f;
		}
		else
		{
			if(floatText.Contains("."))
			{
				int decimalCount = floatText.Substring(floatText.IndexOf(".")).Length;
				if(decimalCount <= decimalsAfterPoint)
				{
					return input;
				}
				else
				{
					return float.Parse(floatText.Substring(0, floatText.IndexOf(".") + decimalsAfterPoint + 1));
				}
			}
			else
			{
				return input;
			}
		}
	}
}
