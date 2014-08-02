using UnityEngine;
using System.Collections;

public class Console : MonoBehaviour
{
	public bool drawConsole = false;
	public Rect consolePos;

	private string consoleText = "Welcome to Velocity!";
	private string commandLine = "";

	void Update()
	{
		if(Input.GetButtonDown("Console"))
		{
			drawConsole = !drawConsole;
		}
	}

	void OnGUI()
	{
		if(drawConsole)
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
		switch(command.ToLower())
		{
			case "quit":
				Application.Quit();
				break;
			default:
				writeToConsole("'" + command + "' is not a valid command!");
				break;
		}
	}

	public void writeToConsole(string content)
	{
		consoleText += "\n" + content; 
	}
}
