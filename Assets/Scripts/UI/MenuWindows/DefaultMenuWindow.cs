using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class DefaultMenuWindow : MonoBehaviour, MenuWindow
    {
        public virtual void OnActivate()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnSetAsBackground()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnClose()
        {
            Destroy(gameObject);
        }

        protected void SetInteractive(bool value)
        {
            transform.GetComponentsInChildren<Selectable>().ToList().ForEach(elem => elem.interactable = value);
        }
    }
}