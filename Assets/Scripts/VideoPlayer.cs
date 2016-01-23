using UnityEngine;

public class VideoPlayer : MonoBehaviour
{
    private MovieTexture MTex { get { return (MovieTexture)GetComponent<Renderer>().material.mainTexture; } }

    void Awake()
    {
        MTex.Play();
    }
}
