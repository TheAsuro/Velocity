using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Demos
{
    public class Demo
    {
        private static readonly string DEMO_DIR = Path.Combine("data","demos");
        private const string DEMO_VERSION_STRING = "VELOCITYDEMO 1.2";

        public string PlayerName { get; private set; }
        public string LevelName { get; private set; }
        public long TotalTickTime { get; private set; }
        public List<DemoTick> Ticks { get; private set; }
        public bool RunVaild { get; set; }
        private string DemoName { get; set; }

        /// <summary>
        /// Creates a new Demo from a file.
        /// </summary>
        public Demo(string demoName)
        {
            DemoName = demoName;
            FileStream stream = new FileStream(GetDemoPath(demoName), FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);
            LoadFromBinaryReader(reader);
        }

        //Make a demo from a list of ticks
        public Demo(List<DemoTick> pTicks, string pPlayerName, string pLevelName)
        {
            Ticks = pTicks;
            PlayerName = pPlayerName;
            LevelName = pLevelName;
            DemoName = Guid.NewGuid().ToString();

            if(Ticks != null && Ticks.Count > 0)
                TotalTickTime = Ticks[Ticks.Count - 1].Time;
        }

        private void LoadFromBinaryReader(BinaryReader reader)
        {
            Ticks = new List<DemoTick>();

            //Read header
            string demoVersion = reader.ReadString();
            if (demoVersion != DEMO_VERSION_STRING)
                throw new ArgumentException("Demo version mismatch! File: " + demoVersion + ", current: " + DEMO_VERSION_STRING);

            PlayerName = reader.ReadString();
            LevelName = reader.ReadString();
            TotalTickTime = reader.ReadInt64();
            RunVaild = reader.ReadBoolean();

            //Read ticks until end of file
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                long time = reader.ReadInt64();
                float xPos = reader.ReadSingle();
                float yPos = reader.ReadSingle();
                float zPos = reader.ReadSingle();
                Vector3 pos = new Vector3(xPos, yPos, zPos);
                float xRot = reader.ReadSingle();
                float yRot = reader.ReadSingle();
                Quaternion rot = Quaternion.Euler(xRot, yRot, 0f);
                DemoTick tick = new DemoTick(time, pos, rot);

                Ticks.Add(tick);
            }
        }

        public void SaveToFile()
        {
            using (FileStream stream = new FileStream(GetDemoPath(DemoName), FileMode.Create))
            {
                SaveToStream(stream);
            }
        }

        public void SaveToStream(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                //header
                writer.Write(DEMO_VERSION_STRING);
                writer.Write(PlayerName);
                writer.Write(LevelName);
                writer.Write(TotalTickTime);
                writer.Write(RunVaild);

                //ticks
                foreach(DemoTick tick in Ticks)
                {
                    writer.Write(tick.Time);
                    writer.Write(tick.Position.x);
                    writer.Write(tick.Position.y);
                    writer.Write(tick.Position.z);
                    writer.Write(tick.Rotation.eulerAngles.x);
                    writer.Write(tick.Rotation.eulerAngles.y);
                }
            }
        }

        public void DeleteDemoFile()
        {
            File.Delete(GetDemoPath(DemoName));
        }

        public static Demo[] GetAllDemos()
        {
            string[] names = GetDemoFiles();
            Demo[] ret = new Demo[names.Length];

            for(int i = 0; i < names.Length; i++)
            {
                ret[i] = new Demo(names[i]);
            }

            return ret;
        }

        private static string[] GetDemoFiles()
        {
            return Directory.GetFiles(Path.Combine(Application.dataPath, DEMO_DIR), "*.vdem");
        }

        private static string GetDemoPath(string demoName)
        {
            return Path.Combine(DEMO_DIR, demoName + ".vdem");
        }
    }
}
