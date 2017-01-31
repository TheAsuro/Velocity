using Demos;
using Race;
using UnityEngine;

// TODO - this class is dumb delete it
public class PlayerBehaviour : MonoBehaviour
{
    //References
    private GameObject myMesh;

    private RaceScript myRaceScript;
    private MouseLook myMouseLook;
    private Camera myCamera;

    //Default values for movement variables
    //Speed, AirSpeed, MaxSpeed, Friction, Jump
    private static float[] defaults = {200f, 200f, 6.4f, 0.6f, 8f, 5f};

    private void Awake()
    {
        myMesh = transform.Find("Mesh").gameObject;
        myRaceScript = myMesh.GetComponent<RaceScript>();
        myCamera = myMesh.transform.Find("Camera").gameObject.GetComponent<Camera>();
        myMouseLook = myCamera.gameObject.GetComponent<MouseLook>();

        ApplySettings();
    }

    public void ApplySettings()
    {
        SetMouseSens(Settings.GameSettings.SingletonInstance.MouseSpeed);
        InvertYInput = Settings.GameSettings.SingletonInstance.InvertY.value;
        SetFov(Settings.GameSettings.SingletonInstance.Fov);
        SetVolume(Settings.GameSettings.SingletonInstance.Volume);
    }

    public void PlaySound(AudioClip pClip)
    {
        myMesh.GetComponent<AudioSource>().clip = pClip;
        myMesh.GetComponent<AudioSource>().Play();
    }

    public void SetMouseSens(float sensitivity)
    {
        myMouseLook.sensitivityX = sensitivity;
        myMouseLook.sensitivityY = sensitivity;
    }

    public void SetFov(float fov)
    {
        myCamera.fieldOfView = fov;
    }

    public void SetVolume(float volume)
    {
        myMesh.GetComponent<AudioSource>().volume = volume;
    }

    public void SetMouseView(bool value)
    {
        myMouseLook.enabled = value;
    }

    public Vector3 GetPosition()
    {
        return myMesh.transform.position;
    }

    public void SetPause(bool value)
    {
        if (value)
            myRaceScript.Pause();
        else
            myRaceScript.Unpause();
    }

    public bool InvertYInput
    {
        set { myMouseLook.invertY = value; }
        get { return myMouseLook.invertY; }
    }

    public Camera PlayerCamera
    {
        get { return myCamera; }
    }
}