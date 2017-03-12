using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    [RequireComponent(typeof(Selectable))]
    public class TabNavigator : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                GetComponent<Selectable>().navigation.selectOnRight.Select();
        }
    }
}