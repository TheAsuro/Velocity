using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
}
