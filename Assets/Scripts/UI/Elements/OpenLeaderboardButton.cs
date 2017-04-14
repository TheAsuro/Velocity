using System;
using Game;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    public class OpenLeaderboardButton : MonoBehaviour
    {
        [SerializeField] public int loadIndex = 0;
        [NonSerialized] public MapData targetMap = null;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                LeaderboardWindow window = (LeaderboardWindow) GameMenu.SingletonInstance.AddWindow(Window.LEADERBOARD);
                window.LoadMap(targetMap ?? GameInfo.info.MapManager.CurrentMap, loadIndex);
            });
        }
    }
}