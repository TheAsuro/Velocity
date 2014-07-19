using UnityEngine;
using System.Collections;

public class SaveData
{
	private string playerName;

	//Creates a new instance with given data
	public SaveData(string pPlayerName)
	{
		playerName = pPlayerName;
	}

	//Creates a new instance from file saved at index
	public SaveData(int index)
	{
		playerName = PlayerPrefs.GetString("PlayerName" + index.ToString());
	}

	public string getPlayerName()
	{
		return playerName;
	}
}
