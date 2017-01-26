using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCam : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Camera>().fieldOfView = Settings.Game.Fov;
        }
    }
}