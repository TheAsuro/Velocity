using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorObjects : MonoBehaviour
{
	public static EditorObjects OBJ;

	private GameObject mySun;
	private GameObject mySelectionPlane;
	private Vector2 mySelectionPlaneScale;

	private List<GameObject> loadedObjects;

	void Awake()
	{
		//Make this the public instance of this class
		if(OBJ == null)
		{
			OBJ = this;
		}

		//Find child objects
		mySun = transform.Find("Sun").gameObject;
		mySelectionPlane = transform.Find("SelectionPlane").gameObject;

		//Load all prefabs into a list
		loadedObjects = new List<GameObject>();

		foreach(Object go in Resources.LoadAll("EditorPrefabs"))
		{
			loadedObjects.Add((GameObject)go);
		}
	}

	void Start()
	{
		mySelectionPlaneScale = mySelectionPlane.renderer.material.GetTextureScale("_MainTex");
	}

	//Rotate the sun (the only directional light)
	public void SetSunRotation(Quaternion rot)
	{
		mySun.transform.rotation = rot;
	}

	//Move selection plane [up or down]
	public void AddSelectionPlanePosition(Vector3 addVector)
	{
		mySelectionPlane.transform.position += addVector;
	}

	//Scale the material of the selection plane
	public void SetSelectionPlaneRelativeMaterialScale(float scale)
	{
		Vector2 realScale = new Vector2(scale * mySelectionPlaneScale.x, scale * mySelectionPlaneScale.y);
		mySelectionPlane.renderer.material.SetTextureScale("_MainTex", realScale);
	}

	//Returns a list of all loadable prefabs
	public List<string> GetAllPrefabNames()
	{
		List<string> nameList = new List<string>();

		foreach(GameObject go in loadedObjects)
		{
			nameList.Add(go.name);
		}

		return nameList;
	}

	//Returns a list of all loadable prefabs that start with the given string
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

	//Return the prefab that has this name
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
}
