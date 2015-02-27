using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    //General stuff
	public List<string> mapNames = new List<string>();
	public List<string> mapAuthors = new List<string>();
    public GameObject[] menuObjects;

    //References to specific things
    public Button playButton;
    public GameObject gameSelectionContentPanel;
    public GameObject mapPanelPrefab;

    private MenuState currentState;
    public MenuState currentMenuState
    {
        get { return currentState; }
    }

	public enum MenuState
    {
        MainMenu,
        GameSelection,
        PlayerSelection
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

        if (!loadLastPlayer())
            playButton.interactable = false;
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

    public void loadPlayerAtIndex(int index)
    {
        GameInfo.info.setCurrentSave(new SaveData(index));
        playButton.interactable = true;
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
        }

        currentState = newState;
    }

    private void SetGameSelectionContent(GameSelectionContent newContent)
    {
        //Clear all children
        foreach(GameObject child in gameSelectionContentPanel.transform)
        {
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

    private void CreateMapPanel(int slot, string name, string author)
    {
        GameObject panel = (GameObject)GameObject.Instantiate(mapPanelPrefab);
        RectTransform t = (RectTransform)panel.transform;
        t.SetParent(gameSelectionContentPanel.transform);
        t.offsetMin = new Vector2(5f, 0f);
        t.offsetMax = new Vector2(-5f, 0f);
        t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f);
        t.localPosition = new Vector3(t.localPosition.x, -42.5f - slot * 75f + ((RectTransform)gameSelectionContentPanel.transform).rect.height, 0f);

        t.FindChild("Name").GetComponent<Text>().text = name;
        t.FindChild("Author").GetComponent<Text>().text = author;
        t.FindChild("Button").GetComponent<Button>().onClick.AddListener(delegate { OnPlayableMapClick(name); }); //Internet magic
    }

    private void OnPlayableMapClick(string mapName)
    {
        GameInfo.info.loadLevel(mapName);
    }
}
