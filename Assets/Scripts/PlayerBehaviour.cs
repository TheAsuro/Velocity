using Demos;
using Movement;
using UnityEngine;
using UnityEngine.UI;

// TODO - this class is dumb delete it
public class PlayerBehaviour : MonoBehaviour
{
    //References
    private GameObject myMesh;

    private RaceScript myRaceScript;
    private MouseLook myMouseLook;
    private Camera myCamera;
    private DemoRecord myRecorder;

    //Default values for movement variables
    //Speed, AirSpeed, MaxSpeed, Friction, Jump
    private static float[] defaults = {200f, 200f, 6.4f, 0.6f, 8f, 5f};

    //Can player change cheat protected variables?
    private bool cheats = false;

    private void Awake()
    {
        myMesh = transform.Find("Mesh").gameObject;
        myRaceScript = myMesh.GetComponent<RaceScript>();
        myCamera = myMesh.transform.Find("Camera").gameObject.GetComponent<Camera>();
        myMouseLook = myCamera.gameObject.GetComponent<MouseLook>();
        myRecorder = myMesh.GetComponent<DemoRecord>();

        ApplySettings();
    }

    public void ApplySettings()
    {
        SetMouseSens(Settings.Game.MouseSpeed);
        InvertYInput = Settings.Game.InvertY;
        SetFov(Settings.Game.Fov);
        SetVolume(Settings.Game.Volume);
    }

    public void ResetPosition(Vector3 pos, Quaternion rot)
    {
        transform.position = Vector3.zero;
        myMesh.transform.position = pos;
        myMesh.transform.rotation = rot;
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

    public Demo GetDemo()
    {
        return myRecorder.GetDemo();
    }

    public void SetWorldBackgroundColor(Color color)
    {
        myCamera.backgroundColor = color;
    }

    public bool GetCheats()
    {
        return cheats;
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

    private void PrintCheatWarning()
    {
        Console.Console.Write("This command is cheat protected, turn on cheats with 'cheats 1'!");
    }
}