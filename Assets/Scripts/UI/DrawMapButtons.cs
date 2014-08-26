using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawMapButtons : MonoBehaviour
{
	public GameObject buttonPrefab;
	public Sprite selectedSprite;
	public Sprite unselectedSprite;

	private List<GameObject> myButtons = new List<GameObject>();

	public void addButtons(MainMenu sender, List<string> mapNames)
	{
		int index = 0;

		for(int i = 0; i < 3; i++)
		{
			for(int j = 0; j < 3; j++)
			{
				//Instantiate button and save a reference
				GameObject newButton = (GameObject)GameObject.Instantiate(buttonPrefab);
				myButtons.Add(newButton);

				//Move button to desired position
				RectTransform newTransform = (RectTransform)newButton.transform;
				newTransform.parent = gameObject.transform;
				newTransform.anchoredPosition = new Vector2(80f + 155f * i, -40f - 75f * j);

				//Change button text
				string mapName = "";
				if(mapNames.Count > index) { mapName = mapNames[index]; }
				newButton.transform.Find("Text").gameObject.GetComponent<UnityEngine.UI.Text>().text = mapName;
				newButton.transform.Find("Text").gameObject.GetComponent<MapButton>().setMainMenu(sender);

				index++;
			}
		}
	}

	public void clearButtons()
	{
		foreach(GameObject button in myButtons)
		{
			GameObject.Destroy(button);
		}
		myButtons.Clear();
	}

	public void setSelectedButton(GameObject button)
	{
		foreach(GameObject otherButton in myButtons)
		{
			otherButton.GetComponent<UnityEngine.UI.Image>().overrideSprite = unselectedSprite;
		}
		button.GetComponent<UnityEngine.UI.Image>().overrideSprite = selectedSprite;
	}
}
