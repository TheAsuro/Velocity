using UnityEngine;
using System.Collections;

public class StartLevelEventTrigger : EventTrigger
{
	void Start()
	{
		foreach(Event e in eventComponent)
		{
			e.fire(null);
		}
	}
}
