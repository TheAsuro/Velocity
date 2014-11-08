using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorObjects : MonoBehaviour
{
	public static EditorObjects OBJ;

	private GameObject mySun;
	private GameObject mySelectionPlane;

	private List<GameObject> addedObjects;

	void Awake()
	{
		if(OBJ == null)
		{
			OBJ = this;
		}

		addedObjects = new List<GameObject>();

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

	public void AddObject(GameObject obj)
	{
		addedObjects.Add(obj);
	}
}
