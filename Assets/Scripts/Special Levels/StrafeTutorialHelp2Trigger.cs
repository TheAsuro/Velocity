using UnityEngine;
using System.Collections;

public class StrafeTutorialHelp2Trigger : Event
{
	public override void fire(params object[] parameters)
	{
		print("<play ghost here>");
	}

	public override void reset()
	{
		print("<reset ghost here>");
	}
}
