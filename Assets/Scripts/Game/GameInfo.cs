using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Api;
using Demos;
using UI;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;

namespace Game
{
    public class GameInfo : MonoBehaviour
    {
        public static GameInfo info;

        public string secretKey = "";
        public TextAsset helpFile;
        public List<string> soundNames;
        public List<AudioClip> soundClips;

        [SerializeField] private MapManager mapManager;
        public MapManager MapManager { get { return mapManager; } }

        [SerializeField] private GameObject eventSystemObj;

        public float circleSpeed1 = 10f;
        public float circleSpeed2 = 20f;
        public float circleSpeed3 = 30f;

        public bool InEditor { get; private set; }
        public bool CheatsActive { get; set; }

        private GameInfoFx fx;
        private Demo currentDemo;
        private GameObject myCanvas;

        private List<Action> actionQueue = new List<Action>();

        private void Awake()
        {
            if (info == null)
            {
                info = this;
                DontDestroyOnLoad(gameObject);
                eventSystemObj.SetActive(true);
            }
            else
            {
                Destroy(gameObject);
            }

            myCanvas = transform.Find("Canvas").gameObject;

            fx = new GameInfoFx(myCanvas.transform.FindChild("FxImage").GetComponent<Image>());
            SceneManager.sceneLoaded += (scene, mode) => fx.StartColorFade(Color.black, new Color(0f, 0f, 0f, 0f), 0.5f);

            UnityUtils.MainThread = Thread.CurrentThread;
        }

        private void Update()
        {
            Settings.Input.ExecuteBoundActions();

            actionQueue.ForEach(action => action());
            actionQueue.Clear();

            //Update effects
            fx.Update();
        }

        //Load a level
        public void PlayLevel(MapData map)
        {
            InEditor = false;
            fx.StartColorFade(new Color(0f, 0f, 0f, 0f), Color.black, 0.5f, () =>
            {
                GameMenu.SingletonInstance.CloseAllWindows();
                MapManager.LoadMap(map);
            });
        }

        public void LoadEditor(string editorLevelName)
        {
            InEditor = true;
            SceneManager.LoadScene("LevelEditor");
        }

        public void LoadMainMenu()
        {
            GameMenu.SingletonInstance.CloseAllWindows();
            SceneManager.LoadScene("MainMenu");
        }

        //Player hit the goal
        public void RunFinished(long[] time, MapData map, Demo demo)
        {
            currentDemo = demo;

            GameMenu.SingletonInstance.CloseAllWindows();
            EndLevelWindow window = (EndLevelWindow) GameMenu.SingletonInstance.AddWindow(Window.END_LEVEL);
            window.Initialize(currentDemo, PlayerSave.current.SaveTimeIfPersonalBest(time, MapManager.CurrentMap));

            if (!demo.RunValid)
            {
                print("Invalid run!");
                return;
            }
            if (PlayerSave.current == null)
            {
                print("Invalid save!");
                return;
            }
            if (!PlayerSave.current.IsLoggedIn)
            {
                print("Account not logged in!");
                return;
            }

            StartCoroutine(UnityUtils.RunWhenDone(Leaderboard.SendEntry(PlayerSave.current, map.id, time.Last(), currentDemo), entryRequest =>
            {
                if (!entryRequest.Error)
                {
                    StartCoroutine(UnityUtils.RunWhenDone(Leaderboard.GetRecord(map), recordRequest =>
                    {
                        if (recordRequest.Error)
                        {
                            GameMenu.SingletonInstance.ShowError(recordRequest.ErrorText);
                        }
                        else if (recordRequest.Result.Length == 0)
                        {
                            GameMenu.SingletonInstance.ShowError("Record request returned no results!");
                        }
                        else
                        {
                            window.NewOnlineRank(recordRequest.Result[0].Rank);
                        }
                    }));
                }
            }));
        }

        public void RunOnMainThread(Action action)
        {
            actionQueue.Add(action);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void SetCursorLock(bool value)
        {
            CursorLockMode mode = CursorLockMode.None;
            if (value)
                mode = CursorLockMode.Locked;
            Cursor.lockState = mode;
            Cursor.visible = !value;
        }
    }
}