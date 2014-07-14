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
		settings = 3
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
		updateFov();
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
			int height = 10 + windowLines.Count * 20;
			int width = 150;
			GUI.Box(new Rect(5,5,width,height), "", skin.box);
			
			int lineHeight = 20;
			int lineWidth = 135;
			
			for(int i = 0; i < windowLines.Count; i++)
			{
				GUI.Label(new Rect(10,10 + 20 * i,lineWidth,lineHeight),linePrefixes[i] + windowLines[i](),skin.label);
			}
		}
		
		if(gamePaused)
		{
			//Display pause info
			string menuText = "Game Paused!";
			drawTextBox(0f,-0.6f,menuText);
		}
		
		if(showEscMenu)
		{
			List<ButtonInfo> escButtons = new List<ButtonInfo>()
			{
				new ButtonInfo("Continue", skin),
				new ButtonInfo("Help", skin),
				new ButtonInfo("Settings", skin),
				new ButtonInfo("Quit", skin)
			};

			//Draw menu buttons
			switch(drawButtonGroup(escButtons, 5, 0f, 0f))
			{
				case "Continue":
					setMenuState(MenuState.closed);
					break;
				case "Help":
					setMenuState(MenuState.intro);
					break;
				case "Settings":
					setMenuState(MenuState.settings);
					break;
				case "Quit":
					Application.Quit();
					break;
				default:
					break;
			}
		}
		
		if(showIntro)
		{
			string infoText = "Press ESC to toggle the menu.\nPress F8 to toggle debug info.\nPress (or hold) space to jump.\nPress E to grab.\nPress R to respawn.\nPress F1 to reset.";
			drawTextBox(0f,0f,infoText);
		}

		if(showSettings)
		{
			fov = drawHorizontalSlider(0f, -0.1f, 100, 20, 60f, 120f, fov, "FOV: ");
			fov = Mathf.RoundToInt(fov);
			updateFov();

			mouseSpeed = drawHorizontalSlider(0f, 0f, 100, 20, 0.5f, 20f, mouseSpeed, "Mouse Speed: ");

			List<ButtonInfo> settingsButtons = new List<ButtonInfo>()
			{
				new ButtonInfo("OK", skin),
				new ButtonInfo("Cancel", skin)
			};

			switch(drawButtonGroup(settingsButtons, 5, 0f, 0.1f))
			{
				case "OK":
					playerObj.GetComponentInChildren<MouseLook>().sensitivityX = mouseSpeed;
					playerObj.GetComponentInChildren<MouseLook>().sensitivityY = mouseSpeed;
					setMenuState(MenuState.escmenu);
					break;
				case "Cancel":
					mouseSpeed = playerObj.GetComponentInChildren<MouseLook>().sensitivityX;
					setMenuState(MenuState.escmenu);
					break;
				default:
					break;
			}
		}
	}

	void OnApplicationFocus(bool focusStatus)
	{
		if(!showEscMenu && focusStatus)
		{
			Screen.lockCursor = true;
		}
	}

	private void updateFov()
	{
		foreach(Camera cam in Camera.allCameras)
		{
			cam.fieldOfView = fov;
		}
	}

	private void setMenuState(MenuState state)
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
	
	public void drawTextBox(float virtualX, float virtualY, string text)
	{
		Rect pos = getVirtualContentPos(virtualX, virtualY, text);
		
		GUI.Box(new Rect(pos.x - 5, pos.y - 5, pos.width + 10, pos.height + 10),"");
		GUI.Label(pos, text, skin.label);
	}
	
	public bool drawButton(float virtualX, float virtualY, string text)
	{
		Rect pos = getVirtualContentPos(virtualX, virtualY, text);
		Rect extendedPos = new Rect(pos.x - 10, pos.y - 5, pos.width + 20, pos.height + 10);
		return GUI.Button(extendedPos, text, skin.button);
	}

	public float drawHorizontalSlider(float virtualX, float virtualY, int realWidth, int realHeight, float min, float max, float value, string description)
	{
		Rect descriptionPos = getVirtualContentPos(virtualX, virtualY, description);

		float valueWidth = skin.label.CalcSize(new GUIContent(value.ToString())).x;
		float valueHeight = skin.label.CalcSize(new GUIContent(value.ToString())).y;
		float valueX = descriptionPos.x + descriptionPos.width + 5 + realWidth + 5;
		float valueY = descriptionPos.y;
		Rect valuePos = new Rect(valueX, valueY, valueWidth, valueHeight);

		float boxWidth = 5 + descriptionPos.width + 5 + realWidth + 5 + valueWidth + 5;
		float boxHeight = Mathf.Max(descriptionPos.height + 10, realHeight);
		Rect boxPos = new Rect(descriptionPos.x - 5, descriptionPos.y - 5, boxWidth, boxHeight);

		float sliderX = descriptionPos.x + descriptionPos.width + 5;
		float sliderY = boxPos.y + 5;
		Rect sliderPos = new Rect(sliderX, sliderY, realWidth, realHeight);

		GUI.Box(boxPos, "");
		GUI.Label(descriptionPos, description, skin.label);
		GUI.Label(valuePos, value.ToString(), skin.label);
		return GUI.HorizontalSlider(sliderPos, value, min, max);
	}

	//Draws a list of buttons and returns the name of the pressed one (null if no button is pressed)
	private string drawButtonGroup(List<ButtonInfo> buttons, int margin, float virtualX, float virtualY)
	{
		float width = 0;
		float height = (buttons.Count - 1) * margin;

		//Get widest button and combined height
		foreach(ButtonInfo button in buttons)
		{
			if(button.getWidth() > width) { width = button.getWidth(); }
			height += button.getHeight();
		}

		//Topmost y position, virtual position is applied here
		float startPosY = ((Screen.height / 2f) - (height / 2f)) + (Screen.width / 2f) * virtualY;

		//Draw the box in the background
		float boxX = (Screen.width / 2f) - (width / 2f);
		Rect boxPos = new Rect(boxX - margin, startPosY - margin, width + 2f * margin, height + 2f * margin);
		GUI.Box(boxPos, "", skin.box);

		foreach(ButtonInfo button in buttons)
		{
			float xPos = (Screen.width / 2f) - (width / 2f);
			Rect pos = new Rect(xPos, startPosY, width, button.getHeight());
			if(GUI.Button(pos, button.getText(), button.getSkin()))
			{
				return button.getText();
			}

			startPosY += button.getHeight() + margin;
		}

		return null;
	}
	
	//virtual position: 0,0 = center of screen; 1,1 = bottom right corner
	private Rect getVirtualContentPos(float virtualX, float virtualY, string text)
	{
		GUIContent content = new GUIContent(text);
		float textWidth = skin.label.CalcSize(content).x;
		float textHeight = skin.label.CalcSize(content).y;
		
		float xPos = Screen.width / 2f + (Screen.width / 2f) * virtualX;
		float yPos = Screen.height / 2f + (Screen.height / 2f) * virtualY;
		
		return new Rect(xPos - textWidth / 2f, yPos - textHeight / 2f, textWidth, textHeight);
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
			Camera.main.GetComponent<MouseLook>().enabled = false;
			Time.timeScale = 0f;
		}
		else
		{
			Camera.main.GetComponent<MouseLook>().enabled = true;
			Time.timeScale = 1f;
		}
	}

	public void savePlayerSettings()
	{

	}

	public void loadPlayerSettings()
	{

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
		recorder.PlayDemo(recorder.getDemo());
	}
}
