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
        [SerializeField] private GameObject fileWindowPrefab;
        [SerializeField] private GameObject escWindowPrefab;
        [SerializeField] private GameObject leaderboardDisplayPrefab;

        private Dictionary<Window, StateMatch> typePairs;

        private void TryCreateTypePairsInstance()
        {
            if (typePairs != null)
                return;

            typePairs = new Dictionary<Window, StateMatch>()
            {
                {Window.FILE, new StateMatch(typeof(FileWindow), fileWindowPrefab)},
                {Window.ESCMENU, new StateMatch(typeof(EscWindow), escWindowPrefab)},
                {Window.LEADERBOARD, new StateMatch(typeof(LeaderboardWindow), leaderboardDisplayPrefab)}
            };
        }

        public MenuWindow CreateWindow(Window window, Transform spawnTransform)
        {
            StateMatch match = GetTypeFromState(window);
            return (MenuWindow) Instantiate(match.prefab, spawnTransform).GetComponent(match.type);
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