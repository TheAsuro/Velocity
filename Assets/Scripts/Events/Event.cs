using UnityEngine;
using System.Collections;

public abstract class Event : MonoBehaviour
{
	public virtual void fire(params object[] parameters)
	{

	}

	public virtual void reset()
	{
		
	}
}