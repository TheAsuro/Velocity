using UnityEngine;
using System.Collections;

public class InfoText : MonoBehaviour
{
	public string content;
	public int boxMargin;
	public int textMargin;
	public bool ignoreDisabledHelp = false;

	private bool display = false;

	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag.Equals("Player"))
		{
			display = true;
		}
	}

	void OnTriggerExit(Collider col)
	{
		if(col.gameObject.tag.Equals("Player"))
		{
			display = false;
		}
	}

	void OnGUI()
	{
		if(display && (GameInfo.info.showHelp || ignoreDisabledHelp))
		{
			GUISkin skin = GameInfo.info.skin;
			GUIContent gc = new GUIContent(content);
			float textWidth = skin.label.CalcSize(gc).x;
			float textHeight = skin.label.CalcSize(gc).y;

			Rect boxPos = new Rect(Screen.width / 2f - textWidth / 2f - boxMargin / 2f - textMargin / 2f, Screen.height - textHeight - boxMargin - textMargin, textWidth + textMargin, textHeight + textMargin);
			Rect textPos = new Rect(boxPos.x + textMargin / 2f, boxPos.y + textMargin / 2f, textWidth, textHeight);

			GUI.Box(boxPos, "");
			GUI.Label(textPos, gc);
		}
	}
}
