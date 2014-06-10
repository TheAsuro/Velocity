using UnityEngine;
using System.Collections;

public abstract class EventTrigger : MonoBehaviour
{
	public void fireEvent(Event eventComponent, params object[] parameters)
	{
		eventComponent.SendMessage("fire", parameters);
	}
}
