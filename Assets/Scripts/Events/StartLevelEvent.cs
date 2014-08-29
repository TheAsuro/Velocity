using UnityEngine;
using System.Collections;

public class StartLevelEvent : Event
{
	public float startFreeze = 0f;

	void Start()
	{
		WorldInfo.info.addResetMethod(reset, "start level reset");
		fire(null);
	}

	public override void fire(params object[] parameters)
	{
		GameInfo.info.getPlayerObject().GetComponent<RaceScript>().startRace(startFreeze);
	}

	public override void reset()
	{
		GameInfo.info.getPlayerObject().GetComponent<RaceScript>().startRace(startFreeze);
	}
}
