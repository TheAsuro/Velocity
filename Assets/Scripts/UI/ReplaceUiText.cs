using UnityEngine;
using System.Collections;

public class ReplaceUiText : MonoBehaviour
{
	public UnityEngine.UI.Text textScript;

	private string initialText = "";
	private SaveData player1, player2, player3;

	void Start()
	{
		initialText = textScript.text;

		//Special case for main menu
		if(Application.loadedLevelName == "MainMenu")
		{
			Transform curTransform = gameObject.transform;
			while(curTransform.parent != null)
			{
				curTransform = curTransform.parent;
			}
			if(curTransform.gameObject.GetComponent<MainMenu>() != null)
			{
				curTransform.gameObject.GetComponent<MainMenu>().addUiTextReset(init);
			}
		}

		init();
	}

	public void init()
	{
		player1 = new SaveData(1);
		player2 = new SaveData(2);
		player3 = new SaveData(3);
	}

	void Update()
	{
		string temp = initialText;
		SaveData playerSave = GameInfo.info.getCurrentSave();

		if(temp.Contains("$player1")) { temp = temp.Replace("$player1", player1.getPlayerName()); }
		else if(temp.Contains("$player2")) { temp = temp.Replace("$player2", player2.getPlayerName()); }
		else if(temp.Contains("$player3")) { temp = temp.Replace("$player3", player3.getPlayerName()); }
		else if(temp.Contains("$player") && playerSave != null) { temp = temp.Replace("$player", playerSave.getPlayerName()); }
		if(temp.Contains("$time")) { temp = temp.Replace("$time", GameInfo.info.getLastTime().ToString()); }
		if(temp.Contains("$map")) { temp = Application.loadedLevelName; }

		temp = temp.Replace("$selectedmap", GameInfo.info.getSelectedMap());
		string aut = GameInfo.info.getSelectedAuthor();
		if(! aut.Equals("?"))
		{
			temp = temp.Replace("$selectedauthor", "by " + aut);
		}
		else
		{
			temp = temp.Replace("$selectedauthor", "");
		}

		textScript.text = temp;
	}
}
