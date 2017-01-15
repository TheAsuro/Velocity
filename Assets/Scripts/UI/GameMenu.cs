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
        EDITORPLAY,
        FILE
    }

    public class GameMenu : MonoBehaviour
    {
        public static GameMenu SingletonInstance { get; private set; }

        [SerializeField] private MenuProperties menuProperties;

        private Window currentWindow = Window.NONE;
        private Stack<MenuWindow> menuStack = new Stack<MenuWindow>();
        private Canvas myCanvas;

        private void Awake()
        {
            if (SingletonInstance == null)
            {
                SingletonInstance = this;
                myCanvas = GetComponent<Canvas>();
            }
            else
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Menu") && currentWindow == Window.NONE)
                AddWindow(Window.ESCMENU);
        }

        public MenuWindow AddWindow(Window window)
        {
            currentWindow = window;
            MenuWindow menu = menuProperties.CreateWindow(window, myCanvas.transform);
            menu.Activate();
            menuStack.Push(menu);
            return menu;
        }

        public void CloseWindow()
        {
            if (menuStack.Count == 0)
                throw new InvalidOperationException();
            menuStack.Pop().Close();
            currentWindow = menuStack.Count == 0 ? Window.NONE : menuProperties.FindWindowType(menuStack.Peek());
        }

        public void CloseAllWindows()
        {
            while (menuStack.Count > 0)
                CloseWindow();
        }
    }
}
