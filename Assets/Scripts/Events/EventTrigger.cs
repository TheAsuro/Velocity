using UnityEngine;
using System.Collections;

public abstract class EventTrigger : MonoBehaviour
{
	public float delay = 0f;

	private float delayEnd = -1f; //values < 0 decativate delayed execution
	private Event tempEventComponent;
	private object[] tempParameters;

	public void fireEvent(Event eventComponent, params object[] parameters)
	{
		if(delay == 0f)
		{
			eventComponent.SendMessage("fire", parameters);
		}
		else
		{
			delayEnd = Time.time + delay;
			tempEventComponent = eventComponent;
			tempParameters = parameters;
		}
	}

	public void reset()
	{
		delay = -1f;
	}

	void Update()
	{
		if(delayEnd > 0f && Time.time >= delayEnd)
		{
			tempEventComponent.SendMessage("fire", tempParameters);
			delayEnd = -1f;
		}
	}
}
