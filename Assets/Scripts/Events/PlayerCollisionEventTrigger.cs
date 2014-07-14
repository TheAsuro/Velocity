using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCollisionEventTrigger : EventTrigger
{
	public bool addToResetList = true;
	public List<Event> eventComponent;

	void Start()
	{
		if(addToResetList)
		{
			WorldInfo.info.addResetMethod(reset, "reset PlayerCollisionEventTrigger " + GetInstanceID().ToString());
		}
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.tag.Equals("Player"))
		{
			foreach(Event eComp in eventComponent)
			{
				fireEvent(eComp, new object[0]);
			}
		}
	}
}
