using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorObjects : MonoBehaviour
{
	public static EditorObjects OBJ;

	private GameObject mySun;
	private GameObject mySelectionPlane;

	private List<GameObject> loadedObjects;
	private List<GameObject> addedObjects;

	void Awake()
	{
		if(OBJ == null)
		{
			OBJ = this;
		}

		loadedObjects = new List<GameObject>();
		addedObjects = new List<GameObject>();

		foreach(Object go in Resources.LoadAll("EditorPrefabs"))
		{
			loadedObjects.Add((GameObject)go);
		}

		mySun = transform.Find("Sun").gameObject;
		mySelectionPlane = transform.Find("SelectionPlane").gameObject;
	}

	public void SetSunRotation(Quaternion rot)
	{
		mySun.transform.rotation = rot;
	}

	public void AddSelectionPlanePosition(Vector3 addVector)
	{
		mySelectionPlane.transform.position += addVector;
	}

	public List<string> GetAllPrefabNames()
	{
		List<string> nameList = new List<string>();

		foreach(GameObject go in loadedObjects)
		{
			nameList.Add(go.name);
		}

		return nameList;
	}

	public List<string> GetAllPrefabNames(string start)
	{
		List<string> nameList = new List<string>();

		foreach(GameObject go in loadedObjects)
		{
			if(go.name.StartsWith(start))
				nameList.Add(go.name);
		}

		return nameList;
	}

	public GameObject GetPrefabByName(string name)
	{
		foreach(GameObject go in loadedObjects)
		{
			if(go.name.Equals(name))
			{
				return go;
			}
		}

		return null;
	}

	public void AddObject(GameObject obj)
	{
		addedObjects.Add(obj);
	}
}
