using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_STANDALONE
using System.IO;
#endif

public class Demo
{
	private List<DemoTick> tickList;
	private string playerName;
	private string levelName;
    private decimal finalTime = -1;
	private bool loadFromFileFailed = false;

	//Load a demo from file
	#if UNITY_STANDALONE
	public Demo(string file)
	{
		try
		{
            FileStream stream = new FileStream(file, FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);

			tickList = new List<DemoTick>();

            //Read header
            string demoVersion = reader.ReadString();
            playerName = reader.ReadString();
            levelName = reader.ReadString();
            finalTime = reader.ReadDecimal();

            //Read ticks until end of file
			while(reader.BaseStream.Position < reader.BaseStream.Length)
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
		catch(FileNotFoundException ex)
		{
			GameInfo.info.writeToConsole(ex.Message + "\n'" + file + "' is not a file!");
			loadFromFileFailed = true;
		}
	}
	#endif

	//Make a demo from a list of ticks
	public Demo(List<DemoTick> pTickList, string pPlayerName, string pLevelName)
	{
		tickList = pTickList;
		playerName = pPlayerName;
		levelName = pLevelName;

        if(tickList != null && tickList.Count > 0)
            finalTime = tickList[tickList.Count - 1].getTime();
	}

	//RIP
	public bool didLoadFromFileFail()
	{
		return loadFromFileFailed;
	}

	public string getPlayerName()
	{
		return playerName;
	}

	public string getLevelName()
	{
		return levelName;
	}

	public int getFrameCount()
	{
		return tickList.Count;	
	}

	public List<DemoTick> getTickList()
	{
		return tickList;
	}

    public decimal getTime()
    {
        return finalTime;
    }

	//Save demo to file
	#if UNITY_STANDALONE
	public void saveToFile(string path)
	{
		string filename = path + "/" + playerName + "-" + levelName + ".vdem";
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
            writer.Write(tick.getTime());
			writer.Write(tick.getPosition().x);
			writer.Write(tick.getPosition().y);
			writer.Write(tick.getPosition().z);
			writer.Write(tick.getRotation().eulerAngles.x);
			writer.Write(tick.getRotation().eulerAngles.y);
		}

		//close
        writer.Close();
        stream.Dispose();
	}
	#endif
}
