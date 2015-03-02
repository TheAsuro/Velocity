using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class MainMenu : MonoBehaviour
{
    public static event System.EventHandler SettingsOpened;

    //General stuff
	public List<string> mapNames = new List<string>();
	public List<string> mapAuthors = new List<string>();
    public GameObject[] menuObjects;
    public GameObject[] settingObjects;
    public Toggle[] settingTitles;

    //References to specific things
    public GameObject gameSelectionContentPanel;
    public GameObject demoContentPanel;
    public GameObject mapPanelPrefab;
    public GameObject demoPanelPrefab;
    public InputField newPlayerNameField;
    public Text blogText;

    private int newPlayerSelectedIndex = 0;

    private MenuState currentState;
    public MenuState currentMenuState
    {
        get { return currentState; }
    }

	public enum MenuState
    {
        MainMenu,
        GameSelection,
        PlayerSelection,
        NewPlayer,
        Demos,
        Settings
    }

    public enum GameSelectionContent
    {
        PlayableMapList,
        ServerList,
        NewServerSettings,
        NewMapSettings,
        EditableMapList
    }

    void Awake()
    {
        SetMenuState(MenuState.MainMenu);
        GameInfo.info.setMenuState(GameInfo.MenuState.othermenu);
        loadLastPlayer();

        WWW www = new WWW("http://theasuro.de/Velocity/feed/");
        StartCoroutine(WaitForBlogEntry(www));
    }

    //Load the last player that was logged in, returns false if loading failed
    private bool loadLastPlayer()
    {
        if (!PlayerPrefs.HasKey("lastplayer"))
            return false;

        int index = PlayerPrefs.GetInt("lastplayer");
        loadPlayerAtIndex(index);
        return true;
    }

    private void loadPlayerAtIndex(int index)
    {
        GameInfo.info.setCurrentSave(new SaveData(index));
    }

    public void OnPlayButtonPress()
    {
        SaveData sd = GameInfo.info.getCurrentSave();
        if(sd == null || sd.getPlayerName().Equals(""))
        {
            //Create a new player if no player is selected
            SetMenuState(MenuState.PlayerSelection);
        }
        else
        {
            SetMenuState(MenuState.GameSelection);
        }
    }

    public void OnLoadButtonPress(int index)
    {
        //Check if a player exists, create new player if not
        SaveData sd = new SaveData(index);
        if (sd.getPlayerName().Equals(""))
        {
            newPlayerSelectedIndex = index;
            SetMenuState(MenuState.NewPlayer);
        }
        else
        {
            loadPlayerAtIndex(index);
            SetMenuState(MenuState.MainMenu);
        }
    }

    public void OnCreatePlayerOK()
    {
        CreateNewPlayer(newPlayerSelectedIndex, newPlayerNameField.text);
    }

    private void CreateNewPlayer(int index, string name)
    {
        SaveData sd = new SaveData(index, name);
        sd.save();
        loadPlayerAtIndex(index);
        SetMenuState(MenuState.MainMenu);
        ReplaceUiText.UpdateSaveInfo();
    }

    public void DeletePlayerAtIndex(int index)
    {
        SaveData sd = new SaveData(index);
        sd.deleteData(mapNames);
        ReplaceUiText.UpdateSaveInfo();

        //Log out from current player if we deleted that one
        if (GameInfo.info.getCurrentSave() != null && GameInfo.info.getCurrentSave().getIndex() == index)
            GameInfo.info.setCurrentSave(null);
    }

    public void SetMenuState(int stateID)
    {
        SetMenuState((MenuState)stateID);
    }

    public void SetMenuState(MenuState newState)
    {
        //Disable all menu groups
        foreach (GameObject menuObj in menuObjects)
        {
            menuObj.SetActive(false);
        }

        //Enable the selected group
        menuObjects[(int)newState].SetActive(true);

        //Do menu-specific preparations
        switch(newState)
        {
            case MenuState.GameSelection:
                SetGameSelectionContent(GameSelectionContent.PlayableMapList);
                break;
            case MenuState.Demos:
                LoadDemoPanels();
                break;
            case MenuState.Settings:
                if (SettingsOpened != null) { SettingsOpened(this, null); }
                break;
        }

        currentState = newState;
    }

    private void SetGameSelectionContent(GameSelectionContent newContent)
    {
        //Clear all children
        foreach(Object child in gameSelectionContentPanel.transform)
        {
            if(child.GetType().Equals(typeof(GameObject)))
                GameObject.Destroy(child);
        }

        //Create new children
        switch (newContent)
        {
            case GameSelectionContent.PlayableMapList:
                //Create a list of all playable maps
                int mapCount = Mathf.Min(mapNames.Count, mapAuthors.Count);
                ((RectTransform)gameSelectionContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapCount + 10f);

                for (int i = 0; i < mapCount; i++)
                {
                    CreateMapPanel(i, mapNames[i], mapAuthors[i]);
                }
                break;
        }
    }

    public void SetSettingGroup(int groupID)
    {
        SetMenuState(MenuState.Settings);

        foreach (GameObject obj in settingObjects)
        {
            obj.SetActive(false);
        }

        settingObjects[groupID].SetActive(true);
        settingTitles[groupID].isOn = true;
    }

    private void CreateMapPanel(int slot, string name, string author)
    {
        GameObject panel = (GameObject)GameObject.Instantiate(mapPanelPrefab);
        RectTransform t = (RectTransform)panel.transform;
        t.SetParent(gameSelectionContentPanel.transform);
        t.offsetMin = new Vector2(5f, 0f);
        t.offsetMax = new Vector2(-5f, 0f);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f);
        float heightOffset = ((RectTransform)gameSelectionContentPanel.transform).rect.height;
        t.localPosition = new Vector3(t.localPosition.x, -42.5f - slot * 75f + heightOffset, 0f);

        t.FindChild("Name").GetComponent<Text>().text = name;
        t.FindChild("Author").GetComponent<Text>().text = author;
        t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { OnPlayableMapClick(name); }); //Internet magic
    }

    private void LoadDemoPanels()
    {
        //Clear all children
        foreach (Object child in demoContentPanel.transform)
        {
            if (child.GetType().Equals(typeof(GameObject)))
                GameObject.Destroy(child);
        }

        //Create a list of all playable maps
        Demo[] allDemos = DemoInfo.GetAllDemos();
        ((RectTransform)demoContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * allDemos.Length + 10f);

        for (int i = 0; i < allDemos.Length; i++)
        {
            CreateDemoPanel(i, allDemos[i].getLevelName(), allDemos[i].getTime().ToString(), allDemos[i].getPlayerName());
        }
    }

    private void CreateDemoPanel(int slot, string map, string time, string player)
    {
        GameObject panel = (GameObject)GameObject.Instantiate(demoPanelPrefab);
        RectTransform t = (RectTransform)panel.transform;
        t.SetParent(demoContentPanel.transform);
        t.offsetMin = new Vector2(5f, 0f);
        t.offsetMax = new Vector2(-5f, 0f);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f);
        float heightOffset = ((RectTransform)demoContentPanel.transform).rect.height / 2f;
        t.localPosition = new Vector3(t.localPosition.x, -42.5f - slot * 75f + heightOffset, 0f);

        t.FindChild("Map").GetComponent<Text>().text = map;
        t.FindChild("Time").GetComponent<Text>().text = time;
        t.FindChild("Player").GetComponent<Text>().text = player;
    }

    private void OnPlayableMapClick(string mapName)
    {
        GameInfo.info.loadLevel(mapName);
    }

    public void OnSettingTitleStatusChange(int group)
    {
        if(settingTitles[group].isOn)
        {
            SetSettingGroup(group);
        }
    }

    public void SaveSettings()
    {
        GameInfo.info.savePlayerSettings();
    }

    public void DeleteSettings()
    {
        PlayerPrefs.DeleteKey("fov");
        PlayerPrefs.DeleteKey("mouseSpeed");
        PlayerPrefs.DeleteKey("invertY");
        PlayerPrefs.DeleteKey("volume");
        PlayerPrefs.DeleteKey("aniso");
        PlayerPrefs.DeleteKey("aa");
        PlayerPrefs.DeleteKey("textureSize");
        PlayerPrefs.DeleteKey("lighting");
        PlayerPrefs.DeleteKey("vsync");

        GameInfo.info.loadPlayerSettings();
        if (SettingsOpened != null)
            SettingsOpened(this, null);
    }

    public IEnumerator WaitForBlogEntry(WWW www)
    {
        yield return www;
        XmlDocument doc = new XmlDocument();
        string fixedStr = www.text.Substring(www.text.IndexOf("<?xml"));
        doc.LoadXml(fixedStr);
        XmlNode node = doc.GetElementsByTagName("item")[0];
        string title = "";
        string content = "";
        foreach(XmlNode subNode in node)
        {
           if (subNode.Name.Equals("title"))
                title = subNode.InnerText;

            if (subNode.Name.Equals("content:encoded"))
                content = subNode.InnerText;
        }
        blogText.text = title + "\n" + stripHtml(content);
    }

    private string stripHtml(string text)
    {
        return text.Replace("<p>","").Replace("</p>", "\n");
    }
}
