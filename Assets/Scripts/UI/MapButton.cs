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
		menu.setSelectedButton(transform.parent.gameObject);
		menu.setSelectedMap(GetComponent<UnityEngine.UI.Text>().text);
	}
}
