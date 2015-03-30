using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Console : MonoBehaviour
{
	public TextAsset helpFile;
	public int rowCount = 22;

	private bool visible = false;
	private GameObject myConsole;
	private Text myOutput;
	private InputField myInput;

	private Vector2 clickDelta = Vector2.zero;
	private bool mouseDown;

	void Start()
	{
		//Tell the GameInfo script that this is the console
		GameInfo.info.setConsole(this);

		//Find all parts of the console
		myConsole = gameObject.transform.Find("Console").gameObject;
		myOutput = myConsole.transform.Find("ConsoleOutput").Find("Mask").Find("Text").GetComponent<Text>();
		myInput = myConsole.transform.Find("ConsoleInput").GetComponent<InputField>();

		//Registering events
		myInput.onEndEdit.AddListener(inputSubmit);
	}

	void Update()
	{
		if(Input.GetButtonDown("Console"))
		{
			toggleVisibility();
		}

		if(Input.GetMouseButtonDown(0))
		{
			Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			Rect titleRect = GameInfo.info.getConsoleTitleRect();

			if(titleRect.x <= mousePos.x && mousePos.x <= titleRect.x + titleRect.width &&
			   titleRect.y <= mousePos.y && mousePos.y <= titleRect.y + titleRect.height)
			{
				mouseDown = true;
				RectTransform t = (RectTransform)myConsole.transform;
				
				clickDelta = mousePos - t.anchoredPosition;
			}
		}

		if(Input.GetMouseButton(0))
		{
			if(mouseDown)
			{
				RectTransform t = (RectTransform)myConsole.transform;
				Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				t.anchoredPosition = mousePos - clickDelta;
				Vector2 newPos = t.anchoredPosition;
				if(t.anchoredPosition.x < t.rect.width / 2 ) { newPos.x = t.rect.width / 2; }
				if(t.anchoredPosition.x > Screen.width - t.rect.width / 2) { newPos.x = Screen.width - t.rect.width / 2; }
				if(t.anchoredPosition.y > -t.rect.height / 2 ) { newPos.y = -t.rect.height / 2; }
				if(t.anchoredPosition.y < -Screen.height + t.rect.height / 2) { newPos.y = -Screen.height + t.rect.height / 2; }
				t.anchoredPosition = newPos;
			}
		}

		if(Input.GetMouseButtonUp(0))
		{
			mouseDown = false;
		}
	}

	private void toggleVisibility()
	{
		visible = !visible;
		myInput.OnDeselect(null);
		myConsole.SetActive(visible);
	}

	public void inputSubmit(string input)
    {
        writeToConsole(input);
		executeCommand(input);
		myInput.text = "";
	}

	public void writeToConsole(string content)
	{
		myOutput.text += "\n" + content;
	}

	public bool isVisible()
	{
		return visible;
	}

	public void executeCommand(string command)
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
				case "logtoconsole":
					logCommand(commandParts);
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
				case "move_accel":
					accelerationCommand(commandParts);
					break;
				case "move_airaccel":
					airAccelerationCommand(commandParts);
					break;
				case "move_maxspeed":
					maxSpeedCommand(commandParts);
					break;
				case "move_maxairspeed":
					maxAirSpeedCommand(commandParts);
					break;
				case "move_jumpheight":
					jumpHeightCommand(commandParts);
					break;
				case "move_gravity":
					gravityCommand(commandParts);
					break;
                case "noclip":
                    noclipCommand(commandParts);
                    break;
				case "cheats":
					cheatsCommand(commandParts);
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

	//Enables logging of certain changes
	private void logCommand(string[] input)
	{
		if(input.Length == 2)
		{
			bool value = false;
			if(input[1].Equals("true") || input[1].Equals("1")) { value = true; }
			GameInfo.info.logToConsole = value;
		}
		else
		{
			writeToConsole("Usage: logToConsole <true/false/1/0>");
		}
	}

	//Connect to a server
	private void connectCommand(string[] input)
	{
		if(input.Length == 3)
		{
			
		}
		else if(input.Length == 4)
		{
			
		}
		else
		{
			writeToConsole("Usage: connect <ip> <port> (password)");
		}
	}

	//Leave the current server
	private void disconnectCommand(string[] input)
	{
		
	}

	//Create a new multiplayer server
	private void newServerCommand(string[] input)
	{
		if(input.Length == 1)
		{
			
		}
		else if(input.Length == 2)
		{
			
		}
		else if(input.Length == 3)
		{
			
		}
		else
		{
			writeToConsole("Usage: newserver <map> (port) (password)");
		}
	}

	//Stops the current server
	private void stopServerCommand(string[] input)
	{
		
	}

	//Play a demo from a file
	private void playDemoCommand(string[] input)
	{
		if(input.Length == 2)
		{
            Demo demo = new Demo(System.IO.Path.Combine(Application.dataPath, input[1]));

            if (demo.didLoadFromFileFail())
                writeToConsole("Could not open demo!");
            else
                GameInfo.info.PlayDemo(demo);
		}
		else
		{
			writeToConsole("Usage: playdemo <demo name>");
		}
	}

	private void frictionCommand(string[] input)
	{
		PlayerInfo myPlayerInfo = GameInfo.info.getPlayerInfo();

		if(myPlayerInfo == null)
		{
			writeToConsole("No player loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current friction: " + myPlayerInfo.getFriction());
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				myPlayerInfo.setFriction(newVal);
			}
		}
		else
		{
			writeToConsole("Usage: move_friction (new friction)");
		}
	}

	private void accelerationCommand(string[] input)
	{
		PlayerInfo myPlayerInfo = GameInfo.info.getPlayerInfo();

		if(myPlayerInfo == null)
		{
			writeToConsole("No player loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current acceleration: " + myPlayerInfo.getAcceleration());
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				myPlayerInfo.setAcceleration(newVal);
			}
		}
		else
		{
			writeToConsole("Usage: move_accel (new accel)");
		}
	}

	private void airAccelerationCommand(string[] input)
	{
		PlayerInfo myPlayerInfo = GameInfo.info.getPlayerInfo();

		if(myPlayerInfo == null)
		{
			writeToConsole("No player loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current air acceleration: " + myPlayerInfo.getAirAcceleration());
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				myPlayerInfo.setAirAcceleration(newVal);
			}
		}
		else
		{
			writeToConsole("Usage: move_airaccel (new air acceleration)");
		}
	}

	private void maxSpeedCommand(string[] input)
	{
		PlayerInfo myPlayerInfo = GameInfo.info.getPlayerInfo();

		if(myPlayerInfo == null)
		{
			writeToConsole("No player loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current speed limit: " + myPlayerInfo.getMaxSpeed());
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
				myPlayerInfo.setMaxSpeed(newVal);
			}
		}
		else
		{
			writeToConsole("Usage: move_maxspeed (new max speed)");
		}
	}

	private void maxAirSpeedCommand(string[] input)
	{
		PlayerInfo myPlayerInfo = GameInfo.info.getPlayerInfo();

		if(myPlayerInfo == null)
		{
			writeToConsole("No player loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current air speed limit: " + myPlayerInfo.getMaxAirSpeed());
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
				myPlayerInfo.setMaxAirSpeed(newVal);
			}
		}
		else
		{
			writeToConsole("Usage: move_maxairspeed (new max air speed)");
		}
	}

	private void jumpHeightCommand(string[] input)
	{
		PlayerInfo myPlayerInfo = GameInfo.info.getPlayerInfo();

		if(myPlayerInfo == null)
		{
			writeToConsole("No player loaded!");
			return;
		}
		if(input.Length == 1)
		{
			writeToConsole("Current jump height: " + myPlayerInfo.getJumpForce());
		}
		else if(input.Length == 2)
		{
			float newVal;
			if(float.TryParse(input[1], out newVal))
			{
				myPlayerInfo.setJumpForce(newVal);
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
				GameInfo.info.setGravity(newVal);
			}
		}
		else
		{
			writeToConsole("Usage: move_gravity (new gravity)");
		}
	}

    private void noclipCommand(string[] input)
    {
        PlayerInfo myPlayerInfo = GameInfo.info.getPlayerInfo();

        if (myPlayerInfo == null)
        {
            writeToConsole("No player loaded!");
            return;
        }

        if (input.Length == 1)
        {
            writeToConsole("Noclip: " + myPlayerInfo.GetNoclip());
        }
        else if (input.Length == 2)
        {
            int newVal;
            if (int.TryParse(input[1], out newVal))
            {
                if (newVal == 0)
                {
                    myPlayerInfo.SetNoclip(false);
                }
                else
                {
                    myPlayerInfo.SetNoclip(true);
                }
            }
        }
        else
        {
            writeToConsole("Usage: noclip (1 or 0)");
        }
    }

	private void cheatsCommand(string[] input)
	{
		PlayerInfo myPlayerInfo = GameInfo.info.getPlayerInfo();

		if(myPlayerInfo == null)
		{
			writeToConsole("No player loaded!");
			return;
		}
		
		if(input.Length == 1)
		{
			writeToConsole("Cheats: " + myPlayerInfo.getCheats());
		}
		else if(input.Length == 2)
		{
			int newVal;
			if(int.TryParse(input[1], out newVal))
			{
				if(newVal == 0)
				{
					myPlayerInfo.setCheats(false);
				}
				else
				{
					myPlayerInfo.setCheats(true);
				}
			}
		}
		else
		{
			writeToConsole("Usage: cheats (1 or 0)");
		}
	}
}
