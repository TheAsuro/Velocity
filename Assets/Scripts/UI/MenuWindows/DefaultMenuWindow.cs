using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class DefaultMenuWindow : MonoBehaviour, MenuWindow
    {
        private bool active = true;
        private Coroutine activeChecker;

        public virtual void OnActivate()
        {
            SetInteractive(true);
            activeChecker = StartCoroutine(ActiveChecker());
        }

        public virtual void OnSetAsBackground()
        {
            SetInteractive(false);
        }

        public virtual void OnClose()
        {
            StopCoroutine(activeChecker);
            Destroy(gameObject);
        }

        private IEnumerator ActiveChecker()
        {
            while (true)
            {
                transform.GetComponentsInChildren<Selectable>().ToList().ForEach(elem => elem.interactable = active);
                yield return null;
            }
        }

        protected void SetInteractive(bool value)
        {
            active = value;
        }
    }
}