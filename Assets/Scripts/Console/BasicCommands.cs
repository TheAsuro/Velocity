using System.IO;
using System.Linq;
using Demos;
using Game;
using UI.MenuWindows;
using UnityEngine;

namespace Console
{
    public class HelpCommand : DefaultConsoleCommand
    {
        public HelpCommand(ConsoleWindow window) : base(window, "help") { }

        public override void Run(string[] arguments)
        {
            Write(GameInfo.info.helpFile.text);
        }
    }

    public class QuitCommand : DefaultConsoleCommand
    {
        public QuitCommand(ConsoleWindow window) : base(window, "quit") { }

        public override void Run(string[] arguments)
        {
            GameInfo.info.Quit();
        }
    }

    public class LogCommand : DefaultConsoleCommand
    {
        public LogCommand(ConsoleWindow window) : base(window, "log") { }

        public override void Run(string[] arguments)
        {
            WriteLine(arguments.Aggregate((a, b) => a + " " + b));
        }

        public override int[] GetArgumentCounts()
        {
            return new int[] { };
        }
    }

    public class PlayDemoCommand : DefaultConsoleCommand
    {
        public PlayDemoCommand(ConsoleWindow window) : base(window, "playdemo") { }

        public override void Run(string[] arguments)
        {
            try
            {
                Demo demo = new Demo(Path.Combine(Application.dataPath, arguments[1]));
                WorldInfo.info.PlayDemo(demo, false, false);
            }
            catch (IOException e)
            {
                WriteLine("Could not open demo! \n" + e.StackTrace);
            }
        }

        public override int[] GetArgumentCounts()
        {
            return new[] {1};
        }

        public override string UsageMessage()
        {
            return "Usage: playdemo <demo name>";
        }
    }

    public class CheatsCommand : IntConsoleCommand
    {
        public CheatsCommand(ConsoleWindow window) : base(window, "cheats") { }

        protected override void RunWithValue(int value)
        {
            GameInfo.info.CheatsActive = value != 0;
        }

        protected override string PrintStatus()
        {
            return "Cheats: " + GameInfo.info.CheatsActive;
        }
    }
}