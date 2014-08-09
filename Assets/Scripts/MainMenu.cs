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
	private bool drawSettings = false;
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
		selectMap = 5,
		settings = 6
	}

	void Start()
	{
		GameInfo.info.setMenuState(GameInfo.MenuState.inactive);
		GameInfo.info.menuLocked = true;
		skin = GameInfo.info.skin;
		if(GameInfo.info.getCurrentSave() != null)
		{
			setState(State.selectMap);
		}
	}

	private void setState(State state)
	{
		updateSaveInfos();

		drawMainButtons = false;
		drawSelectNewGame = false;
		drawSelectLoadGame = false;
		drawNameField = false;
		drawSelectMap = false;
		drawSettings = false;

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
			case State.settings:
				drawSettings = true;
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
		Rect centerMenuPos = new Rect(Screen.width / 2f - 75f, Screen.height / 2f - 75f, 150f, 150f);
		
		//Main buttons
		if(drawMainButtons)
		{
			GUILayout.BeginArea(centerMenuPos, skin.box);
			if(GUILayout.Button("New", skin.button)) { setState(State.newGame); }
			if(GUILayout.Button("Load", skin.button)) { setState(State.loadGame); }
			if(GUILayout.Button("Settings", skin.button)) { setState(State.settings); }
			if(GUILayout.Button("Quit", skin.button)) { Application.Quit(); }
			GUILayout.EndArea();
		}

		//New game
		if(drawSelectNewGame)
		{
			GUILayout.BeginArea(centerMenuPos, skin.box);
			GUILayout.Label("New", skin.label);
			if(GUILayout.Button("1: " + saveNames[0], skin.button)) { selectedNewGameIndex = 1; setState(State.enterName); }
			if(GUILayout.Button("2: " + saveNames[1], skin.button)) { selectedNewGameIndex = 2; setState(State.enterName); }
			if(GUILayout.Button("3: " + saveNames[2], skin.button)) { selectedNewGameIndex = 3; setState(State.enterName); }
			if(GUILayout.Button("Back", skin.button)) { setState(State.main); }
			GUILayout.EndArea();
		}

		//Load game
		if(drawSelectLoadGame)
		{
			GUILayout.BeginArea(centerMenuPos, skin.box);
			GUILayout.Label("Load", skin.label);
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
			Rect mapInfoPos = new Rect(mapSelectPos.x, mapSelectPos.y - 35f, mapSelectPos.width, 30f);
			float boxWidth = mapSelectPos.width / 3f;
			int counter = 0;

			//Info box
			GUILayout.BeginArea(mapInfoPos, skin.box);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Current User: " + GameInfo.info.getCurrentSave().getPlayerName() + " | Select a map.", skin.label);
			if(GUILayout.Button("Back", skin.button, GUILayout.MaxWidth(100f))) { setState(State.loadGame); }
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

		if(drawSettings)
		{
			//Settings box
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 225f, Screen.height / 2f - 125f, 450f, 250f), skin.box);

			GUILayout.BeginHorizontal();

			//Descriptions
			GUILayout.BeginVertical(skin.box);
			GUILayout.Label("FOV", skin.customStyles[0]);
			GUILayout.Label("Mouse Sensitivity", skin.customStyles[0]);
			GUILayout.Label("Volume", skin.customStyles[0]);
			GUILayout.Label("Anisotropic Filtering", skin.customStyles[0]);
			GUILayout.Label("Antialiasing", skin.customStyles[0]);
			GUILayout.Label("Texture Size", skin.customStyles[0]);
			GUILayout.Label("Lighting Quality", skin.customStyles[0]);
			GUILayout.Label("VSync Count", skin.customStyles[0]);
			GUILayout.EndVertical();

			//Sliders
			GUILayout.BeginVertical(skin.box);

			GameInfo.info.fov = GUILayout.HorizontalSlider(GameInfo.info.fov, 60f, 120f, skin.horizontalSlider, skin.horizontalSliderThumb);
			GameInfo.info.fov = Mathf.RoundToInt(GameInfo.info.fov);

			GameInfo.info.mouseSpeed = GUILayout.HorizontalSlider(GameInfo.info.mouseSpeed, 0.5f, 10f, skin.horizontalSlider, skin.horizontalSliderThumb);
			GameInfo.info.mouseSpeed = floor(GameInfo.info.mouseSpeed, 1);

			GameInfo.info.volume = GUILayout.HorizontalSlider(GameInfo.info.volume, 0f, 1f, skin.horizontalSlider, skin.horizontalSliderThumb);
			GameInfo.info.volume = floor(GameInfo.info.volume, 2);

			float tempIn = 0f;
			if(GameInfo.info.anisotropicFiltering) { tempIn = 1f; }
			float tempOut = GUILayout.HorizontalSlider(tempIn, 0f, 1f, skin.horizontalSlider, skin.horizontalSliderThumb);
			GameInfo.info.anisotropicFiltering = (tempOut > 0.5f);

			GameInfo.info.antiAliasing = GUILayout.HorizontalSlider(GameInfo.info.antiAliasing, 0f, 8f, skin.horizontalSlider, skin.horizontalSliderThumb);
			GameInfo.info.antiAliasing = roundAA(GameInfo.info.antiAliasing);

			GameInfo.info.textureSize = GUILayout.HorizontalSlider(GameInfo.info.textureSize, 2f, 0f, skin.horizontalSlider, skin.horizontalSliderThumb);
			GameInfo.info.textureSize = floor(GameInfo.info.textureSize, 0);

			GameInfo.info.lightingLevel = GUILayout.HorizontalSlider(GameInfo.info.lightingLevel, 0f, 4f, skin.horizontalSlider, skin.horizontalSliderThumb);
			GameInfo.info.lightingLevel = floor(GameInfo.info.lightingLevel, 0);

			GameInfo.info.vsyncLevel = GUILayout.HorizontalSlider(GameInfo.info.vsyncLevel, 0f, 2f, skin.horizontalSlider, skin.horizontalSliderThumb);
			GameInfo.info.vsyncLevel = floor(GameInfo.info.vsyncLevel, 0);

			GUILayout.EndVertical();

			//Value labels
			GUILayout.BeginVertical(skin.box);
			GUILayout.Label(GameInfo.info.fov.ToString(), skin.customStyles[1]);
			GUILayout.Label(GameInfo.info.mouseSpeed.ToString(), skin.customStyles[1]);
			GUILayout.Label(GameInfo.info.volume.ToString(), skin.customStyles[1]);
			GUILayout.Label(translateBool(GameInfo.info.anisotropicFiltering), skin.customStyles[1]);
			GUILayout.Label(GameInfo.info.antiAliasing.ToString(), skin.customStyles[1]);
			GUILayout.Label(translateTextureSize(GameInfo.info.textureSize), skin.customStyles[1]);
			GUILayout.Label(GameInfo.info.lightingLevel.ToString(), skin.customStyles[1]);
			GUILayout.Label(GameInfo.info.vsyncLevel.ToString(), skin.customStyles[1]);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			//Ok/Cancel buttons
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("OK", skin.button)) { GameInfo.info.savePlayerSettings(); setState(State.main); }
			if(GUILayout.Button("Cancel", skin.button)) { GameInfo.info.loadPlayerSettings(); setState(State.main); }
			GUILayout.EndHorizontal();

			GUILayout.EndArea();
		}
	}

	//Returns rounded value of a float
	private float floor(float input, int decimalsAfterPoint)
	{
		if(decimalsAfterPoint <= 0)
		{
			return Mathf.Round(input);
		}
		else
		{
			float temp = input * Mathf.Pow(10, decimalsAfterPoint);
			return Mathf.Round(temp) / Mathf.Pow(10, decimalsAfterPoint);
		}
	}

	//Return rounded value acceptable for AA
	private float roundAA(float input)
	{
		if(input < 1f)
		{
			return 0f;
		}
		else if(input < 3f)
		{
			return 2f;
		}
		else if(input < 6f)
		{
			return 4f;
		}
		else
		{
			return 8f;
		}
	}

	private string translateBool(bool input)
	{
		if(input)
		{
			return "On";
		}
		else
		{
			return "Off";
		}
	}

	private string translateTextureSize(float input)
	{
		if(input == 0)
		{
			return "Full Size";
		}
		else if(input == 1)
		{
			return "Half Size";
		}
		else if(input == 2)
		{
			return "Quarter Size";
		}
		else
		{
			return "Error";
		}
	}

	private void newGame(int index)
	{
		SaveData data = new SaveData(index, nameFieldText);
		data.save();
		GameInfo.info.setCurrentSave(data);
		setState(State.selectMap);
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
