using System.Linq;
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
        private string commandName;
        private ConsoleWindow window;

        protected DefaultConsoleCommand(ConsoleWindow window, string commandName)
        {
            this.commandName = commandName;
            this.window = window;
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
}