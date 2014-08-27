using UnityEngine;
using System.Collections;

public class Console : MonoBehaviour
{
	public bool drawConsole = false;
	public Rect consolePos;
	public TextAsset helpFile;

	private Vector2 scrollPos = Vector2.zero;
	private GUISkin skin;
	private string consoleText = "Welcome to Velocity!";
	private string commandLine = "";

	void Start()
	{
		GameInfo.info.setConsole(this);
		skin = GameInfo.info.skin;
	}

	void Update()
	{
		if(Input.GetButtonDown("Console"))
		{
			drawConsole = !drawConsole;
		}
	}

	void OnGUI()
	{
		if(drawConsole && GameInfo.info.getMenuState() != GameInfo.MenuState.closed)
		{
			consolePos = GUILayout.Window(0, consolePos, DrawConsoleGUI, "Console", skin.window);
		}
	}

	void DrawConsoleGUI(int id)
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, skin.horizontalScrollbar, skin.verticalScrollbar, skin.box);
		GUILayout.TextArea(consoleText, skin.textArea, GUILayout.ExpandHeight(true));
		GUILayout.EndScrollView();
		GUI.SetNextControlName("cmd");
		commandLine = GUILayout.TextField(commandLine, skin.textField);

		UnityEngine.Event e = UnityEngine.Event.current;
        if(e.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl().Equals("cmd") && !commandLine.Equals(""))
        {
        	writeToConsole(">" + commandLine);
			executeCommand(commandLine);
			commandLine = "";
        }

		GUI.DragWindow();
	}

	private void executeCommand(string command)
	{
		if(!command.Equals(""))
		{
			string[] commandParts = command.Trim().Split(' ');

			switch(commandParts[0].ToLower())
			{
				case "help": //Print helpful information
					writeToConsole(helpFile.text);
					break;
				case "quit": //Quit the game
					GameInfo.info.quit();
					break;
				case "forcequit":
					forceQuitCommand(commandParts);
					break;
				case "connect":
					connectCommand(commandParts);
					break;
				case "newserver":
					newServerCommand(commandParts);
					break;
				case "disconnect":
					disconnectCommand(commandParts);
					break;
				case "stopserver":
					stopServerCommand(commandParts);
					break;
				case "playdemo":
					playDemoCommand(commandParts);
					break;
				case "move_friction":
					frictionCommand(commandParts);
					break;
				case "move_speed":
					speedCommand(commandParts);
					break;
				case "move_maxspeed":
					maxSpeedCommand(commandParts);
					break;
				case "move_airspeed":
					airSpeedCommand(commandParts);
					break;
				case "move_jumpheight":
					jumpHeightCommand(commandParts);
					break;
				case "move_gravity":
					gravityCommand(commandParts);
					break;
				default:
					writeToConsole("'" + command + "' is not a valid command!");
					break;
			}
		}
	}

	//Quits the game no matter what
	private void forceQuitCommand(string[] input)
	{
		Application.Quit();
	}

	//Connect to a server
	private void connectCommand(string[] input)
	{
		if(input.Length == 3)
		{
			GameInfo.info.connectToServer(input[1], int.Parse(input[2]), "");
		}
		else if(input.Length == 4)
		{
			GameInfo.info.connectToServer(input[1], int.Parse(input[2]), input[3]);
		}
		else
		{
			writeToConsole("Usage: connect <ip> <port> (password)");
		}
	}

	//Leave the current server
	private void disconnectCommand(string[] input)
	{
		GameInfo.info.disconnectFromServer();
	}

	//Create a new multiplayer server
	private void newServerCommand(string[] input)
	{
		if(input.Length == 1)
		{
			GameInfo.info.startServer("");
		}
		else if(input.Length == 2)
		{
			GameInfo.info.startServer(input[1]);
		}
		else
		{
			writeToConsole("Usage: newserver (password)");
		}
	}

	//Stops the current server
	private void stopServerCommand(string[] input)
	{
		GameInfo.info.stopServer();
	}

	//Play a demo from a file
	private void playDemoCommand(string[] input)
	{
		if(input.Length == 2)
		{
			GameInfo.info.playDemoFromFile(input[1]);
		}
		else
		{
			writeToConsole("Usage: playdemo <demo name>");
		}
	}

	private void frictionCommand(string[] input)
	{
		if(Movement.movement == null)
		{
			writeToConsole("No movement loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current friction multiplier: " + Movement.movement.frictionMultiplier);
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				Movement.movement.frictionMultiplier = newVal;
			}
		}
		else
		{
			writeToConsole("Usage: move_friction (new friction multiplier)");
		}
	}

	private void speedCommand(string[] input)
	{
		if(Movement.movement.Equals(null))
		{
			writeToConsole("No movement loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current input multiplier: " + Movement.movement.speed);
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				Movement.movement.speed = newVal;
			}
		}
		else
		{
			writeToConsole("Usage: move_speed (new input speed)");
		}
	}

	private void maxSpeedCommand(string[] input)
	{
		if(Movement.movement.Equals(null))
		{
			writeToConsole("No movement loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current speed limit: " + Movement.movement.maxSpeed);
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				if(newVal == 0f)
				{
					writeToConsole("Value can not be 0!");
					return;
				}
				Movement.movement.maxSpeed = newVal;
			}
		}
		else
		{
			writeToConsole("Usage: move_maxspeed (new max speed)");
		}
	}

	private void airSpeedCommand(string[] input)
	{
		if(Movement.movement.Equals(null))
		{
			writeToConsole("No movement loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current speed limit: " + Movement.movement.maxSpeed);
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				Movement.movement.airSpeed = newVal;
			}
		}
		else
		{
			writeToConsole("Usage: move_airspeed (new air speed)");
		}
	}

	private void jumpHeightCommand(string[] input)
	{
		if(Movement.movement.Equals(null))
		{
			writeToConsole("No movement loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current jump height: " + Movement.movement.jumpForce);
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				Movement.movement.jumpForce = newVal;
			}
		}
		else
		{
			writeToConsole("Usage: move_jumpheight (new jump height)");
		}
	}

	private void gravityCommand(string[] input)
	{
		if(input.Length == 1)
		{
			writeToConsole("Current gravity: " + Physics.gravity.y);
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				Physics.gravity = new Vector3(0f, newVal, 0f);
			}
		}
		else
		{
			writeToConsole("Usage: move_gravity (new gravity)");
		}
	}

	public void writeToConsole(string content)
	{
		consoleText += "\n" + content; 
	}
}
