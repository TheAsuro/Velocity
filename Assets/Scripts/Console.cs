using UnityEngine;
using System.Collections;

public class Console : MonoBehaviour
{
	public bool drawConsole = false;
	public Rect consolePos;

	private string consoleText = "Welcome to Velocity!";
	private string commandLine = "";

	void Start()
	{
		GameInfo.info.setConsole(this);
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
			consolePos = GUILayout.Window(0, consolePos, DrawConsoleGUI, "Console", GameInfo.info.skin.window);
		}
	}

	void DrawConsoleGUI(int id)
	{
		GUILayout.TextArea(consoleText, GUILayout.ExpandHeight(true));
		GUI.SetNextControlName("cmd");
		commandLine = GUILayout.TextField(commandLine);

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
				case "quit": //Quit the game
					Application.Quit();
					break;
				case "connect": //Connect to a server
					if(commandParts.Length == 3)
					{
						GameInfo.info.connectToServer(commandParts[1], int.Parse(commandParts[2]), "");
					}
					else if(commandParts.Length == 4)
					{
						GameInfo.info.connectToServer(commandParts[1], int.Parse(commandParts[2]), commandParts[3]);
					}
					else
					{
						writeToConsole("Usage: connect <ip> <port> <optional password>");
					}
					break;
				case "newserver": //Create a new multiplayer server
					if(commandParts.Length == 1)
					{
						GameInfo.info.startServer("");
					}
					else if(commandParts.Length == 2)
					{
						GameInfo.info.startServer(commandParts[1]);
					}
					else
					{
						writeToConsole("Usage: newserver <optional password>");
					}
					break;
				case "disconnect": //Leave the current server
					GameInfo.info.disconnectFromServer();
					break;
				case "stopserver": //Stops the current server
					GameInfo.info.stopServer();
					break;
				default:
					writeToConsole("'" + command + "' is not a valid command!");
					break;
			}
		}
	}

	public void writeToConsole(string content)
	{
		consoleText += "\n" + content; 
	}
}
