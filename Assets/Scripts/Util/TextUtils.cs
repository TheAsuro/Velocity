using System;
using Game;
using UnityEngine.SceneManagement;

namespace Util
{
    public static class TextUtils
    {
        public static string ToTimeString(this long val)
        {
            return TimeSpan.FromTicks(val).ToTimeString();
        }

        public static string ToShortTimeString(this long val)
        {
            return TimeSpan.FromTicks(val).ToShortTimeString();
        }

        public static string ToTimeString(this TimeSpan time)
        {
            string result = "";
            if (time.Days > 0)
                result += time.Days.ToString("0") + "d ";
            if (time.Days > 0 || time.Hours > 0)
                result += time.Hours.ToString("00") + ":";
            result += time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00") + "." + time.Milliseconds.ToString("00");
            return result;
        }

        public static string ToShortTimeString(this TimeSpan time)
        {
            string result = "";
            if (time.Days > 0)
                result += time.Days.ToString("0") + "d ";
            if (time.Days > 0 || time.Hours > 0)
                result += time.Hours.ToString("0") + ":";
            if (time.Days > 0 || time.Hours > 0 || time.Minutes > 0)
                result += time.Minutes.ToString("0") + ":";
            result += time.Seconds.ToString("0") + "." + time.Milliseconds.ToString("00");
            return result;
        }

        public static string ReplaceDefaultTemplates(this string str)
        {
            return str
                .Replace("$level", SceneManager.GetActiveScene().name)
                .Replace("$player", PlayerSave.current == null ? "" : PlayerSave.current.Name);
        }
    }
}