using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    [RequireComponent(typeof(Button))]
    public class CloseMenuButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => GameMenu.SingletonInstance.CloseWindow());
        }
    }
}
