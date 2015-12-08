using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

public class MainMenu : MonoBehaviour
{
    public static event System.EventHandler LoadSettingsFromDisk;

    //General stuff
	public List<string> mapNames = new List<string>();
	public List<string> mapAuthors = new List<string>();
    public List<Texture2D> mapPreviews = new List<Texture2D>();
    public GameObject[] menuObjects;
    public GameObject[] settingObjects;
    public Toggle[] settingTitles;

    //References to specific things
    public GameObject gameSelectionContentPanel;
    public GameObject demoContentPanel;
    public GameObject mapPanelPrefab;
    public GameObject demoPanelPrefab;
    public GameObject editPanelPrefab;
    public Text blogText;

    public MenuState CurrentState { get; private set; }

    private NewPlayerMenu NewPlayerMenu { get { return menuObjects[(int)MenuState.NewPlayer].GetComponent<NewPlayerMenu>(); } }

	public enum MenuState
    {
        MainMenu,
        GameSelection,
        PlayerSelection,
        NewPlayer,
        Demos,
        Settings,
        Leaderboards
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
        GameInfo.info.SetMenuState(GameInfo.MenuState.othermenu);
		GameInfo.info.lockMenu();
        LoadLastPlayer();

        NewPlayerMenu.OnCreatedNewPlayer += (s, e) => OnPlayerCreated(e.Content);

        WWW www = new WWW("http://theasuro.de/Velocity/feed/");
        StartCoroutine(WaitForBlogEntry(www));
    }

    //Load the last player that was logged in, returns false if loading failed
    private bool LoadLastPlayer()
    {
        if (!PlayerPrefs.HasKey("lastplayer"))
            return false;

        int index = PlayerPrefs.GetInt("lastplayer");
        LoadPlayerAtIndex(index);
        return true;
    }

    private void LoadPlayerAtIndex(int index)
    {
        GameInfo.info.CurrentSave = new SaveData(index);
    }

    public void OnPlayButtonPress()
    {
        SaveData sd = GameInfo.info.CurrentSave;
        if(sd == null || sd.Account.Name.Equals(""))
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
        if (sd.Account.Name.Equals(""))
        {
            NewPlayerMenu.currentIndex = index;
            SetMenuState(MenuState.NewPlayer);
        }
        else
        {
            LoadPlayerAtIndex(index);
            SetMenuState(MenuState.MainMenu);
        }
    }

    public void OnPlayerCreated(int playerIndex)
    {
        LoadPlayerAtIndex(playerIndex);
        SetMenuState(MenuState.MainMenu);
        ReplaceUiText.UpdateSaveInfo();
    }

    public void DeletePlayerAtIndex(int index)
    {
        SaveData sd = new SaveData(index);
        sd.DeleteData(mapNames);
        ReplaceUiText.UpdateSaveInfo();

        //Log out from current player if we deleted that one
        if (GameInfo.info.CurrentSave != null && GameInfo.info.CurrentSave.Index == index)
            GameInfo.info.CurrentSave = null;
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
                LoadSettings();
                SetSettingGroup(0);
                break;
        }

        CurrentState = newState;
    }

    public void SetGameSelectionContent(int contentID)
    {
        SetGameSelectionContent((GameSelectionContent)contentID);
    }

    private void SetGameSelectionContent(GameSelectionContent newContent)
    {
        //Clear all children
        foreach(Transform child in gameSelectionContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //Create new children
        switch (newContent)
        {
            case GameSelectionContent.PlayableMapList:
                LoadPlayableMaps(); break;
            case GameSelectionContent.EditableMapList:
                LoadEditableMaps(); break;
            default:
                print("todo"); break;
        }
    }

    private void LoadPlayableMaps()
    {
        int mapCount = Mathf.Min(mapNames.Count, mapAuthors.Count);
        ((RectTransform)gameSelectionContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapCount + 10f);

        for (int i = 0; i < mapCount; i++)
        {
            string pb = "-";
            decimal pbTime = GameInfo.info.CurrentSave.GetPersonalBest(mapNames[i]);
            if (pbTime != -1)
                pb = pbTime.ToString("0.0000");
            CreateMapPanel(i, mapNames[i], mapAuthors[i], mapPreviews[i], pb);
        }
    }

    private void LoadEditableMaps()
    {
        string[] mapFiles = Directory.GetFiles(Application.dataPath, "*.vlvl");
        for (int i = 0; i < mapFiles.Length; i++)
        {
            mapFiles[i] = Path.GetFileNameWithoutExtension(mapFiles[i]);
        }

        ((RectTransform)gameSelectionContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * mapFiles.Length + 10f);

        for (int i = 0; i < mapFiles.Length; i++)
        {
            CreateEditPanel(i, mapFiles[i]);
        }
    }

    public void SetSettingGroup(int groupID)
    {
        foreach (GameObject obj in settingObjects)
        {
            obj.SetActive(false);
        }

        settingObjects[groupID].SetActive(true);
        settingTitles[groupID].isOn = true;
    }

    private void SetWrText(Transform panelTransform, Api.LeaderboardEntry entry)
    {
        if (panelTransform == null || entry == null)
            return;

        Transform wrObj = panelTransform.FindChild("WR");
        if (wrObj == null || wrObj.GetComponent<Text>() == null)
            return;

        wrObj.GetComponent<Text>().text = "WR: " + entry.time + " by " + entry.playerName;
    }

    private GameObject CreatePanel(int slot, GameObject prefab, Transform parent)
    {
        GameObject panel = (GameObject)GameObject.Instantiate(prefab);
        RectTransform t = (RectTransform)panel.transform;
        t.SetParent(parent);
        t.offsetMin = new Vector2(5f, 0f);
        t.offsetMax = new Vector2(-5f, 0f);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f);
        float heightOffset = ((RectTransform)parent.transform).rect.height / 2f;
        t.localPosition = new Vector3(t.localPosition.x, -42.5f - slot * 75f + heightOffset, 0f);
        return panel;
    }

    private void SetObjectText(Transform objTrans, string value)
    {
        objTrans.GetComponent<Text>().text = value;
    }

    private void CreateMapPanel(int slot, string name, string author, Texture2D preview, string pb)
    {
        Transform t = CreatePanel(slot, mapPanelPrefab, gameSelectionContentPanel.transform).transform;

        t.FindChild("Name").GetComponent<Text>().text = name;
        t.FindChild("Author").GetComponent<Text>().text = "Map by " + author;
        t.FindChild("Preview").GetComponent<RawImage>().texture = preview;
        t.FindChild("PB").GetComponent<Text>().text = "PB: " + pb;
        t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { OnPlayableMapClick(name); });

        Api.Leaderboard.GetRecord(name, (entry) => SetWrText(t, entry));
    }

    private void CreateEditPanel(int slot, string fileName)
    {
        Transform t = CreatePanel(slot, editPanelPrefab, gameSelectionContentPanel.transform).transform;

        t.FindChild("Name").GetComponent<Text>().text = fileName;
        t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { LoadEditorWithLevel(fileName); });
    }

    private void CreateDemoPanel(int slot, string map, string time, string player, string fileName)
    {
        Transform t = CreatePanel(slot, demoPanelPrefab, demoContentPanel.transform).transform;

        t.FindChild("Map").GetComponent<Text>().text = map;
        t.FindChild("Time").GetComponent<Text>().text = time;
        t.FindChild("Player").GetComponent<Text>().text = player;

        t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { PlayDemo(fileName); });
        t.FindChild("Remove").GetComponent<Button>().onClick.AddListener(delegate { DeleteDemo(fileName); });
    }

    private void LoadDemoPanels()
    {
        //Clear all children
        foreach (Transform child in demoContentPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //Create a list of all playable maps
        Demo[] allDemos = DemoInfo.GetAllDemos();
        string[] allDemoFiles = DemoInfo.GetDemoNames();
        ((RectTransform)demoContentPanel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f * allDemos.Length + 10f);

        for (int i = 0; i < allDemos.Length; i++)
        {
            CreateDemoPanel(i, allDemos[i].getLevelName(), allDemos[i].getTime().ToString(), allDemos[i].getPlayerName(), allDemoFiles[i]);
        }
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

    public void LoadSettings()
    {
        if (LoadSettingsFromDisk != null)
            LoadSettingsFromDisk(this, null);
    }

    public void SaveSettings()
    {
        Settings.AllSettings.SaveSettings();
    }

    private void PlayDemo(string name)
    {
        Demo demo = new Demo(Path.Combine(Application.dataPath, name));
        GameInfo.info.PlayDemo(demo);
    }

    private void DeleteDemo(string name)
    {
        DemoInfo.DeleteDemoFile(name);
        SetMenuState(MenuState.Demos);
    }

    public IEnumerator WaitForBlogEntry(WWW www)
    {
        yield return www;
        XmlDocument doc = new XmlDocument();
        string fixedStr = www.text.Substring(www.text.IndexOf("<?xml"));
        doc.LoadXml(fixedStr);
        XmlNode node = doc.GetElementsByTagName("item")[0];
        string title = "";
        string date = "";
        string content = "";
        foreach(XmlNode subNode in node)
        {
            if (subNode.Name.Equals("title"))
                title = subNode.InnerText;

            if (subNode.Name.Equals("pubDate"))
                date = parseDate(subNode.InnerText);

            if (subNode.Name.Equals("content:encoded"))
                content = subNode.InnerText;
        }
        blogText.text = "\"" + stripHtml(title) + "\" - " + date + "\n\n" + stripHtml(content);
    }

    private string parseDate(string text)
    {
        Regex r = new Regex("(.*, \\d* .* \\d*) ");
        return r.Match(text).Captures[0].Value;
    }

    private string stripHtml(string text)
    {
        return System.Web.HttpUtility.HtmlDecode(text).Replace("<p>","").Replace("</p>","\n");
    }

    public void LoadEditorWithLevel(string levelName)
    {
        GameInfo.info.editorLevelName = levelName;
        GameInfo.info.loadLevel("LevelEditor");
    }

    public void Quit()
    {
        GameInfo.info.quit();
    }
}
