using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Demos
{
    public class Demo
    {
        private List<DemoTick> tickList;
        private string playerName;
        private string levelName;
        private decimal finalTime = -1;
        private bool loadFromFileFailed = false;

        public Demo(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);
            LoadFromBinaryReader(reader);
        }

        //Load a demo from a stream
        public Demo(BinaryReader reader)
        {
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
            try
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
            catch (FileNotFoundException ex)
            {
                GameInfo.info.WriteToConsole(ex.Message);
                loadFromFileFailed = true;
            }
        }

        //RIP
        public bool DidLoadFromFileFail()
        {
            return loadFromFileFailed;
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

        //Save demo to file
        public void SaveToFile(string path)
        {
            string filename = Path.Combine(path, playerName + "-" + levelName + ".vdem");
            FileStream stream = new FileStream(filename, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            //header
            writer.Write("VELOCITYDEMO 1.1.0");
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

            //close
            writer.Close();
            stream.Dispose();
        }

        public byte[] GetBinaryData()
        {
            List<byte> data = new List<byte>();

            //header
        
            data.AddRange(Encoding.ASCII.GetBytes("VELOCITYDEMO 1.1.0"));
            data.AddRange(Encoding.ASCII.GetBytes(playerName));
            data.AddRange(Encoding.ASCII.GetBytes(levelName));
            data.AddRange(Encoding.ASCII.GetBytes(finalTime.ToString())); //TODO: convert this to binary

            //ticks
            foreach (DemoTick tick in tickList)
            {
                data.AddRange(Encoding.ASCII.GetBytes(tick.GetTime().ToString())); //TODO: convert this to binary
                data.AddRange(BitConverter.GetBytes(tick.GetPosition().x));
                data.AddRange(BitConverter.GetBytes(tick.GetPosition().y));
                data.AddRange(BitConverter.GetBytes(tick.GetPosition().z));
                data.AddRange(BitConverter.GetBytes(tick.GetRotation().eulerAngles.x));
                data.AddRange(BitConverter.GetBytes(tick.GetRotation().eulerAngles.y));
            }

            return data.ToArray();
        }
    }
}
