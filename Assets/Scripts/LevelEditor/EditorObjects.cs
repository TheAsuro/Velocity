using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorObjects : MonoBehaviour
{
	public static EditorObjects OBJ;

	public TextAsset objectExtentsFile;

	private GameObject mySun;
	private GameObject mySelectionPlane;
	private Vector2 mySelectionPlaneScale;

	private Dictionary<Vector3,GameObject> gridObjects;
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

		gridObjects = new Dictionary<Vector3,GameObject>();
	}

	void Start()
	{
		mySelectionPlaneScale = mySelectionPlane.renderer.material.GetTextureScale("_MainTex");
	}

	public Vector3[] GetObjectExtentsByName(string name)
	{
		string[] extentsFileLines = objectExtentsFile.text.Split('\n');
		foreach(string line in extentsFileLines)
		{
			if(line.StartsWith(name + "="))
			{
				return ParseObjectExtentsString(line.Substring(line.IndexOf("=") + 1));
			}
		}
		return null;
	}

	private Vector3[] ParseObjectExtentsString(string str)
	{
		string[] extentPositions = str.Split('|');
		
		int length = extentPositions.Length;
		Vector3[] extentsArray = new Vector3[length + 1];
		extentsArray[length] = Vector3.zero;

		for(int i = 0; i < length; i++)
		{
			extentsArray[i] = StringToVector(extentPositions[i]);
		}
		return extentsArray;
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

	public void AddObjectToGrid(GameObject obj)
	{
		gridObjects.Add(RoundVectorToGrid(obj.transform.position), obj);
	}

	public void RemoveObjectFromGrid(Vector3 pos)
	{
		Vector3 rPos = RoundVectorToGrid(pos);
		if(!gridObjects.Remove(rPos))
		{
			print("Failed to remove " + pos.ToString());
			print(rPos.y);
		}
	}

	public bool IsGridPositionFree(Vector3 position)
	{
		return !gridObjects.ContainsKey(RoundVectorToGrid(position));
	}

	public GameObject GetObjectAtGridPosition(Vector3 position)
	{
		return gridObjects[RoundVectorToGrid(position)];
	}

	private static Vector3 RoundVectorToGrid(Vector3 input)
	{

		return new Vector3(roundFloat(input.x), roundFloat(input.y), roundFloat(input.z));
	}

	private static int roundFloat(float input)
	{
		float temp = input;
		while(Mathf.Abs(temp) >= 1f)
		{
			temp -= Mathf.Sign(temp) * 1f;
		}
		
		if(temp > 0.99f)
		{
			return Mathf.RoundToInt(input) + 1;
		}
		return Mathf.RoundToInt(input);
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
}