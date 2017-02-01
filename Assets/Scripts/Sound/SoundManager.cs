using UnityEngine;

namespace Sound
{
    public class SoundManager
    {
        private static SoundManager singletonInstance;

        public static SoundManager SingletonInstance
        {
            get
            {
                if (singletonInstance == null)
                    singletonInstance = new SoundManager();
                return singletonInstance;
            }
        }

        public float Volume
        {
            get
            {
                Debug.Assert(audioSource != null);
                return audioSource.volume;
            }
            set
            {
                Debug.Assert(audioSource != null);
                audioSource.volume = value;
            }
        }

        // Assumption: there should at all times be a registered audioSource
        private AudioSource audioSource;

        public void RegisterAudioSource(AudioSource audioSource)
        {
            this.audioSource = audioSource;
        }

        public void UnRegisterAudioSource(AudioSource source)
        {
            Debug.Assert(audioSource == source);
            audioSource = null;
        }

        public void PlaySound(AudioClip sound)
        {
            Debug.Assert(audioSource != null);
            audioSource.clip = sound;
            audioSource.Play();
        }
    }
}