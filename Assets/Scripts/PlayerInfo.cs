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

	//Default values for movement variables
	//Speed, AirSpeed, MaxSpeed, Friction, Jump
	private static float[] defaults = { 5f, 10f, 7f, 0.9f, 5f };

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
	}

	public void resetPosition(Vector3 pos, Quaternion rot)
	{
		transform.position = Vector3.zero;
		myMesh.transform.position = pos;
		myMesh.transform.rotation = rot;
	}

	public void startRace(float delay)
	{
		myRaceScript.startRace(delay);
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

	public void setFrictionMultiplier(float value)
	{
		myMovement.frictionMultiplier = value;
		GameInfo.info.invalidateRun();
	}

	public float getFrictionMultiplier()
	{
		return myMovement.frictionMultiplier;
	}

	public void setSpeed(float value)
	{
		myMovement.speed = value;
		GameInfo.info.invalidateRun();
	}

	//Gets input multiplier, not current speed!
	public float getSpeed()
	{
		return myMovement.speed;
	}

	public void setAirSpeed(float value)
	{
		myMovement.airSpeed = value;
		GameInfo.info.invalidateRun();
	}

	public float getAirSpeed()
	{
		return myMovement.airSpeed;
	}

	public void setMaxSpeed(float value)
	{
		myMovement.maxSpeed = value;
		GameInfo.info.invalidateRun();
	}

	public float getMaxSpeed()
	{
		return myMovement.maxSpeed;
	}

	public void setJumpForce(float value)
	{
		myMovement.jumpForce = value;
		GameInfo.info.invalidateRun();
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
	}

	public Image getCrosshairCircle()
	{
		return myCrosshairCircle;
	}

	public Image getCrosshairCircle2()
	{
		return myCrosshairCircle2;
	}

	public bool validatePlayerVariables()
	{
		float[] currentValues = { getSpeed(), getAirSpeed(), getMaxSpeed(), getFrictionMultiplier(), getJumpForce() };
		for(int i = 0; i < 5; i++)
		{
			if(defaults[i] != currentValues[i])
				return false;
		}
		return true;
	}

	public void enableEditorMode()
	{
		myRaceScript.enableEditorMode();
	}
}
