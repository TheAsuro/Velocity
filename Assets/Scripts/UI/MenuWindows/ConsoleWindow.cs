using Api;
using Demos;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class ConsoleWindow : MonoBehaviour, MenuWindow
    {
        public TextAsset helpFile;
        public int rowCount = 22;

        private Canvas myCanvas;
        private Text myOutput;
        private InputField myInput;

        private Vector2 clickDelta = Vector2.zero;
        private bool mouseDown;

        private void Start()
        {
            //Find all parts of the console
            myCanvas = gameObject.transform.parent.GetComponent<Canvas>();
            myOutput = transform.Find("ConsoleOutput").Find("Mask").Find("Text").GetComponent<Text>();
            myInput = transform.Find("ConsoleInput").GetComponent<InputField>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Rect titleRect = GetConsoleTitleRect();

                if (titleRect.x <= mousePos.x && mousePos.x <= titleRect.x + titleRect.width &&
                    titleRect.y <= mousePos.y && mousePos.y <= titleRect.y + titleRect.height)
                {
                    mouseDown = true;

                    clickDelta = mousePos - ((RectTransform) transform).anchoredPosition;
                }
            }

            if (Input.GetMouseButton(0))
            {
                if (mouseDown)
                {
                    RectTransform t = (RectTransform) transform;
                    Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    t.anchoredPosition = mousePos - clickDelta;
                    Vector2 newPos = t.anchoredPosition;
                    if (t.anchoredPosition.x < t.rect.width / 2)
                    {
                        newPos.x = t.rect.width / 2;
                    }
                    if (t.anchoredPosition.x > Screen.width - t.rect.width / 2)
                    {
                        newPos.x = Screen.width - t.rect.width / 2;
                    }
                    if (t.anchoredPosition.y > -t.rect.height / 2)
                    {
                        newPos.y = -t.rect.height / 2;
                    }
                    if (t.anchoredPosition.y < -Screen.height + t.rect.height / 2)
                    {
                        newPos.y = -Screen.height + t.rect.height / 2;
                    }
                    t.anchoredPosition = newPos;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                mouseDown = false;
            }
        }

        public void Activate()
        {
            //Registering events
            Console.ContentUpdate += OnConsoleContentUpdate;
            myInput.onEndEdit.AddListener(InputSubmit);
        }

        public void SetAsBackground()
        {

        }

        public void Close()
        {
            Console.ContentUpdate -= OnConsoleContentUpdate;
            myInput.onEndEdit.RemoveListener(InputSubmit);
        }

        private void OnConsoleContentUpdate(object s, EventArgs<string> e)
        {
            Write(e.Content);
        }

        private Rect GetConsoleTitleRect()
        {
            RectTransform titleTransform = transform.Find("Title").gameObject.GetComponent<RectTransform>();
            Rect r = new Rect(titleTransform.position.x * myCanvas.scaleFactor - (titleTransform.rect.width / 2f),
                titleTransform.position.y * myCanvas.scaleFactor - (titleTransform.rect.height / 2f),
                titleTransform.rect.width,
                titleTransform.rect.height);
            return r;
        }

        public void InputSubmit(string input)
        {
            Write(input);
            ExecuteCommand(input);
            myInput.text = "";
        }

        private void Write(string content)
        {
            myOutput.text += content;
        }

        public void ExecuteCommand(string command)
        {
            if (command.Equals(""))
                return;

            string[] commandParts = command.Trim().Split(' ');

            switch (commandParts[0].ToLower())
            {
                case "help": //Print helpful information
                    Write(helpFile.text);
                    break;
                case "quit": //Quit the game
                    GameInfo.info.Quit();
                    break;
                case "forcequit":
                    ForceQuitCommand(commandParts);
                    break;
                case "logtoconsole":
                    LogCommand(commandParts);
                    break;
                case "playdemo":
                    PlayDemoCommand(commandParts);
                    break;
                case "move_friction":
                    FrictionCommand(commandParts);
                    break;
                case "move_accel":
                    AccelerationCommand(commandParts);
                    break;
                case "move_airaccel":
                    AirAccelerationCommand(commandParts);
                    break;
                case "move_maxspeed":
                    MaxSpeedCommand(commandParts);
                    break;
                case "move_maxairspeed":
                    MaxAirSpeedCommand(commandParts);
                    break;
                case "move_jumpheight":
                    JumpHeightCommand(commandParts);
                    break;
                case "move_gravity":
                    GravityCommand(commandParts);
                    break;
                case "noclip":
                    NoclipCommand(commandParts);
                    break;
                case "cheats":
                    CheatsCommand(commandParts);
                    break;
                default:
                    Write("'" + command + "' is not a valid command!");
                    break;
            }
        }

        //Quits the game no matter what
        private void ForceQuitCommand(string[] input)
        {
            Application.Quit();
        }

        //Enables logging of certain changes
        private void LogCommand(string[] input)
        {
            if (input.Length == 2)
            {
                bool value = input[1].Equals("true") || input[1].Equals("1");
                GameInfo.info.logToConsole = value;
            }
            else
            {
                Write("Usage: logToConsole <true/false/1/0>");
            }
        }

        //Play a demo from a file
        private void PlayDemoCommand(string[] input)
        {
            if (input.Length == 2)
            {
                Demo demo = new Demo(System.IO.Path.Combine(Application.dataPath, input[1]));

                if (demo.DidLoadFromFileFail())
                    Write("Could not open demo!");
                else
                    GameInfo.info.PlayDemo(demo);
            }
            else
            {
                Write("Usage: playdemo <demo name>");
            }
        }

        private void FrictionCommand(string[] input)
        {
            PlayerBehaviour myPlayerInfo = GameInfo.info.GetPlayerInfo();

            if (myPlayerInfo == null)
            {
                Write("No player loaded!");
                return;
            }
            switch (input.Length)
            {
                case 1:
                    Write("Current friction: " + myPlayerInfo.GetFriction());
                    break;
                case 2:
                    float newVal;
                    if (float.TryParse(input[1], out newVal))
                    {
                        myPlayerInfo.SetFriction(newVal);
                    }
                    break;
                default:
                    Write("Usage: move_friction (new friction)");
                    break;
            }
        }

        private void AccelerationCommand(string[] input)
        {
            PlayerBehaviour myPlayerInfo = GameInfo.info.GetPlayerInfo();

            if (myPlayerInfo == null)
            {
                Write("No player loaded!");
                return;
            }
            switch (input.Length)
            {
                case 1:
                    Write("Current acceleration: " + myPlayerInfo.GetAcceleration());
                    break;
                case 2:
                    float newVal;
                    if (float.TryParse(input[1], out newVal))
                    {
                        myPlayerInfo.SetAcceleration(newVal);
                    }
                    break;
                default:
                    Write("Usage: move_accel (new accel)");
                    break;
            }
        }

        private void AirAccelerationCommand(string[] input)
        {
            PlayerBehaviour myPlayerInfo = GameInfo.info.GetPlayerInfo();

            if (myPlayerInfo == null)
            {
                Write("No player loaded!");
                return;
            }
            switch (input.Length)
            {
                case 1:
                    Write("Current air acceleration: " + myPlayerInfo.GetAirAcceleration());
                    break;
                case 2:
                    float newVal;
                    if (float.TryParse(input[1], out newVal))
                    {
                        myPlayerInfo.SetAirAcceleration(newVal);
                    }
                    break;
                default:
                    Write("Usage: move_airaccel (new air acceleration)");
                    break;
            }
        }

        private void MaxSpeedCommand(string[] input)
        {
            PlayerBehaviour myPlayerInfo = GameInfo.info.GetPlayerInfo();

            if (myPlayerInfo == null)
            {
                Write("No player loaded!");
                return;
            }
            switch (input.Length)
            {
                case 1:
                    Write("Current speed limit: " + myPlayerInfo.GetMaxSpeed());
                    break;
                case 2:
                    float newVal;
                    if (float.TryParse(input[1], out newVal))
                    {
                        if (newVal == 0f)
                        {
                            Write("Value can not be 0!");
                            return;
                        }
                        myPlayerInfo.SetMaxSpeed(newVal);
                    }
                    break;
                default:
                    Write("Usage: move_maxspeed (new max speed)");
                    break;
            }
        }

        private void MaxAirSpeedCommand(string[] input)
        {
            PlayerBehaviour myPlayerInfo = GameInfo.info.GetPlayerInfo();

            if (myPlayerInfo == null)
            {
                Write("No player loaded!");
                return;
            }
            switch (input.Length)
            {
                case 1:
                    Write("Current air speed limit: " + myPlayerInfo.GetMaxAirSpeed());
                    break;
                case 2:
                    float newVal;
                    if (float.TryParse(input[1], out newVal))
                    {
                        if (newVal == 0f)
                        {
                            Write("Value can not be 0!");
                            return;
                        }
                        myPlayerInfo.SetMaxAirSpeed(newVal);
                    }
                    break;
                default:
                    Write("Usage: move_maxairspeed (new max air speed)");
                    break;
            }
        }

        private void JumpHeightCommand(string[] input)
        {
            PlayerBehaviour myPlayerInfo = GameInfo.info.GetPlayerInfo();

            if (myPlayerInfo == null)
            {
                Write("No player loaded!");
                return;
            }
            switch (input.Length)
            {
                case 1:
                    Write("Current jump height: " + myPlayerInfo.GetJumpForce());
                    break;
                case 2:
                    float newVal;
                    if (float.TryParse(input[1], out newVal))
                    {
                        myPlayerInfo.SetJumpForce(newVal);
                    }
                    break;
                default:
                    Write("Usage: move_jumpheight (new jump height)");
                    break;
            }
        }

        private void GravityCommand(string[] input)
        {
            switch (input.Length)
            {
                case 1:
                    Write("Current gravity: " + Physics.gravity.y);
                    break;
                case 2:
                    float newVal;
                    if (float.TryParse(input[1], out newVal))
                    {
                        GameInfo.info.SetGravity(newVal);
                    }
                    break;
                default:
                    Write("Usage: move_gravity (new gravity)");
                    break;
            }
        }

        private void NoclipCommand(string[] input)
        {
            PlayerBehaviour myPlayerInfo = GameInfo.info.GetPlayerInfo();

            if (myPlayerInfo == null)
            {
                Write("No player loaded!");
                return;
            }

            switch (input.Length)
            {
                case 1:
                    Write("Noclip: " + myPlayerInfo.GetNoclip());
                    break;
                case 2:
                    int newVal;
                    if (int.TryParse(input[1], out newVal))
                    {
                        myPlayerInfo.SetNoclip(newVal != 0);
                    }
                    break;
                default:
                    Write("Usage: noclip (1 or 0)");
                    break;
            }
        }

        private void CheatsCommand(string[] input)
        {
            PlayerBehaviour myPlayerInfo = GameInfo.info.GetPlayerInfo();

            if (myPlayerInfo == null)
            {
                Write("No player loaded!");
                return;
            }

            switch (input.Length)
            {
                case 1:
                    Write("Cheats: " + myPlayerInfo.GetCheats());
                    break;
                case 2:
                    int newVal;
                    if (int.TryParse(input[1], out newVal))
                    {
                        myPlayerInfo.SetCheats(newVal != 0);
                    }
                    break;
                default:
                    Write("Usage: cheats (1 or 0)");
                    break;
            }
        }
    }
}