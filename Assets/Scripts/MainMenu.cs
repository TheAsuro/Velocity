using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
	private bool drawSelectNewGame = false;
	private bool drawSelectLoadGame = false;

	private List<string> saveNames = new List<string>();

	private enum State
	{
		main = 1,
		newGame = 2,
		loadGame = 3
	}

	void Start()
	{
		GameInfo.info.setMenuLocked(true);
	}

	private void setState(State state)
	{
		updateSaveInfos();

		switch(state)
		{
			case State.main:
				drawSelectNewGame = false;
				drawSelectLoadGame = false;
				break;
			case State.newGame:
				drawSelectNewGame = true;
				drawSelectLoadGame = false;
				break;
			case State.loadGame:
				drawSelectNewGame = false;
				drawSelectLoadGame = true;
				break;
		}
	}

	private void updateSaveInfos()
	{
		int saveCount = 3;

		saveNames.Clear();

		for(int i = 1; i <= saveCount; i++)
		{
			SaveData data = new SaveData(i);
			string name = data.getPlayerName();
			if(name.Equals(""))
			{
				name = "Empty Save";
			}
			saveNames.Add(name);
		}
	}

	void OnGUI()
	{
		//Center
		Rect centerMenuPos = new Rect(Screen.width / 2f - 50f, Screen.height / 2f - 50f, 100f, 100f);
		GUI.Box(centerMenuPos, "");
		GUILayout.BeginArea(centerMenuPos);
		if(GUILayout.Button("New")) { setState(State.newGame); }
		if(GUILayout.Button("Load")) { setState(State.loadGame); }
		if(GUILayout.Button("Quit")) { Application.Quit(); }
		GUILayout.EndArea();

		//New game
		if(drawSelectNewGame)
		{
			Rect newGameMenuPos = new Rect(Screen.width / 2f + 55, Screen.height / 2f - 50f, 100f, 100f);
			GUI.Box(newGameMenuPos, "");
			GUILayout.BeginArea(newGameMenuPos);
			GUILayout.Label("New");
			if(GUILayout.Button(saveNames[0])) { newGame(1); }
			if(GUILayout.Button(saveNames[1])) { newGame(2); }
			if(GUILayout.Button(saveNames[2])) { newGame(3); }
			GUILayout.EndArea();
		}

		//Load game
		if(drawSelectLoadGame)
		{
			Rect loadGameMenuPos = new Rect(Screen.width / 2f + 55, Screen.height / 2f - 50f, 100f, 100f);
			GUI.Box(loadGameMenuPos, "");
			GUILayout.BeginArea(loadGameMenuPos);
			GUILayout.Label("Load");
			if(GUILayout.Button(saveNames[0])) { loadGame(1); }
			if(GUILayout.Button(saveNames[1])) { loadGame(2); }
			if(GUILayout.Button(saveNames[2])) { loadGame(3); }
			GUILayout.EndArea();
		}
	}

	private void newGame(int index)
	{
		Application.LoadLevel("BasicTutorial");
	}

	private void loadGame(int index)
	{

	}
}
