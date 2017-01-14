using System;
using UnityEngine;

namespace UI
{
    public class MainSubMenu : MonoBehaviour
    {
        public static event EventHandler GoToMainMenu;

        public void BackButtonClick()
        {
            if (GoToMainMenu != null)
                GoToMainMenu(this, null);
        }
    }
}
