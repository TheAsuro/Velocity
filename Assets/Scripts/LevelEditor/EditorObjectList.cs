using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EditorObjectList : MonoBehaviour
{
	private List<GameObject> currentCollectionObjects;
	private string currentCollectionName = "";
	private EditorObjectDisplay[] displays;
	private int pageNumber = 0;

	void Awake()
	{
		displays = new EditorObjectDisplay[6];

		for(int i = 0; i < displays.Length; i++)
		{
			displays[i] = transform.Find("Obj" + (i + 1).ToString()).GetComponent<EditorObjectDisplay>();
		}
	}

	public void ToggleToCollection(string collectionName)
	{
		pageNumber = 0;

		if(currentCollectionName == collectionName)
		{
			SetVisible(false);
			return;
		}

		SetVisible(true);

		currentCollectionName = collectionName;
		currentCollectionObjects = EditorObjects.OBJ.GetObjectGroupByName(collectionName);
		UpdateDisplays();
	}

	private void UpdateDisplays()
	{
		for(int i = 0; i < displays.Length; i++)
		{
			int itemPosition = i + pageNumber * 6;
			if (itemPosition < currentCollectionObjects.Count)
			{
				displays[i].SetObject(currentCollectionObjects[itemPosition]);
			}
			else
			{
				displays[i].SetText("");
				displays[i].SetImage(null);
			}	
		}
	}

	public void addPageNumber(int add)
	{
		if(currentCollectionObjects != null)
		{
			pageNumber += add;

			if(pageNumber < 0)
				pageNumber = 0;

			if(pageNumber * 6 >= currentCollectionObjects.Count)
			{
				pageNumber -= add;
			}
			else
			{
				UpdateDisplays();
			}
		}
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
}
