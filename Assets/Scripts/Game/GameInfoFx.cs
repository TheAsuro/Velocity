using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    internal class GameInfoFx
    {
        public delegate void Callback();

        private delegate void EffectUpdate(Effect effect);

        private Image effectImage;

        private List<Effect> activeEffects;

        private struct Effect
        {
            public float startTime;
            public float duration;
            public EffectUpdate update;
            public Callback callback;
            public Color startColor;
            public Color endColor;
        }

        public GameInfoFx(Image image)
        {
            activeEffects = new List<Effect>();

            effectImage = image;
        }

        //Call this in the script's update function
        public void Update()
        {
            //Go through active effects and update them
            for (int i = 0; i < activeEffects.Count; i++)
            {
                activeEffects[i].update(activeEffects[i]);
            }

            Settings.Input.ExecuteBoundActions();
        }

        public void StartColorFade(Color start, Color end, float duration, Callback callback = null)
        {
            Effect e = new Effect
            {
                startTime = Time.unscaledTime,
                duration = duration,
                startColor = start,
                endColor = end,
                update = FadeToColor,
                callback = callback
            };
            activeEffects.Add(e);
        }

        private void FadeToColor(Effect effect)
        {
            //Fade
            float progress = Interpolate(effect.startTime, Time.unscaledTime, effect.startTime + effect.duration);
            effectImage.color = Color.Lerp(effect.startColor, effect.endColor, progress);

            //Check if we are done
            if (progress >= 1f)
            {
                if (effect.callback != null)
                    effect.callback();
                activeEffects.Remove(effect);
            }
        }

        //Returns 0 if current == start; returns 1 if current == end
        private static float Interpolate(float start, float current, float end)
        {
            return (current - start) / (end - start);
        }
    }
}