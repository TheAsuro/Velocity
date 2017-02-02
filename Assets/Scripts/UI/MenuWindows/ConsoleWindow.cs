using System.Collections.Generic;
using System.IO;
using System.Linq;
using Api;
using Console;
using Demos;
using Game;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.MenuWindows
{
    public class ConsoleWindow : DefaultMenuWindow
    {
        public TextAsset helpFile;
        public int rowCount = 22;

        private Canvas myCanvas;
        private Text myOutput;
        private InputField myInput;

        private Vector2 clickDelta = Vector2.zero;
        private bool mouseDown;

        private List<ConsoleCommand> consoleCommands = new List<ConsoleCommand>();

        private void Start()
        {
            //Find all parts of the console
            myCanvas = gameObject.transform.parent.GetComponent<Canvas>();
            myOutput = transform.Find("ConsoleOutput").Find("Mask").Find("Text").GetComponent<Text>();
            myInput = transform.Find("ConsoleInput").GetComponent<InputField>();

            consoleCommands.Add(new HelpCommand(this));
            consoleCommands.Add(new QuitCommand(this));
            consoleCommands.Add(new LogCommand(this));
            consoleCommands.Add(new PlayDemoCommand(this));
            consoleCommands.Add(new CheatsCommand(this));

            consoleCommands.Add(new FrictionCommand(this));
            consoleCommands.Add(new AcclerationCommand(this));
            consoleCommands.Add(new AirAcclerationCommand(this));
            consoleCommands.Add(new MaxSpeedCommand(this));
            consoleCommands.Add(new MaxAirSpeedCommand(this));
            consoleCommands.Add(new JumpHeightCommand(this));
            consoleCommands.Add(new GravityCommand(this));
            consoleCommands.Add(new NoclipCommand(this));
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

        public override void OnActivate()
        {
            base.OnActivate();

            Console.Console.ContentUpdate += OnConsoleContentUpdate;
            myInput.onEndEdit.AddListener(InputSubmit);
        }

        public override void OnClose()
        {
            base.OnClose();

            Console.Console.ContentUpdate -= OnConsoleContentUpdate;
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

        public void Write(string content)
        {
            myOutput.text += content;
        }

        public void ExecuteCommand(string command)
        {
            if (command.Equals(""))
                return;

            string[] commandParts = command.Trim().Split(' ');

            ConsoleCommand selectedCommand = consoleCommands.FirstOrDefault(cmd => cmd.MatchesName(commandParts[0]));
            if (selectedCommand != null)
            {
                if (selectedCommand.GetArgumentCounts().Length == 0 || selectedCommand.GetArgumentCounts().Contains(commandParts.Length - 1))
                    selectedCommand.Run(commandParts.Where((_, index) => index != 0).ToArray());
                else
                    Write(selectedCommand.UsageMessage());
            }
            else
            {
                Write("'" + command + "' is not a valid command!");
            }
        }
    }
}