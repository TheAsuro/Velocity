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
            else
                LoadDemo(defaultDemo);
        }

        private void LoadDemo(string path)
        {
            if (File.Exists(path))
            {
                Demo demo = new Demo(path);
                SceneManager.LoadScene(demo.GetLevelName(), LoadSceneMode.Additive);
                // TODO play demo
            }
            else
            {
                throw new ArgumentException("MainMenuScene tried to laod invalid demo!");
            }
        }
    }
}
