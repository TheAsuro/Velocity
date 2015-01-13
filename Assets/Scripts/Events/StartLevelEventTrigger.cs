using UnityEngine;
using System.Collections;

public class StartLevelEventTrigger : EventTrigger
{
	void Start()
	{
		WorldInfo.info.addStartMethod(DoTrigger, "start level event trigger");
	}

	void DoTrigger()
	{
		foreach(Event e in eventComponent)
		{
			e.fire(null);
		}
	}
}
