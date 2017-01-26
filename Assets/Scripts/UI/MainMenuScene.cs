using System;
using System.IO;
using Demos;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

namespace UI
{
    public class MainMenuScene : MonoBehaviour
    {
        [SerializeField] private string defaultDemo;

        private Demo loadingDemo;
        private int otherMenusOpen = 0;

        private void Start()
        {
            GameMenu.SingletonInstance.AddWindow(Window.MAIN_MENU);

            if (defaultDemo == "")
                throw new MissingReferenceException("Put in default demo pls");

            GameMenu.SingletonInstance.OnMenuAdded += IncreaseMenuCounter;
            GameMenu.SingletonInstance.OnMenuRemoved += DecreaseMenuCounter;

            LoadDemo(defaultDemo);
        }

        private void OnDestroy()
        {
            GameMenu.SingletonInstance.OnMenuAdded -= IncreaseMenuCounter;
            GameMenu.SingletonInstance.OnMenuAdded -= DecreaseMenuCounter;
        }

        private void LoadDemo(string path)
        {
            string absolutePath = Path.Combine(Application.dataPath, path);

            if (File.Exists(absolutePath))
            {
                loadingDemo = new Demo(absolutePath);
                SceneManager.LoadSceneAsync(loadingDemo.GetLevelName(), LoadSceneMode.Additive);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                throw new ArgumentException("MainMenuScene tried to laod invalid demo: " + absolutePath);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (loadingDemo != null && scene.name == loadingDemo.GetLevelName())
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                WorldInfo.info.PlayDemo(loadingDemo, true, true);
                loadingDemo = null;
            }
        }

        private void IncreaseMenuCounter(object sender, EventArgs eventArgs)
        {
            if (otherMenusOpen == 0)
                Camera.main.gameObject.GetComponent<BlurOptimized>().enabled = true;
            otherMenusOpen++;
        }

        private void DecreaseMenuCounter(object sender, EventArgs eventArgs)
        {
            otherMenusOpen--;
            if (otherMenusOpen == 0)
                Camera.main.gameObject.GetComponent<BlurOptimized>().enabled = false;
        }
    }
}
