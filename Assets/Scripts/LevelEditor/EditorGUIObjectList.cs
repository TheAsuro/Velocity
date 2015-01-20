using UnityEngine;
using System.Collections;

public class EditorGUIObjectList : MonoBehaviour
{
	private string currentCollectionName = "";

	public void ToggleToCollection(string collectionName)
	{
		if(currentCollectionName == collectionName)
		{
			SetVisible(false);
			return;
		}

		SetVisible(true);

		currentCollectionName = collectionName;
		SetVerticalPositionByCollectionName();

		//TODO do stuff
	}

	public void SetVisible(bool visible)
	{
		if(visible)
		{
			gameObject.SetActive(true);
		}
		else
		{
			gameObject.SetActive(false);
			currentCollectionName = "";
		}
	}

	private void SetVerticalPositionByCollectionName()
	{
		switch(currentCollectionName)
		{
			case "basic":
				SetVerticalSlot(0);
				break;
			case "ramps":
				SetVerticalSlot(1);
				break;
			case "special":
				SetVerticalSlot(2);
				break;
			case "trigger":
				SetVerticalSlot(3);
				break;
			case "custom":
				SetVerticalSlot(4);
				break;
			default:
				SetVerticalSlot(0);
				break;
		}
	}

	private void SetVerticalSlot(int slot)
	{
		//TODO fix this
		RectTransform rt = (RectTransform)gameObject.transform;
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -95f - slot * ((Screen.height - 40f) / 5f ));
	}
}
