using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class ErrorWindow : DefaultMenuWindow
    {
        [SerializeField] private Text contentText;

        public void SetText(string text)
        {
            contentText.text = text;
        }
    }
}