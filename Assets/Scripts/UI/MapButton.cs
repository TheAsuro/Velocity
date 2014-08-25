using UnityEngine;
using System.Collections;

public class MapButton : MonoBehaviour
{
	private MainMenu menu;

	public void setMainMenu(MainMenu pMenu)
	{
		menu = pMenu;
	}

	public void click()
	{
		menu.setSelectedMap(GetComponent<UnityEngine.UI.Text>().text);
	}
}
