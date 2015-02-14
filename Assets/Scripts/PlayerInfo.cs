using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
	//References
	private GameObject myMesh;
	private Canvas myCanvas;
	private RaceScript myRaceScript;
	private MouseLook myMouseLook;
	private Camera myCamera;
	private DemoRecord myRecorder;
	private Movement myMovement;
	private Image myCrosshair;
	private Image myCrosshairCircle;
	private Image myCrosshairCircle2;
	private Image myCrosshairCircle3;

	//Default values for movement variables
	//Speed, AirSpeed, MaxSpeed, Friction, Jump
	private static float[] defaults = { 200f, 200f, 6.4f, 0.6f, 8f, 5f };

	//Can player change cheat protected variables?
	private bool cheats = false;

	void Awake()
	{
		myMesh = transform.Find("Mesh").gameObject;
		myCanvas = transform.Find("Canvas").GetComponent<Canvas>();
		myRaceScript = myMesh.GetComponent<RaceScript>();
		myCamera = myMesh.transform.Find("Camera").gameObject.GetComponent<Camera>();
		myMouseLook = myCamera.gameObject.GetComponent<MouseLook>();
		myRecorder = myMesh.GetComponent<DemoRecord>();
		myMovement = myMesh.GetComponent<Movement>();
		myCrosshair = myCanvas.transform.Find("Crosshair").GetComponent<Image>();
		myCrosshairCircle = myCanvas.transform.Find("CrosshairCircle").GetComponent<Image>();
		myCrosshairCircle2 = myCanvas.transform.Find("CrosshairCircle2").GetComponent<Image>();
		myCrosshairCircle3 = myCanvas.transform.Find("CrosshairCircle3").GetComponent<Image>();
	}

	public void resetPosition(Vector3 pos, Quaternion rot)
	{
		transform.position = Vector3.zero;
		myMesh.transform.position = pos;
		myMesh.transform.rotation = rot;
	}

	public void prepareRace(float delay)
	{
		myRaceScript.prepareRace(delay);
	}

	public void playSound(AudioClip pClip)
	{
		myMesh.audio.clip = pClip;
		myMesh.audio.Play();
	}

	public string getCurrentSpeed()
	{
		return myMovement.getXzVelocityString();
	}

	public void setMouseSens(float sensitivity)
	{
		myMouseLook.sensitivityX = sensitivity;
		myMouseLook.sensitivityY = sensitivity;
	}

	public void setFov(float fov)
	{
		myCamera.fieldOfView = fov;
	}

	public void setVolume(float volume)
	{
		myMesh.audio.volume = volume;
	}

	public void startDemo(string playerName)
	{
		myRecorder.startDemo(playerName);
	}

	public void stopDemo()
	{
		myRecorder.stopDemo();
	}

	public void setMouseView(bool value)
	{
		myMouseLook.enabled = value;
	}

	public Vector3 getPosition()
	{
		return myMesh.transform.position;
	}

	public Demo getDemo()
	{
		return myRecorder.getDemo();
	}

	public void freeze()
	{
		myMovement.freeze();
	}

	public void unfreeze()
	{
		myMovement.unfreeze();
	}

	public void setFriction(float value)
	{
		if(cheats)
			myMovement.friction = value;
		else
			printCheatWarning();
	}

	public float getFriction()
	{
		return myMovement.friction;
	}

	public void setAcceleration(float value)
	{
		if(cheats)
			myMovement.accel = value;
		else
			printCheatWarning();
	}

	//Gets input multiplier, not current speed!
	public float getAcceleration()
	{
		return myMovement.accel;
	}

	public void setAirAcceleration(float value)
	{
		if(cheats)
			myMovement.airAccel = value;
		else
			printCheatWarning();
	}

	public float getAirAcceleration()
	{
		return myMovement.airAccel;
	}

	public void setMaxSpeed(float value)
	{
		if(cheats)
			myMovement.maxSpeed = value;
		else
			printCheatWarning();
	}

	public float getMaxSpeed()
	{
		return myMovement.maxSpeed;
	}

	public void setMaxAirSpeed(float value)
	{
		if(cheats)
			myMovement.maxAirSpeed = value;
		else
			printCheatWarning();
	}

	public float getMaxAirSpeed()
	{
		return myMovement.maxAirSpeed;
	}

	public void setJumpForce(float value)
	{
		if(cheats)
			myMovement.jumpForce = value;
		else
			printCheatWarning();
	}

	public float getJumpForce()
	{
		return myMovement.jumpForce;
	}

	public void setWorldBackgroundColor(Color color)
	{
		myCamera.backgroundColor = color;
	}

	public void setCrosshairColor(Color color)
	{
		myCrosshair.color = color;
		myCrosshairCircle.color = color;
		myCrosshairCircle2.color = color;
		myCrosshairCircle3.color = color;
	}

	public Image getCrosshairCircle()
	{
		return myCrosshairCircle;
	}

	public Image getCrosshairCircle2()
	{
		return myCrosshairCircle2;
	}

	public Image getCrosshairCircle3()
	{
		return myCrosshairCircle3;
	}

	public bool getCheats()
	{
		return cheats;
	}

	public void setCheats(bool value)
	{
		cheats = value;
		if(cheats)
		{
			GameInfo.info.invalidateRun();
		}
		else
		{
			resetCheatValues();
		}
	}

	public void resetCheatValues()
	{
		setAcceleration(defaults[0]);
		setAirAcceleration(defaults[1]);
		setMaxSpeed(defaults[2]);
		setMaxAirSpeed(defaults[3]);
		setFriction(defaults[4]);
		setJumpForce(defaults[5]);
	}

	public bool editorMode
	{
		set { myRaceScript.setEditorMode(value); }
		get { return myRaceScript.getEditorMode(); }
	}

	public bool invertYInput
	{
		set { myMouseLook.invertY = value; }
		get { return myMouseLook.invertY; }
	}

	private void printCheatWarning()
	{
		GameInfo.info.writeToConsole("This command is cheat protected, turn on cheats with 'cheats 1'!");
	}
}
