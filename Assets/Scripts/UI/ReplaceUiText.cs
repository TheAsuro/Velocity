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

		temp = temp.Replace("$selectedmap", GameInfo.info.getSelectedMap());

		textScript.text = temp;
	}
}
