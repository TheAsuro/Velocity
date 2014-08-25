using UnityEngine;
using System.Collections;

public class ReplaceUiText : MonoBehaviour
{
	public UnityEngine.UI.Text textScript;

	private string initialText = "";

	void Start()
	{
		initialText = textScript.text;
	}

	void Update()
	{
		string temp = initialText;
		SaveData playerSave = GameInfo.info.getCurrentSave();
		if(playerSave != null)
		{
			temp = temp.Replace("$player", playerSave.getPlayerName());
		}
		textScript.text = temp;
	}
}
