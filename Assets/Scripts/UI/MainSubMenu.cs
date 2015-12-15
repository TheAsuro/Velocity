using UnityEngine;
using System;

public class MainSubMenu : MonoBehaviour
{
    public static EventHandler GoToMainMenu;

    protected void BackButtonClick()
    {
        if (GoToMainMenu != null)
            GoToMainMenu(this, null);
    }
}
