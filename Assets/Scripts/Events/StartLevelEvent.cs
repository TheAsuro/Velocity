using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartLevelEvent : Event
{
	public float introLength = 0f;
	public float startFreeze = 0f;
	public GameObject introDisplay;

	private float introStartTime;
	private bool waitForIntro = false;

	void Start()
	{
		WorldInfo.info.addResetMethod(reset, "start level reset");
		fire(null);
	}

	public override void fire(params object[] parameters)
	{
		if(introLength > 0f)
		{
			introStartTime = Time.time;
			waitForIntro = true;
			introDisplay.SetActive(true);
			GameInfo.info.getPlayerObject().GetComponent<RaceScript>().startRace(startFreeze + introLength);
		}
		else
		{
			GameInfo.info.getPlayerObject().GetComponent<RaceScript>().startRace(startFreeze);
		}
	}

	void Update()
	{
		if(waitForIntro && Time.time > introStartTime + introLength)
		{
			waitForIntro = false;
			introDisplay.SetActive(false);
		}
	}

	public override void reset()
	{
		GameInfo.info.getPlayerObject().GetComponent<RaceScript>().startRace(startFreeze);
	}
}
