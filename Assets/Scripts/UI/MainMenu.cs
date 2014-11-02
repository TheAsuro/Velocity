using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
	public List<string> mapNames = new List<string>();
	public List<string> mapAuthors = new List<string>();
	public DrawMapButtons buttonDrawThingy;
	public delegate void Reset();

	private int selectedIndex = -1;
	private List<string> saveNames = new List<string>();
	private List<Reset> uiResets = new List<Reset>();

	//GUI objects
	private GameObject titleObj;
	private GameObject mainMenuObj;
	private GameObject newGameMenuObj;
	private GameObject enterNameMenuObj;
	private GameObject loadGameMenuObj;
	private GameObject selectMapMenuObj;
	private GameObject settingsMenuObj;
	private GameObject otherSettingsMenuObj;
	private GameObject serverSetupMenuObj;
	private GameObject serverJoinMenuObj;
	private GameObject serverPortObj;
	private GameObject serverPasswordObj;
	private GameObject serverJoinIpObj;
	private GameObject serverJoinPortObj;
	private GameObject serverJoinPasswordObj;

	private enum State
	{
		main,
		newGame,
		loadGame,
		enterName,
		selectMap,
		settings,
		othersettings,
		serversetup,
		serverjoin
	}

	void Start()
	{
		GameInfo.info.setMenuState(GameInfo.MenuState.inactive);
		GameInfo.info.menuLocked = true;

		//Find canvas in children
		GameObject canvas = transform.Find("Canvas").gameObject;

		//Find menu objects
		titleObj = canvas.transform.Find("Title").gameObject;
		mainMenuObj = canvas.transform.Find("MainMenu").gameObject;
		newGameMenuObj = canvas.transform.Find("NewGame").gameObject;
		enterNameMenuObj = canvas.transform.Find("EnterName").gameObject;
		loadGameMenuObj = canvas.transform.Find("LoadGame").gameObject;
		settingsMenuObj = canvas.transform.Find("Settings").gameObject;
		otherSettingsMenuObj = canvas.transform.Find("OtherSettings").gameObject;
		selectMapMenuObj = canvas.transform.Find("SelectMap").gameObject;
		serverSetupMenuObj = canvas.transform.Find("ServerSetup").gameObject;
		serverJoinMenuObj = canvas.transform.Find("ServerJoin").gameObject;
		serverPortObj = serverSetupMenuObj.transform.Find("PortInput").gameObject;
		serverPasswordObj = serverSetupMenuObj.transform.Find("PassInput").gameObject;
		serverJoinIpObj = serverJoinMenuObj.transform.Find("IpInput").gameObject;
		serverJoinPortObj = serverJoinMenuObj.transform.Find("PortInput").gameObject;
		serverJoinPasswordObj = serverJoinMenuObj.transform.Find("PassInput").gameObject;

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
			case "othersettings": setState(State.othersettings); break;
			case "serversetup": setState(State.serversetup); break;
			case "serverjoin": setState(State.serverjoin); break;
			default: setState(State.main); break;
		}
	}

	//Menu state stuff
	private void setState(State state)
	{
		updateSaveInfos();

		titleObj.SetActive(false);
		mainMenuObj.SetActive(false);
		newGameMenuObj.SetActive(false);
		loadGameMenuObj.SetActive(false);
		enterNameMenuObj.SetActive(false);
		selectMapMenuObj.SetActive(false);
		settingsMenuObj.SetActive(false);
		otherSettingsMenuObj.SetActive(false);
		serverSetupMenuObj.SetActive(false);
		serverJoinMenuObj.SetActive(false);

		switch(state)
		{
			case State.main:
				mainMenuObj.SetActive(true);
				titleObj.SetActive(true);
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
				GameInfo.info.setSelectedMap("");
				buttonDrawThingy.clearButtons();
				buttonDrawThingy.addButtons(this, mapNames);
				break;
			case State.settings:
				settingsMenuObj.SetActive(true);
				loadSettingsMenu();
				break;
			case State.othersettings:
				otherSettingsMenuObj.SetActive(true);
				break;
			case State.serversetup:
				serverSetupMenuObj.SetActive(true);
				break;
			case State.serverjoin:
				serverJoinMenuObj.SetActive(true);
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

		resetUi();
	}

	public void addUiTextReset(Reset reset)
	{
		uiResets.Add(reset);
	}

	private void resetUi()
	{
		foreach(Reset r in uiResets)
		{
			r();
		}
	}

	private void loadSettingsMenu()
	{
		GameInfo.info.loadPlayerSettings();
		setSlider("VolumeRow", GameInfo.info.volume);
		setSlider("SensitivityRow", GameInfo.info.mouseSpeed);
		setSlider("FovRow", GameInfo.info.fov);
		setSlider("VsyncRow", GameInfo.info.vsyncLevel, SettingsSlider.translateFloat(GameInfo.info.vsyncLevel));
		setSlider("LightingRow", GameInfo.info.lightingLevel);
		setSlider("AntiAliasingRow", GameInfo.info.antiAliasing);
		setSlider("AnisotropicFilteringRow", boolToFloat(GameInfo.info.anisotropicFiltering), SettingsSlider.translateFloat(boolToFloat(GameInfo.info.anisotropicFiltering)));
		setSlider("TextureSizeRow", GameInfo.info.textureSize, SettingsSlider.translateTextureSize(GameInfo.info.textureSize));
	}

	public void saveSettingsMenu()
	{
		GameInfo.info.savePlayerSettings();
	}

	//Set a specific slider to a value
	private void setSlider(string rowName, float value, string valueString = "")
	{
		string tempStr = valueString;
		if(tempStr.Equals("")) { tempStr = value.ToString(); }
		Transform rowT = settingsMenuObj.transform.Find(rowName);
		rowT.Find("Slider").gameObject.GetComponent<UnityEngine.UI.Slider>().value = value;
		rowT.Find("Value").gameObject.GetComponent<UnityEngine.UI.Text>().text = tempStr;
	}

	private float boolToFloat(bool input)
	{
		if(input)
		{
			return 1f;
		}
		return 0f;
	}

	public void startServer()
	{
		int port;

		UnityEngine.UI.Text portText = serverPortObj.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
		UnityEngine.UI.Text passText = serverPasswordObj.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();

		bool ok = int.TryParse(portText.text, out port);
		string pass = passText.text;

		if(ok)
		{
			GameInfo.info.startServer(port, pass, GameInfo.info.getSelectedMap());
		}
		else
		{
			portText.text = "42069";
		}
	}

	public void joinServer()
	{
		string ip;
		int port;
		string pass;

		UnityEngine.UI.Text ipText = serverJoinIpObj.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
		UnityEngine.UI.Text portText = serverJoinPortObj.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
		UnityEngine.UI.Text passText = serverJoinPasswordObj.transform.Find("Text").GetComponent<UnityEngine.UI.Text>();

		bool ok = int.TryParse(portText.text, out port);
		if(ok)
		{
			ip = ipText.text;
			pass = passText.text;
			GameInfo.info.connectToServer(ip, port, pass);
		}
		else
		{
			portText.text = "42069";
		}
	}

	public void loadServerSetup()
	{
		//Change to world hub if no map is specified
		if(GameInfo.info.getSelectedMap() == null)
		{
			GameInfo.info.setSelectedMap("WorldHub");
		}
		setState(State.serversetup);
	}

	public void newGame()
	{
		newGame(selectedIndex);
	}

	private void newGame(int index)
	{
		SaveData data = new SaveData(index, enterNameMenuObj.transform.Find("InputField").gameObject.GetComponent<UnityEngine.UI.InputField>().text.text);
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

	public void loadMap()
	{
		string selectedMap = GameInfo.info.getSelectedMap();
		if(!selectedMap.Equals(""))
		{
			loadMap(selectedMap);
		}
	}

	private void loadMap(string name)
	{
		Application.LoadLevel(name);
	}

	public void setSelectedIndex(string value)
	{
		selectedIndex = int.Parse(value);
	}

	public void setSelectedButton(GameObject button)
	{
		buttonDrawThingy.setSelectedButton(button);
	}

	public void deletePlayerData(string index)
	{
		int parsed;
		if(int.TryParse(index, out parsed))
		{
			deletePlayerData(parsed);
		}
	}

	private void deletePlayerData(int index)
	{
		SaveData temp = new SaveData(index);
		temp.deleteData();
		updateSaveInfos();
	}

	public void deleteSettings()
	{
		PlayerPrefs.DeleteKey("fov");
		PlayerPrefs.DeleteKey("mouseSpeed");
		PlayerPrefs.DeleteKey("volume");
		PlayerPrefs.DeleteKey("aniso");
		PlayerPrefs.DeleteKey("aa");
		PlayerPrefs.DeleteKey("textureSize");
		PlayerPrefs.DeleteKey("lighting");
		PlayerPrefs.DeleteKey("vsync");
	}

	public void deleteEverything()
	{
		PlayerPrefs.DeleteAll();
	}

	public void quit()
	{
		GameInfo.info.quit();
	}
}
