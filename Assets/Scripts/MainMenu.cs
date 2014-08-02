using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
	public List<string> mapNames = new List<string>();

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
			Rect mapSelectPos = new Rect(Screen.width / 4f, Screen.height / 2f - 100f, Screen.width / 2f, 200f);
			Rect mapInfoPos = new Rect(mapSelectPos.x, mapSelectPos.y - 35f, mapSelectPos.width, 22f);
			float boxWidth = mapSelectPos.width / 3f;
			int counter = 0;

			//Info box
			GUI.Box(mapInfoPos, "", skin.box);
			GUILayout.BeginArea(mapInfoPos);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Current User: " + GameInfo.info.getCurrentSave().getPlayerName() + " | Select a map.");
			if(GUILayout.Button("Back")) { setState(State.loadGame); }
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			//Big box for the list of maps
			GUI.Box(mapSelectPos, "", skin.box);

			//Create three coloumns
			for(int i = 0; i < 3; i++)
			{
				Rect boxRect = new Rect(mapSelectPos.x + i*boxWidth, mapSelectPos.y, boxWidth, mapSelectPos.height);
				GUILayout.BeginArea(boxRect);

				//Fill them with buttons
				for(int j = 0; j < 3; j++)
				{
					if(counter + j < mapNames.Count)
					{
						if(GUILayout.Button(mapNames[counter + j], skin.button)) { loadMap(mapNames[counter + j]); }
					}
				}
				counter += 3;

				GUILayout.EndArea();
			}
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
