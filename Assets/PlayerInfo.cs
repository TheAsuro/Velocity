using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{
	private GameObject myMesh;
	private RaceScript myRaceScript;
	private MouseLook myMouseLook;
	private Camera myCamera;
	private DemoRecord myRecorder;

	void Awake()
	{
		myMesh = transform.Find("Mesh").gameObject;
		myRaceScript = myMesh.GetComponent<RaceScript>();
		myCamera = myMesh.transform.Find("Camera").gameObject.GetComponent<Camera>();
		myMouseLook = myCamera.gameObject.GetComponent<MouseLook>();
		myRecorder = myMesh.GetComponent<DemoRecord>();
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
}
