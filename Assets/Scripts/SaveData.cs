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

	public bool saveIfPersonalBest(decimal time, string mapName)
	{
		decimal pbTime = getPersonalBest(mapName);
		if(pbTime == 0 || time < pbTime)
		{
			PlayerPrefs.SetString(playerName + "_" + mapName, time.ToString());
			return true;
		}
		return false;
	}

	public decimal getPersonalBest(string mapName)
	{
		return decimal.Parse(PlayerPrefs.GetString(playerName + "_" + mapName));
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