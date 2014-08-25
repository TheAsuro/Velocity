using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
	public List<string> mapNames = new List<string>();

	private bool drawSelectMap = false;
	private string nameFieldText = "name";
	private int selectedIndex = -1;
	private GUISkin skin;

	private List<string> saveNames = new List<string>();

	//GUI objects
	private GameObject mainMenuObj;
	private GameObject newGameMenuObj;
	private GameObject enterNameMenuObj;
	private GameObject loadGameMenuObj;
	private GameObject selectMapMenuObj;
	private GameObject settingsMenuObj;

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

		//Find canvas in children
		GameObject canvas = transform.Find("Canvas").gameObject;

		//Find menu objects
		mainMenuObj = canvas.transform.Find("MainMenu").gameObject;
		newGameMenuObj = canvas.transform.Find("NewGame").gameObject;
		enterNameMenuObj = canvas.transform.Find("EnterName").gameObject;
		loadGameMenuObj = canvas.transform.Find("LoadGame").gameObject;
		settingsMenuObj = canvas.transform.Find("Settings").gameObject;
		selectMapMenuObj = canvas.transform.Find("SelectMap").gameObject;

		//Load menu
		setState(State.main);
	}

	//Translate string to menu state, necessary because new gui
	public void setState(string name)
	{
		switch(name.ToLower())
		{
			case "main": setState(State.main); break;
			case "newgame": setState(State.newGame); break;
			case "loadgame": setState(State.loadGame); break;
			case "entername": setState(State.enterName); break;
			case "selectmap": setState(State.selectMap); break;
			case "settings": setState(State.settings); break;
			default: setState(State.main); break;
		}
	}

	//Menu state stuff
	private void setState(State state)
	{
		updateSaveInfos();

		mainMenuObj.SetActive(false);
		newGameMenuObj.SetActive(false);
		loadGameMenuObj.SetActive(false);
		enterNameMenuObj.SetActive(false);
		selectMapMenuObj.SetActive(false);
		settingsMenuObj.SetActive(false);

		switch(state)
		{
			case State.main:
				mainMenuObj.SetActive(true);
				break;
			case State.newGame:
				newGameMenuObj.SetActive(true);
				break;
			case State.loadGame:
				loadGameMenuObj.SetActive(true);
				break;
			case State.enterName:
				enterNameMenuObj.SetActive(true);
				break;
			case State.selectMap:
				selectMapMenuObj.SetActive(true);
				break;
			case State.settings:
				settingsMenuObj.SetActive(true);
				loadSettingsMenu();
				break;
		}
	}

	//Get information about current saves
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

	private void loadSettingsMenu()
	{
		GameInfo.info.loadPlayerSettings();
		setSlider("VolumeRow", GameInfo.info.volume);
		setSlider("SensitivityRow", GameInfo.info.mouseSpeed);
		setSlider("FovRow", GameInfo.info.fov);
		setSlider("VsyncRow", GameInfo.info.vsyncLevel);
		setSlider("LightingRow", GameInfo.info.lightingLevel);
		setSlider("AntiAliasingRow", GameInfo.info.antiAliasing);
		setSlider("AnisotropicFilteringRow", boolToFloat(GameInfo.info.anisotropicFiltering));
		setSlider("TextureSizeRow", GameInfo.info.textureSize);
	}

	private void setSlider(string rowName, float value)
	{
		settingsMenuObj.transform.Find(rowName).Find("Slider").gameObject.GetComponent<UnityEngine.UI.Slider>().value = value;
	}

	void OnGUI()
	{
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
	}

	private float boolToFloat(bool input)
	{
		if(input)
		{
			return 1f;
		}
		return 0f;
	}

	public void newGame()
	{
		newGame(selectedIndex);
	}

	private void newGame(int index)
	{
		SaveData data = new SaveData(index, nameFieldText);
		data.save();
		GameInfo.info.setCurrentSave(data);
		setState(State.selectMap);
	}

	public void loadGame()
	{
		loadGame(selectedIndex);
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

	public void setSelectedIndex(string value)
	{
		selectedIndex = int.Parse(value);
	}
}
