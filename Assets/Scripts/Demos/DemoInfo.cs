using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

static class DemoInfo
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

    private static string[] GetDemoNames()
    {
        return Directory.GetFiles(Application.dataPath, "*.vdem");
    }
}