using System;
using UnityEngine;

namespace UI
{
    public class GameMenu : MonoBehaviour
    {
        public static GameMenu SingletonInstance { get; private set; }

        [SerializeField]
        private FileSelection fileSelection;

        private void Awake()
        {
            if (SingletonInstance != null)
                throw new InvalidOperationException();
            SingletonInstance = this;
        }

        public void ShowFileWindow(Action<string> okAction)
        {
            fileSelection.Show(okAction);
        }
    }
}
