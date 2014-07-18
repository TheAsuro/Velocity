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

	public enum MenuState
	{
		closed = 0,
		escmenu = 1,
		intro = 2,
		settings = 3,
		demo = 4
	}

	public class ButtonInfo
	{
		private string text;
		private GUISkin skin;
		private float width;
		private float height;

		public ButtonInfo(string pText, GUISkin pSkin)
		{
			text = pText;
			skin = pSkin;

			GUIContent content = new GUIContent(text);
			width = skin.button.CalcSize(content).x;
			height = skin.button.CalcSize(content).y;
		}

		public float getWidth()
		{
			return width;
		}

		public float getHeight()
		{
			return height;
		}

		public string getText()
		{
			return text;
		}

		public GUIStyle getSkin()
		{
			return skin.button;
		}
	}
	
	void Awake()
	{
		//TODO make this a proper singleton thingy stuff
		info = this;
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
			GUILayout.BeginVertical();

			for(int i = 0; i < windowLines.Count; i++)
			{
				GUILayout.Label(linePrefixes[i] + windowLines[i](), skin.label);
			}

			GUILayout.EndVertical();
		}
		
		if(gamePaused)
		{
			//Display pause info
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 50f, Screen.height / 6f, 100f, 30f));
			GUILayout.Box("Paused", skin.box);
			GUILayout.EndArea();
		}
		
		if(showEscMenu)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 50f, Screen.height / 2f - 50f, 100f, 100f));

			if(GUILayout.Button("Continue", skin.button)) { setMenuState(MenuState.closed); }
			if(GUILayout.Button("Help", skin.button)) { setMenuState(MenuState.intro); }
			if(GUILayout.Button("Settings", skin.button)) { setMenuState(MenuState.settings); }
			if(GUILayout.Button("Quit", skin.button)) { Application.Quit(); }

			GUILayout.EndArea();
		}
		
		if(showIntro)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 100f, Screen.height / 2f - 50f, 200f, 100f));

			string infoText = "Press ESC to toggle the menu.\nPress F8 to toggle debug info.\nPress (or hold) space to jump.\nPress E to grab.\nPress R to respawn.\nPress F1 to reset.";
			GUILayout.Box(infoText, skin.box);

			GUILayout.EndArea();
		}

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

	void OnApplicationFocus(bool focusStatus)
	{
		if(!showEscMenu && focusStatus)
		{
			Screen.lockCursor = true;
		}
	}

	public void setMenuState(MenuState state)
	{
		//Reset all states
		setGamePaused(true);
		showIntro = false;
		showEscMenu = false;
		showSettings = false;
		Screen.lockCursor = false;

		switch(state)
		{
			case MenuState.escmenu:
				showEscMenu = true;
				break;
			case MenuState.intro:
				showIntro = true;
				break;
			case MenuState.settings:
				showSettings = true;
				break;
			default:
				setGamePaused(false);
				Screen.lockCursor = true;
				break;
		}

		menuState = state;
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

	private void applySettings()
	{
		playerObj.GetComponentInChildren<MouseLook>().sensitivityX = mouseSpeed;
		playerObj.GetComponentInChildren<MouseLook>().sensitivityY = mouseSpeed;
		
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
	}

	public GameObject getPlayerObject()
	{
		return playerObj;
	}

	public void StartDemo()
	{
		recorder.StartDemo("sweg");
	}

	public void StopDemo()
	{
		recorder.StopDemo();
	}

	public void PlayLastDemo()
	{
		//TODO make this properly
		playerObj.collider.enabled = false;
		recorder.getDemo().saveToFile(Application.dataPath);
		recorder.PlayDemo(recorder.getDemo());
	}

	public void setMouseView(bool value)
	{
		if(!viewLocked)
		{
			Camera.main.GetComponent<MouseLook>().enabled = value;
		}
	}

	public void lockMouseView(bool value)
	{
		Camera.main.GetComponent<MouseLook>().enabled = value;
		viewLocked = true;
	}

	public void unlockMouseView()
	{
		viewLocked = false;
	}

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
