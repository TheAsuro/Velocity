using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    [RequireComponent(typeof(Button))]
    public class CopyTextButton : MonoBehaviour
    {
        [SerializeField] private Text targetText;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => GUIUtility.systemCopyBuffer = targetText.text);
        }
    }
}