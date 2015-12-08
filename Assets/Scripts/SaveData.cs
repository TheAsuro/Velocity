using UnityEngine;
using System.Collections.Generic;
using Api;

public class SaveData
{
	public int Index { get; private set; }
	public Account Account { get; private set; }

	//Creates a new instance with given data
	public SaveData(int index, string playerName)
	{
		Index = index;
        Account = new Account(playerName);
	}

	//Loads a new instance from file saved at index
	public SaveData(int index)
	{
		Index = index;
		Account = new Account(PlayerPrefs.GetString("PlayerName" + index));
	}

	public void SaveName()
	{
		PlayerPrefs.SetString("PlayerName" + Index.ToString(), Account.Name);
	}

	public bool SaveIfPersonalBest(decimal time, string mapName)
	{
        decimal pbTime = GetPersonalBest(mapName);
        if(pbTime <= 0 || time < pbTime)
        {
            PlayerPrefs.SetString(Account.Name + "_" + mapName, time.ToString());
            return true;
		}
		return false;
	}

	public decimal GetPersonalBest(string mapName)
	{
        if (PlayerPrefs.HasKey(Account.Name + "_" + mapName))
        {
			string s = PlayerPrefs.GetString(Account.Name + "_" + mapName);
            if(!s.Equals(""))
				return decimal.Parse(s);
			else
				return -1;
        }
        else
            return -1;
	}

	public void DeleteData(List<string> mapNames)
	{
		PlayerPrefs.DeleteKey("PlayerName" + Index.ToString());

        foreach(string map in mapNames)
        {
            PlayerPrefs.DeleteKey(Account.Name + "_" + map);
        }
	}
}