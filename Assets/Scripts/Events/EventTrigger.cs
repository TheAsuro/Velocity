using UnityEngine;
using System.Collections.Generic;

public abstract class EventTrigger : MonoBehaviour
{
	public List<Event> eventComponent;

	private Event tempEventComponent;
	private object[] tempParameters;

	public void fireEvent(Event eventComponent, params object[] parameters)
	{
		eventComponent.SendMessage("fire", parameters);
	}
}
