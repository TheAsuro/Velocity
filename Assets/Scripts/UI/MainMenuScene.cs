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
            if (defaultDemo == "")
                throw new MissingReferenceException("Put in default demo pls");

            GameMenu.SingletonInstance.OnMenuAdded += (sender, args) =>
            {
                if (otherMenusOpen == 0)
                    Camera.main.gameObject.GetComponent<BlurOptimized>().enabled = true;
                otherMenusOpen++;
            };
            GameMenu.SingletonInstance.OnMenuRemoved += (sender, args) =>
            {
                otherMenusOpen--;
                if (otherMenusOpen == 0)
                    Camera.main.gameObject.GetComponent<BlurOptimized>().enabled = false;
            };

            LoadDemo(defaultDemo);
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
    }
}
