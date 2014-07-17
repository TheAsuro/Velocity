using UnityEngine;
using System.Collections;

public class InfoText : MonoBehaviour
{
	public string content;
	public int boxMargin;
	public bool ignoreDisabledHelp = false;

	private bool display = false;
	private GUISkin skin;

	void Awake()
	{
		skin = GameInfo.info.skin;
	}

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
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 200f, Screen.height - 50f - boxMargin, 400f, 50f));
			GUILayout.Box(content, skin.box);
			GUILayout.EndArea();
		}
	}
}
