using UnityEngine;
using System.Collections;

public class SaveData
{
	private int index;
	private string playerName;
	private string playerLevel;
	private Vector3 playerPos;

	//Creates a new instance with given data
	public SaveData(int pIndex, string pPlayerName)
	{
		index = pIndex;
		playerName = pPlayerName;
		playerLevel = "";
		playerPos = Vector3.zero;
	}

	//Creates a new instance with given data (for a later level)
	public SaveData(int pIndex, string pPlayerName, string pPlayerLevel, Vector3 pPlayerPos)
	{
		index = pIndex;
		playerName = pPlayerName;
		playerLevel = pPlayerLevel;
		playerPos = pPlayerPos;
	}

	//Creates a new instance from file saved at index
	public SaveData(int pIndex)
	{
		index = pIndex;
		playerName = PlayerPrefs.GetString("PlayerName" + pIndex.ToString());
		playerLevel = PlayerPrefs.GetString("PlayerLevel" + index.ToString());
		float posX = PlayerPrefs.GetFloat("PlayerX" + index.ToString());
		float posY = PlayerPrefs.GetFloat("PlayerY" + index.ToString());
		float posZ = PlayerPrefs.GetFloat("PlayerZ" + index.ToString());
		playerPos = new Vector3(posX, posY, posZ);
	}

	public void save()
	{
		PlayerPrefs.SetString("PlayerName" + index.ToString(), playerName);
		PlayerPrefs.SetString("PlayerLevel" + index.ToString(), playerLevel);
		PlayerPrefs.SetFloat("PlayerX" + index.ToString(), playerPos.x);
		PlayerPrefs.SetFloat("PlayerY" + index.ToString(), playerPos.y);
		PlayerPrefs.SetFloat("PlayerZ" + index.ToString(), playerPos.z);
	}

	public string getPlayerName()
	{
		return playerName;
	}

	public string getPlayerLevel()
	{
		return playerLevel;
	}

	public Vector3 getPlayerPosition()
	{
		return playerPos;
	}

	public int getIndex()
	{
		return index;
	}
}