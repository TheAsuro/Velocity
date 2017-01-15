using UnityEngine;

namespace UI.MenuWindows
{
    public class EscWindow : MonoBehaviour, MenuWindow
    {
        private void Update()
        {
            if (Input.GetButtonDown("Menu"))
                GameMenu.SingletonInstance.CloseWindow();
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void SetAsBackground()
        {
            gameObject.SetActive(false);
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        public void ToMainMenu()
        {
            GameInfo.info.LoadLevel("MainMenu");
        }

        public void Quit()
        {
            GameInfo.info.Quit();
        }
    }
}