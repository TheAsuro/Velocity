using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Demo
{
	private List<DemoTick> tickList;
	private string playerName;
	private string levelName;

	public Demo(string file)
	{
		string content = System.IO.File.ReadAllText(file);

		string[] lines = content.Split('\n');
		playerName = lines[1];
		levelName = lines[2];
		tickList = new List<DemoTick>();

		for(int i = 3; i < lines.Length; i++)
		{
			if(!lines[i].Equals(""))
			{
				string[] lineParts = lines[i].Split('|');
				float time = float.Parse(lineParts[0]);
				string[] posParts = lineParts[1].Split(';');
				Vector3 pos = new Vector3(float.Parse(posParts[0]), float.Parse(posParts[1]), float.Parse(posParts[2]));
				string[] rotParts = lineParts[2].Split(';');
				Quaternion rot = new Quaternion(float.Parse(rotParts[0]), float.Parse(rotParts[1]), float.Parse(rotParts[2]), float.Parse(rotParts[3]));
				DemoTick tick = new DemoTick(time, pos, rot);
				tickList.Add(tick);
			}
		}
	}

	public Demo(List<DemoTick> pTickList, string pPlayerName, string pLevelName)
	{
		tickList = pTickList;
		playerName = pPlayerName;
		levelName = pLevelName;
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

	public void saveToFile(string path)
	{
		string filename = path + "/" + playerName + "-" + levelName + ".vdem";

		//Initialize stream
		StreamWriter writer = File.CreateText(filename);

		//Write header
		writer.Write("VELOCITYDEMO 1.0.0\n" + playerName + "\n" + levelName + "\n");

		//Write ticks
		foreach(DemoTick tick in tickList)
		{
			writer.Write(tick.getTime());
			writer.Write("|");
			writer.Write(tick.getPosition().x);
			writer.Write(";");
			writer.Write(tick.getPosition().y);
			writer.Write(";");
			writer.Write(tick.getPosition().z);
			writer.Write("|");
			writer.Write(tick.getRotation().x);
			writer.Write(";");
			writer.Write(tick.getRotation().y);
			writer.Write(";");
			writer.Write(tick.getRotation().z);
			writer.Write(";");
			writer.Write(tick.getRotation().w);
			writer.Write("\n");
		}

		//End Stream
		writer.Flush();
		writer.Close();
	}
}
