using UI.MenuWindows;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FileInfo : MonoBehaviour
    {
        private FileWindow fileSel;
        private Text label;
        private Button button;
        private float originalHeight;
        private RectTransform rt;

        private void Awake()
        {
            rt = (RectTransform)transform;
            originalHeight = rt.rect.height;
        }

        public string Text
        {
            get { return label.text; }
            set { label.text = value; }
        }

        public void Initialize()
        {
            fileSel = transform.parent.parent.parent.gameObject.GetComponent<FileWindow>();
            button = transform.Find("Button").gameObject.GetComponent<Button>();
            label = transform.Find("Button").gameObject.GetComponent<Text>();
            button.onClick.AddListener(Select);
        }

        private void Select()
        {
            fileSel.UpdatePath(label.text);
        }

        public void SetVerticalOffset(int position)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, position * originalHeight * -1f);
            rt.offsetMin = new Vector2(rt.offsetMin.x, position * originalHeight * -1f - originalHeight);
        }

        public float GetOriginalHeight()
        {
            return originalHeight;
        }
    }
}
