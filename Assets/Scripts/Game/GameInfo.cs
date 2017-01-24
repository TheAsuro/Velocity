using System;
using System.Collections.Generic;
using Api;
using Demos;
using UI;
using UI.MenuWindows;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class GameInfo : MonoBehaviour
    {
        public static GameInfo info;

        public PlayerData PlayerData { get; private set; }

        public string secretKey = "";
        public TextAsset helpFile;

        //GUI
        private string selectedMap;

        private string selectedAuthor = "?";
        private GameInfoFx fx;

        //Sound
        public List<string> soundNames;

        public List<AudioClip> soundClips;

        //Stuff
        private Demo currentDemo;

        private decimal lastTime = -1;

        public string LastTimeString
        {
            get { return lastTime.ToString("0.0000"); }
        }

        //Debug window (top-left corner, toggle with f8)
        public bool logToConsole = true;

        //GUI settings
        public float circleSpeed1 = 10f;

        public float circleSpeed2 = 20f;
        public float circleSpeed3 = 30f;

        //Editor
        public bool InEditor { get; private set; }

        public string editorLevelName = "";

        //References
        private GameObject myCanvas;

        //Load infos like player name, pb's, etc.
        private SaveData currentSave;

        public SaveData CurrentSave
        {
            get { return currentSave; }
            set
            {
                currentSave = value;
                if (value != null)
                {
                    PlayerPrefs.SetString("lastplayer", currentSave.Name);
                }
            }
        }

        public bool LastRunWasPb { get; private set; }

        public bool CheatsActive { get; set; }

        private void Awake()
        {
            if (info == null)
            {
                info = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            myCanvas = transform.Find("Canvas").gameObject;

            fx = new GameInfoFx(myCanvas.transform.FindChild("FxImage").GetComponent<Image>());

            if (!PlayerPrefs.HasKey("lastplayer"))
                CurrentSave = new SaveData(PlayerPrefs.GetString("lastplayer"));
        }

        private void Start()
        {
            GameMenu.SingletonInstance.AddWindow(Window.MAIN_MENU);
        }

        private void Update()
        {
            Settings.Input.ExecuteBoundActions();
            HttpApi.ConsumeCallbacks();

            //Update effects
            fx.Update();
        }

        //Prepare for new level
        private void OnLevelWasLoaded(int level)
        {
            fx.StartColorFade(Color.black, new Color(0f, 0f, 0f, 0f), 0.5f);
        }

        //Load a level
        public void LoadLevel(string levelName)
        {
            // TODO - better?
            InEditor = levelName == "editor";

            //Server stuff might be here later
            fx.StartColorFade(new Color(0f, 0f, 0f, 0f), Color.black, 0.5f, () => SceneManager.LoadScene(levelName));
        }

        //Player hit the goal
        public void RunFinished(TimeSpan time, Demo demo)
        {
            currentDemo = demo;
            lastTime = time.Ticks / (decimal) 10000000;

            LastRunWasPb = CurrentSave.SaveIfPersonalBest(lastTime, SceneManager.GetActiveScene().name);

            EndLevelWindow window = (EndLevelWindow) GameMenu.SingletonInstance.AddWindow(Window.END_LEVEL);
            window.Initialize(currentDemo);

            if (!WorldInfo.info.RaceScript.RunVaild)
            {
                print("Invalid run!");
                return;
            }
            if (currentSave == null)
            {
                print("Invalid save!");
                return;
            }
            if (!currentSave.Account.IsLoggedIn)
            {
                print("Account not logged in!");
                return;
            }

            Leaderboard.SendEntry(currentSave.Account.Name, lastTime, SceneManager.GetActiveScene().name, currentSave.Account.Token, currentDemo);
        }

        //Leave the game
        public void Quit()
        {
            Application.Quit();
        }

        public void DeletePlayer(string name)
        {
            SaveData sd = new SaveData(name);
            sd.DeleteData();

            //Log out from current player if we deleted that one
            if (CurrentSave != null && CurrentSave.Name == name)
                CurrentSave = null;
        }

        public decimal GetLastTime()
        {
            return lastTime;
        }

        //Map selection in main menu
        public void SetSelectedMap(string map, string author = "?")
        {
            selectedMap = map;
            selectedAuthor = author;
        }

        public string GetSelectedMap()
        {
            return selectedMap;
        }

        public string GetSelectedAuthor()
        {
            return selectedAuthor;
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

    internal class GameInfoFx
    {
        public delegate void Callback();

        private delegate void EffectUpdate(Effect effect);

        private Image effectImage;

        private List<Effect> activeEffects;

        private struct Effect
        {
            public float startTime;
            public float duration;
            public EffectUpdate update;
            public Callback callback;
            public Color startColor;
            public Color endColor;
        }

        public GameInfoFx(Image image)
        {
            activeEffects = new List<Effect>();

            effectImage = image;
        }

        //Call this in the script's update function
        public void Update()
        {
            //Go through active effects and update them
            for (int i = 0; i < activeEffects.Count; i++)
            {
                activeEffects[i].update(activeEffects[i]);
            }

            Settings.Input.ExecuteBoundActions();
        }

        public void StartColorFade(Color start, Color end, float duration, Callback callback = null)
        {
            Effect e = new Effect
            {
                startTime = Time.unscaledTime,
                duration = duration,
                startColor = start,
                endColor = end,
                update = FadeToColor,
                callback = callback
            };
            activeEffects.Add(e);
        }

        private void FadeToColor(Effect effect)
        {
            //Fade
            float progress = Interpolate(effect.startTime, Time.unscaledTime, effect.startTime + effect.duration);
            effectImage.color = Color.Lerp(effect.startColor, effect.endColor, progress);

            //Check if we are done
            if (progress >= 1f)
            {
                if (effect.callback != null)
                    effect.callback();
                activeEffects.Remove(effect);
            }
        }

        //Returns 0 if current == start; returns 1 if current == end
        private static float Interpolate(float start, float current, float end)
        {
            return (current - start) / (end - start);
        }
    }
}