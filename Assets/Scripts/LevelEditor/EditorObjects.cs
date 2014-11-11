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

	void Start()
	{
		mySelectionPlaneScale = mySelectionPlane.renderer.material.GetTextureScale("_MainTex");
	}

	public void SetSunRotation(Quaternion rot)
	{
		mySun.transform.rotation = rot;
	}

	public void AddSelectionPlanePosition(Vector3 addVector)
	{
		mySelectionPlane.transform.position += addVector;
	}

	public void SetSelectionPlaneRelativeMaterialScale(float scale)
	{
		Vector2 realScale = new Vector2(scale * mySelectionPlaneScale.x, scale * mySelectionPlaneScale.y);
		mySelectionPlane.renderer.material.SetTextureScale("_MainTex", realScale);
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
