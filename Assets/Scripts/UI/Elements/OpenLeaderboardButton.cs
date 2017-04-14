using System;
using Game;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Elements
{
    public class OpenLeaderboardButton : MonoBehaviour
    {
        [SerializeField] public int loadIndex = 0;
        [NonSerialized] public MapData targetMap = null;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(CreateClickListener(targetMap, loadIndex));
        }

        public static UnityAction CreateClickListener(MapData map, int index)
        {
            return () =>
            {
                LeaderboardWindow window = (LeaderboardWindow) GameMenu.SingletonInstance.AddWindow(Window.LEADERBOARD);
                window.LoadMap(map ?? GameInfo.info.MapManager.CurrentMap, index);
            };
        }
    }
}