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

        public void OnActivate()
        {
            gameObject.SetActive(true);
        }

        public void OnSetAsBackground()
        {
            gameObject.SetActive(false);
        }

        public void OnClose()
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