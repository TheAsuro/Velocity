using Settings;
using UnityEngine;
using Util;

namespace Game
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCam : MonoBehaviour
    {
        private void Awake()
        {
            SetFov(GameSettings.SingletonInstance.Fov);
            GameSettings.OnSettingsChanged += SetFov;
        }

        private void SetFov(object sender, EventArgs<GameSettings> settings)
        {
            SetFov(settings.Content.Fov);
        }

        private void SetFov(float fov)
        {
            GetComponent<Camera>().fieldOfView = fov;
        }

        private void OnDestroy()
        {
            GameSettings.OnSettingsChanged -= SetFov;
        }
    }
}