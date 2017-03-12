using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    public class EnterConfirm : MonoBehaviour
    {
        [SerializeField] private Button confirmButton;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return))
                confirmButton.onClick.Invoke();
        }
    }
}