using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
	private Text myNumbers;
	private Text myTimes;
	private Text myPlayers;
	private Text myInfo;

	private int currentIndex = 0;

	void Awake()
	{
		myNumbers = transform.Find("Numbers").Find("Text").gameObject.GetComponent<Text>();
		myTimes = transform.Find("Times").Find("Text").gameObject.GetComponent<Text>();
		myPlayers = transform.Find("Players").Find("Text").gameObject.GetComponent<Text>();
		myInfo = transform.Find("Info").Find("Text").gameObject.GetComponent<Text>();
	}

	//Request leaderboard entries from the server
	public void getLeaderboardEntries(string map)
	{
		WWWForm form = new WWWForm();
		form.AddField("Map", map);
		form.AddField("Index", currentIndex);

		WWW www = new WWW("http://localhost/mapleaderboard.php", form);
		StartCoroutine(WaitForLeaderboardData(www, currentIndex));
	}

	//Wait for the server
	private IEnumerator WaitForLeaderboardData(WWW www, int index)
	{
		yield return www;

		if(www.error != null)
		{
			if(www.error.Equals("couldn't connect to host"))
			{
				myPlayers.text = "No connection to leaderboard server :/";
			}
			else
			{
				Debug.Log("WWW Error: " + www.error);
			}
		}
		else
		{
			processEntries(www.text, index);
		}
	}

	//Server answered, display recieved data
	private void processEntries(string entries, int index)
	{
		int indexCounter = index + 1;

		clear();

		if(entries.Equals(""))
		{
			return;
		}

		string[] rows = entries.Split('\n');

		foreach(string row in rows)
		{
			string[] items = row.Split('|');
			addRow(indexCounter.ToString(), items[0], items[1], items[2]);
			indexCounter++;
		}
	}

	private void clear()
	{
		myNumbers.text = "";
		myTimes.text = "";
		myPlayers.text = "";
		myInfo.text = "";
	}

	public void addRow(string nr, string time, string name, string dateStr)
	{
		string nl = "\n";
		if(myNumbers.text.Equals(""))
			nl = "";

		myNumbers.text += nl + nr;
		myTimes.text += nl + time;
		myPlayers.text += nl + name;
		myInfo.text += nl + dateStr;
	}

	//Wait for the server to answer
	public IEnumerator SendLeaderboardData(WWW www)
	{
		yield return www;

		if(www.error != null)
		{
			Debug.Log("WWW Error: " + www.error);
		}
	}

	public void up()
	{
		currentIndex -= 10;

		if(currentIndex < 0)
			currentIndex = 0;

		getLeaderboardEntries(Application.loadedLevelName);
	}

	public void down()
	{
		currentIndex += 10;

		getLeaderboardEntries(Application.loadedLevelName);
	}
}
