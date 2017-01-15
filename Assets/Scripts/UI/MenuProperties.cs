using System;
using System.Collections.Generic;
using System.Linq;
using UI.MenuWindows;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu]
    public class MenuProperties : ScriptableObject
    {
        [SerializeField] private GameObject escWindowPrefab;
        [SerializeField] private GameObject fileWindowPrefab;
        [SerializeField] private GameObject leaderboardWindowPrefab;
        [SerializeField] private GameObject settingsWindowPrefab;

        private Dictionary<Window, StateMatch> typePairs;

        private void TryCreateTypePairsInstance()
        {
            if (typePairs != null)
                return;

            typePairs = new Dictionary<Window, StateMatch>()
            {
                {Window.ESCMENU, new StateMatch(typeof(EscWindow), escWindowPrefab)},
                {Window.FILE, new StateMatch(typeof(FileWindow), fileWindowPrefab)},
                {Window.LEADERBOARD, new StateMatch(typeof(LeaderboardWindow), leaderboardWindowPrefab)},
                {Window.SETTINGS, new StateMatch(typeof(SettingsWindow), settingsWindowPrefab)},
            };
        }

        public MenuWindow CreateWindow(Window window, Transform spawnTransform)
        {
            StateMatch match = GetTypeFromState(window);
            return (MenuWindow) Instantiate(match.prefab, spawnTransform, false).GetComponent(match.type);
        }

        public Window FindWindowType(MenuWindow menuWindow)
        {
            TryCreateTypePairsInstance();
            return typePairs.First(pair => pair.Value.type == menuWindow.GetType()).Key;
        }

        private StateMatch GetTypeFromState(Window window)
        {
            TryCreateTypePairsInstance();
            if (!typePairs.ContainsKey(window))
                throw new NotImplementedException();
            return typePairs[window];
        }

        private struct StateMatch
        {
            public Type type;
            public GameObject prefab;

            public StateMatch(Type type, GameObject prefab)
            {
                this.type = type;
                this.prefab = prefab;
            }
        }
    }
}