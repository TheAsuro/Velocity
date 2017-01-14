using System.IO;
using UnityEngine;

namespace Demos
{
    internal static class DemoInfo
    {
        public static Demo[] GetAllDemos()
        {
            string[] names = GetDemoNames();
            Demo[] ret = new Demo[names.Length];

            for(int i = 0; i < names.Length; i++)
            {
                ret[i] = new Demo(names[i]);
            }

            return ret;
        }

        public static void DeleteDemoFile(string path)
        {
            File.Delete(path);
        }

        public static string[] GetDemoNames()
        {
            return Directory.GetFiles(Application.dataPath, "*.vdem");
        }
    }
}