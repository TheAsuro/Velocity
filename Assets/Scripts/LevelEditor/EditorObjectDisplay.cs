using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
    public class EditorObjectDisplay : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        public void SetText(string text)
        {
            transform.Find("Text").gameObject.GetComponent<Text>().text = text;
        }

        public void SetImage(Texture texture)
        {
            if(texture != null)
                transform.Find("RawImage").gameObject.GetComponent<RawImage>().texture = texture;
        }

        public void SetObject(GameObject obj, Texture image)
        {
            SetText(obj.name);
            SetImage(image);
        }

        private string GetText()
        {
            return transform.Find("Text").gameObject.GetComponent<Text>().text;
        }

        private void OnClick()
        {
            // TODO
        }
    }
}