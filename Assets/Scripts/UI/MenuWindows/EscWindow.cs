using UnityEngine;

namespace UI.MenuWindows
{
    public class EscWindow : MonoBehaviour, MenuWindow
    {
        private void Start()
        {
            // TODO this is shit
            ((RectTransform)transform).anchoredPosition = Vector2.zero;
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

        public void ToPreviousMenu()
        {
            GameMenu.SingletonInstance.CloseWindow();
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