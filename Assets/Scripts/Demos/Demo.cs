using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_STANDALONE_WIN
using System.IO;
#endif

public class Demo
{
	private List<DemoTick> tickList;
	private string playerName;
	private string levelName;
	private bool loadFromFileFailed = false;

	//Load a demo from file (Currently only windows)
	#if UNITY_STANDALONE_WIN
	public Demo(string file)
	{
		try
		{
			string content = GetString(System.IO.File.ReadAllBytes(file));

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

	//Convert string to bytes (for saving)
	private static byte[] GetBytes(string str)
	{
	    byte[] bytes = new byte[str.Length * sizeof(char)];
	    System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
	    return bytes;
	}

	//Convert bytes to string (for loading)
	private static string GetString(byte[] bytes)
	{
	    char[] chars = new char[bytes.Length / sizeof(char)];
	    System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
	    return new string(chars);
	}

	//Save demo to file (Currently only on windows)
	#if UNITY_STANDALONE_WIN
	public void saveToFile(string path)
	{
		string filename = path + "/" + playerName + "-" + levelName + ".vdem";
		string content = "";

		//header
		content += "VELOCITYDEMO 1.0.0\n" + playerName + "\n" + levelName + "\n";

		//ticks
		foreach(DemoTick tick in tickList)
		{
			content += tick.getTime();
			content += "|";
			content += tick.getPosition().x;
			content += ";";
			content += tick.getPosition().y;
			content += ";";
			content += tick.getPosition().z;
			content += "|";
			content += tick.getRotation().x;
			content += ";";
			content += tick.getRotation().y;
			content += ";";
			content += tick.getRotation().z;
			content += ";";
			content += tick.getRotation().w;
			content += "\n";
		}

		//write
		File.WriteAllBytes(filename, GetBytes(content));
	}
	#endif
}
