using System.Linq;
using Game;
using JetBrains.Annotations;
using UI.MenuWindows;

namespace Console
{
    public interface ConsoleCommand
    {
        /// <summary>
        /// Number of arguments this command accepts. Empty array means any number.
        /// </summary>
        int[] GetArgumentCounts();

        bool MatchesName(string commandStart);
        void Run(string[] arguments);
        string UsageMessage();
    }

    public abstract class DefaultConsoleCommand : ConsoleCommand
    {
        protected string commandName;
        private ConsoleWindow window;

        protected DefaultConsoleCommand(ConsoleWindow window, string commandName)
        {
            this.commandName = commandName;
            this.window = window;
        }

        protected bool CheatCheck()
        {
            if (GameInfo.info.CheatsActive)
                return true;
            else
            {
                WriteLine("You need to activate cheats to use this command!");
                return false;
            }
        }

        protected void Write(string text)
        {
            window.Write(text);
        }

        protected void WriteLine(string text)
        {
            window.Write(text + '\n');
        }

        public virtual int[] GetArgumentCounts()
        {
            return new[] {0};
        }

        public bool MatchesName(string commandStart)
        {
            return commandName == commandStart;
        }

        public abstract void Run(string[] arguments);

        public virtual string UsageMessage()
        {
            return "This is not how you use '" + commandName + "'. You need " + GetArgumentCounts().Select(x => x.ToString()).Aggregate((a, b) => a + "/" + b) + " arguments!";
        }
    }

    public abstract class IntConsoleCommand : DefaultConsoleCommand
    {
        protected IntConsoleCommand(ConsoleWindow window, string commandName) : base(window, commandName) { }

        public override void Run(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                int newVal;
                if (int.TryParse(arguments[0], out newVal))
                {
                    RunWithValue(newVal);
                    WriteLine(commandName + " set.");
                }
                else
                {
                    WriteLine("Could not parse input.");
                }
            }
            else
                PrintStatus();
        }

        protected abstract void RunWithValue(int value);

        protected abstract string PrintStatus();

        public override int[] GetArgumentCounts()
        {
            return new[] {0, 1};
        }

        public override string UsageMessage()
        {
            return "Usage: " + commandName + "(0/1)";
        }
    }

    public abstract class FloatConsoleCommand : DefaultConsoleCommand
    {
        protected FloatConsoleCommand(ConsoleWindow window, string commandName) : base(window, commandName) { }

        public override void Run(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                float newVal;
                if (float.TryParse(arguments[0], out newVal))
                {
                    RunWithValue(newVal);
                    WriteLine(commandName + " set.");
                }
                else
                {
                    WriteLine("Could not parse input.");
                }
            }
            else
                PrintStatus();
        }

        protected abstract void RunWithValue(float value);

        protected abstract string PrintStatus();

        public override int[] GetArgumentCounts()
        {
            return new[] {0, 1};
        }

        public override string UsageMessage()
        {
            return "Usage: " + commandName + "(0/1)";
        }
    }
}