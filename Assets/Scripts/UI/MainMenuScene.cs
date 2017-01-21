using System;
using System.IO;
using Demos;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuScene : MonoBehaviour
    {
        [SerializeField] private string defaultDemo;

        private void Awake()
        {
            if (defaultDemo == "")
                throw new MissingReferenceException("Put in default demo pls");
            LoadDemo(defaultDemo);
        }

        private void LoadDemo(string path)
        {
            string absolutePath = Path.Combine(Application.dataPath, path);

            if (File.Exists(absolutePath))
            {
                Demo demo = new Demo(absolutePath);
                SceneManager.LoadScene(demo.GetLevelName(), LoadSceneMode.Additive);
                DemoPlayer.SingletonInstance.PlayDemo(demo);
            }
            else
            {
                throw new ArgumentException("MainMenuScene tried to laod invalid demo: " + absolutePath);
            }
        }
    }
}
