using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Demo
{
	private Dictionary<float,Vector3> posList;
	private string playerName;
	private string levelName;

	public Demo(Dictionary<float,Vector3> pPosList, string pPlayerName, string pLevelName)
	{
		posList = pPosList;
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
		return posList.Count;	
	}

	public Dictionary<float,Vector3> getPosList()
	{
		return posList;
	}
}
