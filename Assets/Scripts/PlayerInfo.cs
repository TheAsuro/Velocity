using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
	private GameObject myMesh;
	private RaceScript myRaceScript;
	private MouseLook myMouseLook;
	private Camera myCamera;
	private DemoRecord myRecorder;
	private Movement myMovement;

	void Awake()
	{
		myMesh = transform.Find("Mesh").gameObject;
		myRaceScript = myMesh.GetComponent<RaceScript>();
		myCamera = myMesh.transform.Find("Camera").gameObject.GetComponent<Camera>();
		myMouseLook = myCamera.gameObject.GetComponent<MouseLook>();
		myRecorder = myMesh.GetComponent<DemoRecord>();
		myMovement = myMesh.GetComponent<Movement>();
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
	}

	public float getFrictionMultiplier()
	{
		return myMovement.frictionMultiplier;
	}

	public void setSpeed(float value)
	{
		myMovement.speed = value;
	}

	//Gets input multiplier, not current speed!
	public float getSpeed()
	{
		return myMovement.speed;
	}

	public void setAirSpeed(float value)
	{
		myMovement.airSpeed = value;
	}

	public float getAirSpeed()
	{
		return myMovement.airSpeed;
	}

	public void setMaxSpeed(float value)
	{
		myMovement.maxSpeed = value;
	}

	public float getMaxSpeed()
	{
		return myMovement.maxSpeed;
	}

	public void setJumpForce(float value)
	{
		myMovement.jumpForce = value;
	}

	public float getJumpForce()
	{
		return myMovement.jumpForce;
	}

	public void setWorldBackgroundColor(Color color)
	{
		myCamera.backgroundColor = color;
	}
}
