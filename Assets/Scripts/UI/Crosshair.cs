using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour
{
	public Texture2D crosshair;

	void OnGUI()
	{
		if(GameInfo.info.getMenuState() == GameInfo.MenuState.closed)
		{
			Rect pos = new Rect(Screen.width / 2f - crosshair.width / 2f, Screen.height / 2f - crosshair.height / 2f, crosshair.width, crosshair.height);
			GUI.DrawTexture(pos, crosshair);
		}
	}
}
