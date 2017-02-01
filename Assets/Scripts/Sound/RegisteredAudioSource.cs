using UnityEngine;

namespace Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class RegisteredAudioSource : MonoBehaviour
    {
        private void Awake()
        {
            SoundManager.SingletonInstance.RegisterAudioSource(GetComponent<AudioSource>());
        }

        private void OnDestroy()
        {
            SoundManager.SingletonInstance.UnRegisterAudioSource(GetComponent<AudioSource>());
        }
    }
}