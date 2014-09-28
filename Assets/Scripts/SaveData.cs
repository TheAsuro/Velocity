using UnityEngine;
using System.Collections;

public class SaveData
{
	private int index;
	private string playerName;

	//Creates a new instance with given data
	public SaveData(int pIndex, string pPlayerName)
	{
		index = pIndex;
		playerName = pPlayerName;
	}

	//Loads a new instance from file saved at index
	public SaveData(int pIndex)
	{
		index = pIndex;
		playerName = PlayerPrefs.GetString("PlayerName" + pIndex.ToString());
	}

	public void save()
	{
		PlayerPrefs.SetString("PlayerName" + index.ToString(), playerName);
	}

	public bool saveIfPersonalBest(float time, string mapName)
	{
		float pbTime = getPersonalBest(mapName);
		if(pbTime == 0f || time < pbTime)
		{
			PlayerPrefs.SetFloat(playerName + "_" + mapName, time);
			return true;
		}
		return false;
	}

	public float getPersonalBest(string mapName)
	{
		return PlayerPrefs.GetFloat(playerName + "_" + mapName);
	}

	public string getPlayerName()
	{
		return playerName;
	}

	public int getIndex()
	{
		return index;
	}

	public void deleteData()
	{
		PlayerPrefs.DeleteKey("PlayerName" + index.ToString());
		//TODO delete player map times
	}
}