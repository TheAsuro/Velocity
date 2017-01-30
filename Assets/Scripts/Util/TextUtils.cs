using System;

namespace Util
{
    public static class TextUtils
    {
        public static string ToTimeString(this long val)
        {
            return (val / 1000000).ToString();
        }

        public static string ShortText(this TimeSpan time)
        {
            string result = "";
            if (time.Days > 0)
                result += time.Days.ToString("0") + "d ";
            if (time.Days > 0 || time.Hours > 0)
                result += time.Hours.ToString("00") + ":";
            result += time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00") + ":" + time.Milliseconds.ToString("00");
            return result;
        }
    }
}