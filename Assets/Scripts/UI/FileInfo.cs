using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FileInfo : MonoBehaviour
{
    private FileSelection fileSel;
    private Text label;
    private Button button;
    private float originalHeight;
    RectTransform rt;

    void Awake()
    {
        rt = (RectTransform)transform;
        originalHeight = rt.rect.height;
    }

    public string text
    {
        get { return label.text; }
        set { label.text = value; }
    }

    public void Initialize()
    {
        fileSel = transform.parent.parent.parent.gameObject.GetComponent<FileSelection>();
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
