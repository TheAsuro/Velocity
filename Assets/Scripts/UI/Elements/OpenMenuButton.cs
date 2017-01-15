using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    [RequireComponent(typeof(Button))]
    public class OpenMenuButton : MonoBehaviour
    {
        [SerializeField] private Window targetWindowType;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => GameMenu.SingletonInstance.AddWindow(targetWindowType));
        }
    }
}
