using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EditorInfo : MonoBehaviour
{
	public LayerMask spawnLayers;
	public LayerMask removeLayers;
	public GameObject selectionBoxPrefab;

	private GameObject selectedPrefab;

	//GUI
	private Transform canvasT;
	private Transform topT;
	private InputField prefabText;
	private Toggle snapToggle;

	//Selection
	private GameObject selectionBox = null;
	//private Vector3 selectionStartPos = NaV; //unused now, may be useful for area spawning

	//Thing that tells you something went wrong
	public static Vector3 NaV = new Vector3(float.NaN, float.NaN, float.NaN);

	//Values set by GUI
	private bool snapToGrid = true;

	void Awake()
	{
		canvasT = transform.parent.Find("Canvas");
		topT = canvasT.Find("Top");
		prefabText = topT.Find("PrefabInput").GetComponent<InputField>();
		prefabText.onEndEdit.AddListener(PrefabSubmit);
		snapToggle = topT.Find("SnapToGrid").GetComponent<Toggle>();
	}

	void Update()
	{
		DrawSelectionBox();

		//Start selection with lmb
		if(Input.GetMouseButtonDown(0))
		{
			//selectionStartPos = GetMouseOnSelectionPlane();
		}

		//Spawn object when releasing lmb
		if(Input.GetMouseButtonUp(0))
		{
			//Spawn object at mouse release point
			Vector3 pos = GetMouseOnSelectionPlane();
			SpawnPrefab(selectedPrefab, pos, Quaternion.identity);
		}

		//Remove object with shift + lmb
		if(Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonUp(1))
		{
			RemoveSelectedObject();
		}

		//Quit with esc if in test mode
		if(Input.GetKey(KeyCode.Escape))
		{
			EndTest();
		}

		//Move spawn plane with scroll wheel
		float scrollCount = Input.GetAxis("Mouse ScrollWheel");
		EditorObjects.OBJ.AddSelectionPlanePosition(new Vector3(0f, scrollCount * 10f, 0f));
	}

	private void SetInterfaceActive(bool value)
	{
		canvasT.gameObject.SetActive(value);
		EditorObjects.OBJ.SetSelectionPlaneVisibility(value);
	}

	//Draws a box where the cursor is/a rectangle of the selected area
	private void DrawSelectionBox()
	{
		Vector3 selectionPos = GetMouseOnSelectionPlane();

		//Draw box only when cursor is on a valid square, rmb and shift are not pressed and snap to grid
		if(!selectionPos.Equals(NaV) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftShift) && snapToGrid)
		{
			Vector3 roundedSelectionPos = EditorObjects.RoundVectorToGrid(selectionPos) + new Vector3(0f, 0.5f, 0f);

			if(selectionBox == null)
			{
				selectionBox = (GameObject)GameObject.Instantiate(selectionBoxPrefab, roundedSelectionPos, Quaternion.identity);
			}
			else
			{
				selectionBox.transform.position = roundedSelectionPos;
			}
		}
		else
		{
			GameObject.Destroy(selectionBox);
			selectionBox = null;
		}
	}

	//Destroys the object the mouse is over
	private void RemoveSelectedObject()
	{
		RaycastHit hitInfo;
		bool hit = MouseRaycast(removeLayers, out hitInfo);

		if(hit)
		{
			Transform transformToDelete = hitInfo.collider.gameObject.transform;
			while(transformToDelete.parent != null)
			{
				transformToDelete = transformToDelete.parent;
			}
			
			EditorObjects.OBJ.RemoveObjectFromGrid(transformToDelete.gameObject);
			EditorObjects.OBJ.RemoveNonGridObject(transformToDelete.gameObject);
			DestroyRecursive(transformToDelete.gameObject);
		}
	}

	//Destroys all children of an object
	private void DestroyRecursive(GameObject go)
	{
		foreach(Transform childTransform in go.transform)
		{
			DestroyRecursive(childTransform.gameObject);
		}
		GameObject.Destroy(go);
	}

	//Called when player submits the selection textbox
	private void PrefabSubmit(string sub)
	{
		SelectPrefab(sub);
	}

	//Toggles snap on/off depending on gui button
	public void UpdateSnap()
	{
		if(snapToggle != null)
		{
			snapToGrid = snapToggle.isOn;
		}
	}

	//Start playing the level
	public void TestLevel()
	{
		GameObject startSpawn = EditorObjects.OBJ.GetStartSpawn();
		if(startSpawn != null)
		{
			GameInfo.info.spawnNewPlayer(startSpawn.GetComponent<Respawn>(), true, true);
			GameInfo.info.getPlayerInfo().startRace(1f);
		}
		SetInterfaceActive(false);
	}

	//Stop playing the level
	public void EndTest()
	{
		GameInfo.info.setPlayerInfo(null);
		GameInfo.info.cleanUpPlayer();
		SetInterfaceActive(true);
	}

	//Selects the prefab by name
	private void SelectPrefab(string name)
	{
		selectedPrefab = EditorObjects.OBJ.GetPrefabByName(name);
	}

	//Instantiates a new prefab
	private void SpawnPrefab(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		//Check if we have a valid position and object
		if(!pos.Equals(NaV) && prefab != null)
		{
			Vector3 newPos = pos;

			//If object has a collider, align it to ground
			if(prefab.collider != null)
			{
				float additionalHeight = EditorObjects.OBJ.GetObjectHeightByName(prefab.name);
				newPos = new Vector3(pos.x, pos.y + additionalHeight, pos.z);
			}

			//Round position to grid
			if(snapToGrid)
			{
				newPos = EditorObjects.RoundXZToGrid(newPos);
			}

			//The extents of the prefab will prevent placement of other blocks
			Vector3[] prefabExtents = EditorObjects.OBJ.GetObjectExtentsByName(prefab.name);
			foreach(Vector3 ex in prefabExtents)
			{
				Vector3 actualPos = newPos + ex;
				if(!EditorObjects.OBJ.IsGridPositionFree(actualPos))
					return;
			}
			
			//Create the object
			GameObject instance = (GameObject)GameObject.Instantiate(prefab, newPos, rot);
			
			//Set the name to the prefab's name so we know from wich one we created it
			instance.name = prefab.name;

			//Add to the list of objects in grid
			if(snapToGrid)
			{
				EditorObjects.OBJ.AddObjectToGrid(instance);
			}
			else
			{
				EditorObjects.OBJ.AddNonGridObject(instance);
			}
		}
	}

	//Get the position the mouse is over
	private Vector3 GetMouseOnSelectionPlane()
	{
		RaycastHit hitInfo;
		bool hit = MouseRaycast(spawnLayers, out hitInfo);
		if(!hit)
			return NaV;

		return hitInfo.point;
	}

	private Vector3 GetMouseOnSelectionPlane(LayerMask layers)
	{
		RaycastHit hitInfo;
		bool hit = MouseRaycast(layers, out hitInfo);
		if(!hit)
			return NaV;

		return hitInfo.point;
	}

	//Casts a ray from the camera into the direction of the mouse cursor
	private bool MouseRaycast(LayerMask layers, out RaycastHit hit, float length = Mathf.Infinity)
	{
		Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane);
		Vector3 worldPos = camera.ScreenToWorldPoint(mousePos);
		Vector3 camPos = camera.transform.position;
		Vector3 rayDirection = worldPos - camPos;
		Ray clickRay = new Ray(camPos, rayDirection);
		return Physics.Raycast(clickRay, out hit, length, layers);
	}

	public void ExitEditor()
	{
		Application.LoadLevel("MainMenu");
	}

	private Vector3 SimpleVectorMultiply(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}
}
