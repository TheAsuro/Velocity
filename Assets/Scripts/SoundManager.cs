using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager SingletonInstance { get; private set; }

    private AudioSource soundPlayer;

    private void Awake()
    {
        if (SingletonInstance != null)
            throw new InvalidOperationException();

        SingletonInstance = this;
        soundPlayer = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        SingletonInstance = null;
    }

    public void PlaySound(string clipName)
    {
        // TODO
    }

    public void PlaySound(AudioClip clip)
    {
        soundPlayer.clip = clip;
        soundPlayer.Play();
    }
}