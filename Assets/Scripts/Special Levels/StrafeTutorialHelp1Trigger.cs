using UnityEngine;
using System.Collections;

public class StrafeTutorialHelp1Trigger : Event
{
	private bool draw = false;
	private GUISkin skin;

	void Start()
	{
		skin = GameInfo.info.skin;
	}

	public override void fire(params object[] parameters)
	{
		draw = true;
		GameInfo.info.setMenuState(GameInfo.MenuState.othermenu);
	}

	public override void reset()
	{
		draw = false;
		GameInfo.info.menuLocked = false;
		GameInfo.info.setMenuState(GameInfo.MenuState.closed);
	}

	void OnGUI()
	{
		if(draw)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 350f, Screen.height / 2f - 250f, 700f, 500f), skin.box);
			GUILayout.Box("Sample text \nSample text \nSample text \nSample text", skin.box, GUILayout.ExpandHeight(true));

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("OK", skin.button, GUILayout.MaxWidth(200f), GUILayout.MaxHeight(30f)))
			{
				reset();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.EndArea();
		}
	}
}
