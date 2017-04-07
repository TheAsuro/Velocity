using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MenuWindows
{
    public class LoadWindow : DefaultMenuWindow
    {
        public override void OnActivate()
        {
            base.OnActivate();
            StartCoroutine(Fade(Color.clear, Color.white, 0.1f));
        }

        public override void OnClose()
        {
            base.OnClose();
            StartCoroutine(Fade(Color.white, Color.clear, 0.1f));
        }

        private IEnumerator Fade(Color start, Color end, float duration)
        {
            float startTime = Time.unscaledTime;
            float completion = 0f;
            Image image = GetComponent<Image>();
            while (completion < 1f)
            {
                completion = (Time.unscaledTime - startTime) / duration;
                image.color = Color.Lerp(start, end, completion);
                yield return false;
            }
        }
    }
}