using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ConsoleResize : MonoBehaviour
    {
        public int maxLineCount = 22;
        public int lineHeight = 22;

        private RectTransform myTransform;
        private Text myText;
        private float startSizeY;

        private void Awake()
        {
            myTransform = (RectTransform)transform;
            myText = GetComponent<Text>();

            startSizeY = myTransform.sizeDelta.y;
        }

        private void Update()
        {
            int lineCount = myText.text.Split('\n').Length;

            if(lineCount > maxLineCount)
            {
                int additionalLines = lineCount - maxLineCount;
                myTransform.sizeDelta = new Vector2(myTransform.sizeDelta.x, startSizeY + additionalLines * lineHeight);
            }
            else
            {
                myTransform.sizeDelta = new Vector2(myTransform.sizeDelta.x, startSizeY);
            }
        }
    }
}
