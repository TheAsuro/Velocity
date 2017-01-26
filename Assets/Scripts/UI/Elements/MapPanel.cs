using Api;
using Game;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.Elements
{
    public class MapPanel : MonoBehaviour
    {
        [SerializeField] private Text nameField;
        [SerializeField] private Text authorField;
        [SerializeField] private RawImage previewImage;
        [SerializeField] private Text pbField;
        [SerializeField] private Text wrField;
        [SerializeField] private Text wrPlayerField;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button pbButton;
        [SerializeField] private Button wrButton;

        public void Set(int slot, MapData map, string pb)
        {


            nameField.text = map.name;
            authorField.text = map.author;
            previewImage.texture = map.previewImage;
            pbField.text = pb;
            loadButton.onClick.AddListener(() => OnPlayableMapClick(map));
            pbButton.onClick.AddListener(() =>
            {
                LeaderboardWindow leaderboard = (LeaderboardWindow) GameMenu.SingletonInstance.AddWindow(Window.LEADERBOARD);
                // TODO: load leaderboards at pb index
                leaderboard.LoadMap(map);
            });
            wrButton.onClick.AddListener(() =>
            {
                LeaderboardWindow leaderboard = (LeaderboardWindow) GameMenu.SingletonInstance.AddWindow(Window.LEADERBOARD);
                leaderboard.LoadMap(map);
            });

            StartCoroutine(UnityUtils.RunWhenDone(Leaderboard.GetRecord(map), (request) =>
            {
                if (!request.Error)
                {
                    if (request.Result.Length == 1)
                        SetWrText(request.Result[0]);
                }
            }));
        }

        private void OnPlayableMapClick(MapData map)
        {
            GameInfo.info.PlayLevel(map);
        }

        private void SetWrText(LeaderboardEntry entry)
        {
            wrField.text = entry.time.ToString("0.0000");
            wrPlayerField.text = entry.playerName;
        }
    }
}