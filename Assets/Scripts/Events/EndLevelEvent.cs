using UnityEngine;
using System.Collections;

public class EndLevelEvent : Event
{
	public Vector3 relativeGoalPos;

	public override void fire(params object[] stuff)
	{
		GameInfo.info.levelFinished();
	}

	public override void reset()
	{
		
	}
}
