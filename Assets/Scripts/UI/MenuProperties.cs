using System;
using System.Collections.Generic;
using System.Linq;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [CreateAssetMenu]
    public class MenuProperties : ScriptableObject
    {
        [SerializeField] private GameObject textDisplayPrefab;
        [SerializeField] private GameObject textDisplayLinePrefab;
        [SerializeField] private GameObject bugReportPrefab;
        [SerializeField] private GameObject demoListPrefab;
        [SerializeField] private GameObject endLevelPrefab;
        [SerializeField] private GameObject escWindowPrefab;
        [SerializeField] private GameObject fileWindowPrefab;
        [SerializeField] private GameObject gameSelectionPrefab;
        [SerializeField] private GameObject leaderboardWindowPrefab;
        [SerializeField] private GameObject loginWindowPrefab;
        [SerializeField] private GameObject mainMenuPrefab;
        [SerializeField] private GameObject newPlayerWindow;
        [SerializeField] private GameObject playWindow;
        [SerializeField] private GameObject settingsWindowPrefab;

        private Dictionary<Window, TypeAndPrefab> typePairs;

        private void TryCreateTypePairsInstance()
        {
            if (typePairs != null)
                return;

            typePairs = new Dictionary<Window, TypeAndPrefab>()
            {
                {Window.BUG_REPORT, new TypeAndPrefab(typeof(BugReportWindow), bugReportPrefab)},
                {Window.DEMO_LIST, new TypeAndPrefab(typeof(DemoListWindow), demoListPrefab)},
                {Window.END_LEVEL, new TypeAndPrefab(typeof(EndLevelWindow), endLevelPrefab)},
                {Window.ESC_MENU, new TypeAndPrefab(typeof(EscWindow), escWindowPrefab)},
                {Window.FILE, new TypeAndPrefab(typeof(FileWindow), fileWindowPrefab)},
                {Window.GAME_SELECTION, new TypeAndPrefab(typeof(GameSelectionWindow), gameSelectionPrefab)},
                {Window.LEADERBOARD, new TypeAndPrefab(typeof(LeaderboardWindow), leaderboardWindowPrefab)},
                {Window.LOGIN, new TypeAndPrefab(typeof(LoginWindow), loginWindowPrefab)},
                {Window.MAIN_MENU, new TypeAndPrefab(typeof(MainMenu), mainMenuPrefab)},
                {Window.NEW_PLAYER, new TypeAndPrefab(typeof(NewPlayerWindow), newPlayerWindow)},
                {Window.PLAY, new TypeAndPrefab(typeof(PlayWindow), playWindow)},
                {Window.SETTINGS, new TypeAndPrefab(typeof(SettingsWindow), settingsWindowPrefab)},
            };
        }

        public GameObject CreateTextDisplay(IEnumerable<DisplayText> displayTexts, Transform spawnTransform)
        {
            GameObject obj = Instantiate(textDisplayPrefab, spawnTransform, false);
            foreach (DisplayText disp in displayTexts)
            {
                GameObject line = Instantiate(textDisplayLinePrefab, obj.transform, false);
                line.GetComponent<Text>().text = disp.Text;
                line.GetComponent<Text>().color = disp.Color;
            }
            return obj;
        }

        public MenuWindow CreateWindow(Window window, Transform spawnTransform)
        {
            TypeAndPrefab match = GetTypeFromState(window);
            return (MenuWindow) Instantiate(match.prefab, spawnTransform, false).GetComponent(match.type);
        }

        public Window FindWindowType(MenuWindow menuWindow)
        {
            TryCreateTypePairsInstance();
            return typePairs.First(pair => pair.Value.type == menuWindow.GetType()).Key;
        }

        private TypeAndPrefab GetTypeFromState(Window window)
        {
            TryCreateTypePairsInstance();
            if (!typePairs.ContainsKey(window))
                throw new NotImplementedException(window.ToString());
            return typePairs[window];
        }

        private struct TypeAndPrefab
        {
            public Type type;
            public GameObject prefab;

            public TypeAndPrefab(Type type, GameObject prefab)
            {
                this.type = type;
                this.prefab = prefab;
            }
        }
    }

    public struct DisplayText
    {
        public string Text { get; private set; }
        public Color Color { get; private set; }

        public DisplayText(string text, Color color) : this()
        {
            Text = text;
            Color = color;
        }
    }
}