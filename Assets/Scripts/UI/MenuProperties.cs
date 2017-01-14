using System;
using UI.MenuWindows;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu]
    public class MenuProperties : ScriptableObject
    {
        [SerializeField]
        public GameObject escWindowPrefab;
        [SerializeField]
        private GameObject leaderboardDisplayPrefab;

        public MenuWindow CreateWindow(Window window, Transform spawnTransform)
        {
            StateMatch match = GetTypeFromState(window);
            return (MenuWindow)Instantiate(match.prefab, spawnTransform).GetComponent(match.type);
        }

        private StateMatch GetTypeFromState(Window window)
        {
            switch (window)
            {
                case Window.ESCMENU:
                    return new StateMatch()
                    {
                        type = typeof(EscWindow),
                        prefab = escWindowPrefab
                    };
                case Window.LEADERBOARD:
                    return new StateMatch()
                    {
                        type = typeof(LeaderboardWindow),
                        prefab = leaderboardDisplayPrefab
                    };
                default:
                    throw new ArgumentException();
            }
        }

        private struct StateMatch
        {
            public Type type;
            public GameObject prefab;
        }
    }
}