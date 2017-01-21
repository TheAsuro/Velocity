using Demos;
using Movement;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBehaviour : MonoBehaviour
{
    //References
    private GameObject myMesh;

    private Canvas myCanvas;
    private RaceScript myRaceScript;
    private MouseLook myMouseLook;
    private Camera myCamera;
    private DemoRecord myRecorder;
    private MovementBehaviour myMovement;
    private Image myCrosshair;

    //Default values for movement variables
    //Speed, AirSpeed, MaxSpeed, Friction, Jump
    private static float[] defaults = {200f, 200f, 6.4f, 0.6f, 8f, 5f};

    //Can player change cheat protected variables?
    private bool cheats = false;

    private void Awake()
    {
        myMesh = transform.Find("Mesh").gameObject;
        myCanvas = transform.Find("Canvas").GetComponent<Canvas>();
        myRaceScript = myMesh.GetComponent<RaceScript>();
        myCamera = myMesh.transform.Find("Camera").gameObject.GetComponent<Camera>();
        myMouseLook = myCamera.gameObject.GetComponent<MouseLook>();
        myRecorder = myMesh.GetComponent<DemoRecord>();
        myMovement = myMesh.GetComponent<MovementBehaviour>();
        myCrosshair = myCanvas.transform.Find("CrosshairCircle").GetComponent<Image>();

        ApplySettings();
    }

    private void Update()
    {
        myCrosshair.material.SetFloat("_Speed", myMovement.XzVelocity);
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

    public void PrepareRace(float delay)
    {
        myRaceScript.PrepareRace(delay);
    }

    public void PlaySound(AudioClip pClip)
    {
        myMesh.GetComponent<AudioSource>().clip = pClip;
        myMesh.GetComponent<AudioSource>().Play();
    }

    public string GetCurrentSpeed()
    {
        return myMovement.GetXzVelocityString();
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

    public void StartRecording(string playerName)
    {
        myRecorder.StartDemo(playerName);
    }

    public void StopRecording()
    {
        myRecorder.StopDemo();
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

    public void Freeze()
    {
        myMovement.Freeze();
    }

    public void Unfreeze()
    {
        myMovement.Unfreeze();
    }

    public void SetFriction(float value)
    {
        if (cheats)
            myMovement.friction = value;
        else
            PrintCheatWarning();
    }

    public float GetFriction()
    {
        return myMovement.friction;
    }

    public void SetAcceleration(float value)
    {
        if (cheats)
            myMovement.accel = value;
        else
            PrintCheatWarning();
    }

    //Gets input multiplier, not current speed!
    public float GetAcceleration()
    {
        return myMovement.accel;
    }

    public void SetAirAcceleration(float value)
    {
        if (cheats)
            myMovement.airAccel = value;
        else
            PrintCheatWarning();
    }

    public float GetAirAcceleration()
    {
        return myMovement.airAccel;
    }

    public void SetMaxSpeed(float value)
    {
        if (cheats)
            myMovement.maxSpeed = value;
        else
            PrintCheatWarning();
    }

    public float GetMaxSpeed()
    {
        return myMovement.maxSpeed;
    }

    public void SetMaxAirSpeed(float value)
    {
        if (cheats)
            myMovement.maxAirSpeed = value;
        else
            PrintCheatWarning();
    }

    public float GetMaxAirSpeed()
    {
        return myMovement.maxAirSpeed;
    }

    public void SetJumpForce(float value)
    {
        if (cheats)
            myMovement.jumpForce = value;
        else
            PrintCheatWarning();
    }

    public float GetJumpForce()
    {
        return myMovement.jumpForce;
    }

    public void SetWorldBackgroundColor(Color color)
    {
        myCamera.backgroundColor = color;
    }

    public bool GetCheats()
    {
        return cheats;
    }

    public void SetCheats(bool value)
    {
        cheats = value;
        if (cheats)
        {
            GameInfo.info.InvalidateRun("Activated cheats");
        }
        else
        {
            ResetCheatValues();
        }
    }

    public void ResetCheatValues()
    {
        SetAcceleration(defaults[0]);
        SetAirAcceleration(defaults[1]);
        SetMaxSpeed(defaults[2]);
        SetMaxAirSpeed(defaults[3]);
        SetFriction(defaults[4]);
        SetJumpForce(defaults[5]);
    }

    public bool EditorMode
    {
        set { myRaceScript.SetEditorMode(value); }
        get { return myRaceScript.GetEditorMode(); }
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

    public bool CanvasEnabled
    {
        get
        {
            return myCanvas != null && myCanvas.enabled;
        }
        set
        {
            if (myCanvas == null)
            {
                return;
            }
            myCanvas.enabled = value;
        }
    }

    private void PrintCheatWarning()
    {
        Console.Write("This command is cheat protected, turn on cheats with 'cheats 1'!");
    }

    public void SetNoclip(bool value)
    {
        if (cheats || value == false)
            myMovement.Noclip = value;
        else
            PrintCheatWarning();
    }

    public bool GetNoclip()
    {
        return myMovement.Noclip;
    }
}