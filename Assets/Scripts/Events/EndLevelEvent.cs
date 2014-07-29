using UnityEngine;
using System.Collections;

public class EndLevelEvent : Event
{
	public Vector3 relativeGoalPos;

	public override void fire(params object[] stuff)
	{
		PlayerEffects effects = GameInfo.info.getPlayerObject().GetComponent<PlayerEffects>();
		effects.movePlayerTowardsPosition(transform.position + relativeGoalPos, 1f);
		GameInfo.info.setMenuState(GameInfo.MenuState.endlevel);
	}

	public override void reset()
	{
		PlayerEffects effects = GameInfo.info.getPlayerObject().GetComponent<PlayerEffects>();
		effects.stopMoveToPos();
	}
}
