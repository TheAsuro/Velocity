using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorObjects : MonoBehaviour
{
	public static EditorObjects OBJ;

	public TextAsset objectInfoFile;

	private GameObject mySun;
	private GameObject mySelectionPlane;
	private BlockInfo bInfo;

	private Dictionary<Vector3,GameObject> gridObjects;
	private List<GameObject> nonGridObjects;
	private List<GameObject> loadedObjects;

	private GameObject startSpawn;

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
			if(go.GetType() == typeof(GameObject))
				loadedObjects.Add((GameObject)go);
		}

		gridObjects = new Dictionary<Vector3,GameObject>();
		nonGridObjects = new List<GameObject>();
		bInfo = new BlockInfo(objectInfoFile);
	}

	public Vector3[] GetObjectExtentsByName(string blockName)
	{
		return bInfo.GetBlockExtents(blockName);
	}

	public Vector3 GetObjectRotationByName(string blockName)
	{
		return bInfo.GetBlockRotation(blockName);
	}

	public Vector3 GetObjectOffsetByName(string blockName)
	{
		return bInfo.GetBlockOffset(blockName);
	}

	private Vector3 StringToVector(string input)
	{
		string[] inputValues = input.Split(',');
		float[] parsedValues = new float[3];
		for(int i = 0; i < 3; i++)
		{
			float val;
			if(float.TryParse(inputValues[i], out val))
			{
				parsedValues[i] = val;
			}
			else
			{
				return EditorInfo.NaV;
			}
		}

		return new Vector3(parsedValues[0], parsedValues[1], parsedValues[2]);
	}

	public GameObject GetStartSpawn()
	{
		return startSpawn;
	}

	private void SpecialObjectCheck(GameObject obj)
	{
		switch(obj.name)
		{
			case "Start":
				startSpawn = obj;
				break;
		}
	}

	private void RemoveSpecialObjectCheck(GameObject obj)
	{
		switch(obj.name)
		{
			case "Start":
				startSpawn = null;
				break;
		}
	}

	public void AddNonGridObject(GameObject obj)
	{
		nonGridObjects.Add(obj);
		SpecialObjectCheck(obj);
	}

	public void RemoveNonGridObject(GameObject obj)
	{
		nonGridObjects.Remove(obj);
		RemoveSpecialObjectCheck(obj);
	}

	public void AddObjectToGrid(GameObject obj)
	{
		Vector3 pos = RoundVectorToGrid(obj.transform.position);

		foreach(Vector3 addPos in GetObjectExtentsByName(obj.name))
		{
			gridObjects.Add(pos + RoundVectorToGrid(addPos), obj);
		}

		SpecialObjectCheck(obj);
	}

	public void RemoveObjectFromGrid(GameObject obj)
	{
		Vector3 pos = RoundVectorToGrid(obj.transform.position);

		foreach(Vector3 addPos in GetObjectExtentsByName(obj.name))
		{
			gridObjects.Remove(pos + RoundVectorToGrid(addPos));
		}

		RemoveSpecialObjectCheck(obj);
	}

	public bool IsGridPositionFree(Vector3 position)
	{
		return !gridObjects.ContainsKey(RoundVectorToGrid(position));
	}

	public GameObject GetObjectAtGridPosition(Vector3 position)
	{
		return gridObjects[RoundVectorToGrid(position)];
	}

	public static Vector3 RoundVectorToGrid(Vector3 input)
	{
		return new Vector3(Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y), Mathf.RoundToInt(input.z));
	}

	public static Vector3 RoundXZToGrid(Vector3 input)
	{
		return new Vector3(roundFloat(input.x), input.y, roundFloat(input.z));
	}

	private static int roundFloat(float input)
	{
		float temp = input;
		while(Mathf.Abs(temp) >= 1f)
		{
			temp -= Mathf.Sign(temp) * 1f;
		}
		
		if(Mathf.Abs(temp) > 0.99f)
		{
			return Mathf.RoundToInt(input) + 1;
		}

		return Mathf.RoundToInt(input);
	}

	//Saves the current level to a xml file
	public void SaveLevelToFile(string path)
	{
		List<ObjectData> allObj = new List<ObjectData>();
		foreach(GameObject obj in GetAllObjects())
		{
			ObjectData oData = new ObjectData();
			oData.name = obj.name;
			oData.position = obj.transform.localPosition;
			oData.rotation = obj.transform.localRotation;
			oData.scale = obj.transform.localScale;
			allObj.Add(oData);
		}
		LevelData data = new LevelData();
		data.levelObjects = allObj;
		data.WriteToFile(path);
	}

	//Rotate the sun (the directional light)
	public void SetSunRotation(Quaternion rot)
	{
		mySun.transform.rotation = rot;
	}

	//Move selection plane (currently just up/down)
	public void AddSelectionPlanePosition(Vector3 addVector)
	{
		mySelectionPlane.transform.position += addVector;
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

	public static List<Vector3> GetGridPositions(Vector3 start, Vector3 end)
	{
		Vector3 roundStart = RoundVectorToGrid(start);
		Vector3 roundEnd = RoundVectorToGrid(end);

		List<Vector3> temp = new List<Vector3>();

		if(roundStart.Equals(roundEnd))
		{
			temp.Add(roundStart);
			return temp;
		}

		for(float x = roundStart.x; x <= roundEnd.x; x++)
		{
			for(float y = roundStart.y; y <= roundEnd.y; y++)
			{
				for(float z = roundStart.z; z <= roundEnd.z; z++)
				{
					temp.Add(new Vector3(x,y,z));
				}
			}
		}

		return temp;
	}

	public List<GameObject> GetAllObjects()
	{
		List<GameObject> objs = nonGridObjects;
		foreach(GameObject obj in gridObjects.Values)
		{
			//Dont include duplicates created by extents
			if(!objs.Contains(obj))
				objs.Add(obj);
		}
		return objs;
	}

	public void SetSelectionPlaneVisibility(bool value)
	{
		mySelectionPlane.SetActive(value);
	}
}