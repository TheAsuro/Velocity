using UnityEngine;
using System.Collections;

public class StrafeTutorial : MonoBehaviour
{
	private Movement movementScript;
	private string state = "start";

	void Start()
	{
		movementScript = GameInfo.info.getPlayerObject().GetComponent<Movement>();
		nextState();
	}

	private void nextState()
	{
		switch(state)
		{
			case "start":
				playWatchDemo();
				state = "watch";
				break;
			case "watch":
				playSimpleControls();
				state = "simple";
				break;
			case "simple":
				playAdvancedControls();
				state = "advanced";
				break;
			case "advanced":
				playFullControls();
				state = "full";
				break;
		}
	}

	private void playWatchDemo()
	{
		//Watch only
		movementScript.setPlayerControls(false,false,false,false,false,false,false);
	}

	private void playSimpleControls()
	{
		//Move left and right with automatic air-strafing
		movementScript.setPlayerControls(false,false,false,false,true,false,false);
	}

	private void playAdvancedControls()
	{
		//Move left and right with manual air-strafing
		movementScript.setPlayerControls(false,false,false,false,true,false,true);
	}

	private void playFullControls()
	{
		//Full control
		movementScript.setPlayerControls(true,true,true,true,true,true,true);
	}
}
