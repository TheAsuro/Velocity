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
		string myText = GetComponent<UnityEngine.UI.Text>().text;
		string myAuthor = "?";
		int mapIndex = menu.mapNames.IndexOf(myText);
		if(menu.mapAuthors.Count > mapIndex && mapIndex != -1)
		{
			myAuthor = menu.mapAuthors[mapIndex];
		}
		GameInfo.info.setSelectedMap(myText, myAuthor);
	}
}
