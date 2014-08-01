using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
	private bool drawMainButtons = true;
	private bool drawSelectNewGame = false;
	private bool drawSelectLoadGame = false;
	private bool drawNameField = false;
	private bool drawSelectMap = false;
	private string nameFieldText = "name";
	private int selectedNewGameIndex = -1;
	private GUISkin skin;

	private List<string> saveNames = new List<string>();

	private enum State
	{
		main = 1,
		newGame = 2,
		loadGame = 3,
		enterName = 4,
		selectMap = 5
	}

	void Start()
	{
		GameInfo.info.setMenuState(GameInfo.MenuState.inactive);
		GameInfo.info.menuLocked = true;
		skin = GameInfo.info.skin;
	}

	private void setState(State state)
	{
		updateSaveInfos();

		drawMainButtons = false;
		drawSelectNewGame = false;
		drawSelectLoadGame = false;
		drawNameField = false;
		drawSelectMap = false;

		switch(state)
		{
			case State.main:
				drawMainButtons = true;
				break;
			case State.newGame:
				drawSelectNewGame = true;
				break;
			case State.loadGame:
				drawSelectLoadGame = true;
				break;
			case State.enterName:
				drawNameField = true;
				break;
			case State.selectMap:
				drawSelectMap = true;
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
		Rect centerMenuPos = new Rect(Screen.width / 2f - 50f, Screen.height / 2f - 75f, 100f, 150f);
		
		//Main buttons
		if(drawMainButtons)
		{
			GUI.Box(centerMenuPos, "", skin.box);
			GUILayout.BeginArea(centerMenuPos);
			if(GUILayout.Button("New", skin.button)) { setState(State.newGame); }
			if(GUILayout.Button("Load", skin.button)) { setState(State.loadGame); }
			if(GUILayout.Button("Quit", skin.button)) { Application.Quit(); }
			GUILayout.EndArea();
		}

		//New game
		if(drawSelectNewGame)
		{
			GUI.Box(centerMenuPos, "", skin.box);
			GUILayout.BeginArea(centerMenuPos);
			GUILayout.Label("New");
			if(GUILayout.Button("1: " + saveNames[0], skin.button)) { selectedNewGameIndex = 1; setState(State.enterName); }
			if(GUILayout.Button("2: " + saveNames[1], skin.button)) { selectedNewGameIndex = 2; setState(State.enterName); }
			if(GUILayout.Button("3: " + saveNames[2], skin.button)) { selectedNewGameIndex = 3; setState(State.enterName); }
			if(GUILayout.Button("Back", skin.button)) { setState(State.main); }
			GUILayout.EndArea();
		}

		//Load game
		if(drawSelectLoadGame)
		{
			GUI.Box(centerMenuPos, "", skin.box);
			GUILayout.BeginArea(centerMenuPos);
			GUILayout.Label("Load");
			if(GUILayout.Button("1: " + saveNames[0], skin.button)) { loadGame(1); }
			if(GUILayout.Button("2: " + saveNames[1], skin.button)) { loadGame(2); }
			if(GUILayout.Button("3: " + saveNames[2], skin.button)) { loadGame(3); }
			if(GUILayout.Button("Back", skin.button)) { setState(State.main); }
			GUILayout.EndArea();
		}

		//select map
		if(drawSelectMap)
		{
			GUI.Box(centerMenuPos, "", skin.box);
			GUILayout.BeginArea(centerMenuPos);
			GUILayout.Label("Map");
			if(GUILayout.Button("Basic Tutorial", skin.button)) { loadMap("BasicTutorial"); }
			if(GUILayout.Button("Air Strafe Tutorial", skin.button)) { loadMap("StrafeTutorial"); }
			if(GUILayout.Button("Map 1", skin.button)) { loadMap("Level3"); }
			GUILayout.EndArea();
		}

		//Enter name for new game
		if(drawNameField)
		{
			GUI.Box(centerMenuPos, "", skin.box);
			GUILayout.BeginArea(centerMenuPos);
			GUILayout.Label("Enter name");
			nameFieldText = GUILayout.TextField(nameFieldText);
			if(GUILayout.Button("Back", skin.button)) { setState(State.newGame); }
			if(GUILayout.Button("OK", skin.button))
			{
				newGame(selectedNewGameIndex);
			}
			GUILayout.EndArea();
		}
	}

	private void newGame(int index)
	{
		SaveData data = new SaveData(index, nameFieldText);
		GameInfo.info.setCurrentSave(data);
	}

	private void loadGame(int index)
	{
		SaveData data = new SaveData(index);
		GameInfo.info.setCurrentSave(data);
		setState(State.selectMap);

	}

	private void loadMap(string name)
	{
		Application.LoadLevel(name);
	}
}
