using System.IO;
using UnityEngine;

namespace Util
{
    public static class Paths
    {
        public const string DATA_DIR = "data";

        public static string PlayerSaveDir
        {
            get { return Path.Combine(DATA_DIR, "players"); }
        }

        public static string DemoDir
        {
            get { return Path.Combine(DATA_DIR, "demos"); }
        }

        public static string GetDemoPath(string demoName)
        {
            return Path.Combine(DemoDir, demoName + ".vdem");
        }

        public static string GetSavePath(string playerName)
        {
            return Path.Combine(PlayerSaveDir, playerName + ".vsav");
        }
    }
}