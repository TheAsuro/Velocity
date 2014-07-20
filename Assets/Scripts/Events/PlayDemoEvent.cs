using UnityEngine;
using System.Collections;

public class PlayDemoEvent : Event
{
	public override void fire(params object[] parameters)
	{
		GameInfo.info.saveLastDemo();
		GameInfo.info.playLastDemo();
	}
}
