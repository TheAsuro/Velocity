using System;
using System.Collections;
using System.Collections.Generic;
using UI.MenuWindows;
using UnityEngine;
using Util;

namespace UI
{
    public enum Window
    {
        NONE,
        BUG_REPORT,
        DEMO_LIST,
        END_LEVEL,
        ESC_MENU,
        FILE,
        GAME_SELECTION,
        LEADERBOARD,
        LEVEL_SELECT,
        LOGIN,
        NEW_PLAYER,
        SETTINGS,
        MAIN_MENU,
        PLAY
    }

    public class GameMenu : MonoBehaviour
    {
        public static GameMenu SingletonInstance { get; private set; }

        private const float DEFAULT_TEXT_DISPLAY_DURATION = 0.8f;

        public event EventHandler OnMenuAdded;
        public event EventHandler OnMenuRemoved;

        [SerializeField] private MenuProperties menuProperties;
        [SerializeField] private GameObject debugWindow;

        private Window currentWindow = Window.NONE;
        private Stack<MenuWindow> menuStack = new Stack<MenuWindow>();
        private Canvas myCanvas;

        public static GameObject CreatePanel(int slot, GameObject prefab, Transform parent)
        {
            GameObject panel = Instantiate(prefab);
            RectTransform t = (RectTransform) panel.transform;
            t.SetParent(parent);
            t.offsetMin = new Vector2(5f, 0f);
            t.offsetMax = new Vector2(-5f, 0f);
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 75f);
            float heightOffset = ((RectTransform) parent.transform).rect.height / 2f;
            t.localPosition = new Vector3(t.localPosition.x, -42.5f - slot * 75f + heightOffset, 0f);
            return panel;
        }

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
            if (Input.GetButtonDown("Menu") && (currentWindow == Window.NONE || currentWindow == Window.PLAY))
                AddWindow(Window.ESC_MENU);
            if (Input.GetButtonDown("Debug"))
                ToggleDebugWindow();
        }

        public MenuWindow AddWindow(Window window)
        {
            if (window == Window.NONE)
                throw new ArgumentException("Can't add NONE window!");

            currentWindow = window;
            MenuWindow menu = menuProperties.CreateWindow(window, myCanvas.transform);
            menu.OnActivate();
            menuStack.Push(menu);

            if (OnMenuAdded != null)
                OnMenuAdded(this, new EventArgs());

            return menu;
        }

        public void CloseWindow()
        {
            if (menuStack.Count == 0)
                throw new InvalidOperationException();
            menuStack.Pop().OnClose();
            currentWindow = menuStack.Count == 0 ? Window.NONE : menuProperties.FindWindowType(menuStack.Peek());

            if (OnMenuRemoved != null)
                OnMenuRemoved(this, new EventArgs());
        }

        public void CloseAllWindows()
        {
            while (menuStack.Count > 0)
                CloseWindow();
        }

        public void ToggleDebugWindow()
        {
            debugWindow.SetActive(!debugWindow.activeSelf);
        }

        public DebugWindow GetDebugWindow()
        {
            return debugWindow.GetComponent<DebugWindow>();
        }

        public void ShowTextBox(string text, Color color, float duration = DEFAULT_TEXT_DISPLAY_DURATION)
        {
            ShowTextBox(new[] {new DisplayText(text, color)}, duration);
        }

        public void ShowTextBox(IEnumerable<DisplayText> lines, float duration = DEFAULT_TEXT_DISPLAY_DURATION)
        {
            GameObject textBox = menuProperties.CreateTextDisplay(lines, myCanvas.transform);
            StartCoroutine(UnityUtils.WaitForRemove(textBox, duration));
        }
    }
}