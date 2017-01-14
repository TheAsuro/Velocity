using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor
{
    public class TogglePanel : MonoBehaviour
    {
        public GameObject objectToToggle;

        private void Awake()
        {
            Toggle t = GetComponent<Toggle>();
            if(t != null)
                t.onValueChanged.AddListener(SetValue);
        }

        private void SetValue(bool value)
        {
            objectToToggle.SetActive(value);
        }
    }
}
