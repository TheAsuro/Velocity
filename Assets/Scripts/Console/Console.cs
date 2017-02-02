using System;
using Util;

namespace Console
{
    public static class Console
    {
        public static string Content { get; private set; }

        public static event EventHandler<EventArgs<string>> ContentUpdate;

        public static void WriteLine(string line)
        {
            Write(line + "\n");
        }

        public static void Write(string text)
        {
            Content += text;

            if (ContentUpdate != null)
                ContentUpdate(null, new EventArgs<string>(text));
        }
    }
}