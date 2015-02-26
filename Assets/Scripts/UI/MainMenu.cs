using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
	public List<string> mapNames = new List<string>();
	public List<string> mapAuthors = new List<string>();
    public GameObject[] menuObjects;

    private MenuState currentState;

	public enum MenuState
    {
        MainMenu,
        GameSelection
    }

    void Awake()
    {
        SetMenuState(MenuState.MainMenu);
        GameInfo.info.setMenuState(GameInfo.MenuState.othermenu);
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
        

        currentState = newState;
    }
}
