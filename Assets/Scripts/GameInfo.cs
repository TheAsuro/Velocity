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
	
	//Respawn with r
	private Respawn currentSpawn = null;

	public enum MenuState
	{
		closed = 0,
		escmenu = 1,
		intro = 2,
		settings = 3
	}
	
	void Awake()
	{
		//TODO make this a proper singleton thingy stuff
		info = this;
		Screen.lockCursor = true;
		setMenuState(MenuState.intro);
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
			//Draw menu buttons
			if(drawButton(0f,-0.25f,"Help"))
			{
				setMenuState(MenuState.intro);
			}
			if(drawButton(0f,0f,"Settings"))
			{
				setMenuState(MenuState.settings);
			}
			if(drawButton(0f,0.25f,"Quit"))
			{
				Application.Quit();
			}
		}
		
		if(showIntro)
		{
			string infoText = "Press ESC to toggle the menu.\nPress F8 to toggle debug info.\nPress (or hold) space to jump.\nPress E to grab.\nPress R to respawn.";
			drawTextBox(0f,0f,infoText);
		}

		if(showSettings)
		{
			
		}
	}

	void OnApplicationFocus(bool focusStatus)
	{
		if(!showEscMenu && focusStatus)
		{
			Screen.lockCursor = true;
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
	
	//virtual position: 0,0 = center of screen
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
	
	public void setSpawn(Respawn spawn)
	{
		currentSpawn = spawn;
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
	
	public bool getGamePaused()
	{
		return gamePaused;
	}
	
	public Respawn getCurrentSpawn()
	{
		return currentSpawn;
	}
}
