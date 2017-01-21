using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Demos
{
    public class Demo
    {
        private const string DEMO_VERSION_STRING = "VELOCITYDEMO 1.1.1";

        private List<DemoTick> tickList;
        private string playerName;
        private string levelName;
        private decimal finalTime = -1;

        /// <summary>
        /// Creates a new Demo, throws IOException.
        /// </summary>
        public Demo(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);
            LoadFromBinaryReader(reader);
        }

        //Make a demo from a list of ticks
        public Demo(List<DemoTick> pTickList, string pPlayerName, string pLevelName)
        {
            tickList = pTickList;
            playerName = pPlayerName;
            levelName = pLevelName;

            if(tickList != null && tickList.Count > 0)
                finalTime = tickList[tickList.Count - 1].GetTime();
        }

        private void LoadFromBinaryReader(BinaryReader reader)
        {
            tickList = new List<DemoTick>();

            //Read header
            reader.ReadString(); // demo version
            playerName = reader.ReadString();
            levelName = reader.ReadString();
            finalTime = reader.ReadDecimal();

            //Read ticks until end of file
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                decimal time = reader.ReadDecimal();
                float xPos = reader.ReadSingle();
                float yPos = reader.ReadSingle();
                float zPos = reader.ReadSingle();
                Vector3 pos = new Vector3(xPos, yPos, zPos);
                float xRot = reader.ReadSingle();
                float yRot = reader.ReadSingle();
                Quaternion rot = Quaternion.Euler(xRot, yRot, 0f);
                DemoTick tick = new DemoTick(time, pos, rot);

                tickList.Add(tick);
            }
        }

        public string GetPlayerName()
        {
            return playerName;
        }

        public string GetLevelName()
        {
            return levelName;
        }

        public int GetFrameCount()
        {
            return tickList.Count;	
        }

        public List<DemoTick> GetTickList()
        {
            return tickList;
        }

        public decimal GetTime()
        {
            return finalTime;
        }

        public void SaveToFile(string path)
        {
            string filename = Path.Combine(path, playerName + "-" + levelName + ".vdem");
            using (FileStream stream = new FileStream(filename, FileMode.Create))
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
                writer.Write(playerName);
                writer.Write(levelName);
                writer.Write(finalTime);

                //ticks
                foreach(DemoTick tick in tickList)
                {
                    writer.Write(tick.GetTime());
                    writer.Write(tick.GetPosition().x);
                    writer.Write(tick.GetPosition().y);
                    writer.Write(tick.GetPosition().z);
                    writer.Write(tick.GetRotation().eulerAngles.x);
                    writer.Write(tick.GetRotation().eulerAngles.y);
                }
            }
        }
    }
}
