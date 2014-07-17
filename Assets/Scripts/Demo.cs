using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Demo
{
	private List<DemoTick> tickList;
	private string playerName;
	private string levelName;

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
		//Initialize stream
		StreamWriter writer = File.CreateText(path);

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
