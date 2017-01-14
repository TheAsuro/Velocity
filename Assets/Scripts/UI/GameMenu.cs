using System;
using System.Collections.Generic;
using UI.MenuWindows;
using UnityEngine;

namespace UI
{
    public enum Window
    {
        NONE,
        ESCMENU,
        DEMO,
        LEADERBOARD,
        ENDLEVEL,
        EDITOR,
        EDITORPLAY
    }

    public class GameMenu : MonoBehaviour
    {
        public static GameMenu SingletonInstance { get; private set; }

        [SerializeField] private MenuProperties menuProperties;
        [SerializeField] private FileSelection fileSelection;

        private Window currentWindow = Window.NONE;
        private Stack<MenuWindow> menuStack = new Stack<MenuWindow>();
        private Canvas myCanvas;

        private void Awake()
        {
            if (SingletonInstance != null)
                throw new InvalidOperationException();

            SingletonInstance = this;
            myCanvas = transform.Find("Canvas").GetComponent<Canvas>();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Menu") && currentWindow == Window.NONE)
                AddWindow(Window.ESCMENU);
        }

        public void AddWindow(Window window)
        {
            currentWindow = window;
            MenuWindow menu = menuProperties.CreateWindow(window, myCanvas.transform);
            menu.Activate();
            menuStack.Push(menu);
        }

        public void CloseWindow()
        {
            menuStack.Pop().Close();
        }

        public void CloseAllWindows()
        {
            while (menuStack.Count > 0)
                CloseWindow();
        }

        public void ShowFileWindow(Action<string> okAction)
        {
            fileSelection.Show(okAction);
        }
    }
}
