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
        [SerializeField] private string previewLevelName;

        private int otherMenusOpen = 0;

        private void Start()
        {
            GameMenu.SingletonInstance.AddWindow(Window.MAIN_MENU);

            GameMenu.SingletonInstance.OnMenuAdded += IncreaseMenuCounter;
            GameMenu.SingletonInstance.OnMenuRemoved += DecreaseMenuCounter;

            SceneManager.LoadSceneAsync(previewLevelName, LoadSceneMode.Additive);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            GameMenu.SingletonInstance.OnMenuAdded -= IncreaseMenuCounter;
            GameMenu.SingletonInstance.OnMenuAdded -= DecreaseMenuCounter;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == previewLevelName)
            {
                RenderSettings.skybox = WorldInfo.info.skybox;
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