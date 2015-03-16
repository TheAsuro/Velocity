using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct previewImageElement
{
	public string name;
	public Texture2D image;
}

public class EditorInfo : MonoBehaviour
{
	public static EditorInfo info;

	public LayerMask spawnLayers;
	public LayerMask removeLayers;
	public GameObject selectionBoxPrefab;
	public previewImageElement[] previewImages;

	private GameObject selectedPrefab;

	//GUI
	private Transform canvasT;
	private Transform topT;
	private InputField prefabText;
	private InputField levelNameText;
	private Toggle snapToggle;
	private EventSystem eSys;
    private FileSelection fileSel;

	//Selection
	private GameObject selectionBox = null;
	private int rotationDirection = 0; //0 = positive z (forward), 1 = positive x (right), 2 = negative z (back), 3 = negative x (left)

	//Thing that tells you something went wrong
	public static Vector3 NaV = new Vector3(float.NaN, float.NaN, float.NaN);

	//Values set by GUI
	private bool snapToGrid = true;

	void Awake()
	{
		if(EditorInfo.info == null)
		{
			info = this;
		}
		else
		{
			Destroy(gameObject);
		}

		canvasT = transform.parent.Find("Canvas");
		topT = canvasT.Find("Top");
		prefabText = topT.Find("PrefabInput").GetComponent<InputField>();
		prefabText.onEndEdit.AddListener(PrefabSubmit);
		levelNameText = topT.Find("LevelNameInput").GetComponent<InputField>();
		snapToggle = topT.Find("SnapToGrid").GetComponent<Toggle>();
		eSys = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        fileSel = canvasT.Find("FileSelection").GetComponent<FileSelection>();
	}

    void Start()
    {
        //If there is a preset map in gameinfo, load this map now
        if (!GameInfo.info.editorLevelName.Equals(""))
        {
            levelNameText.text = GameInfo.info.editorLevelName;
            LoadLevel();
        }
    }

	void Update()
	{
		DrawSelectionBox();

		//Is the cursor over a gui element
		bool onGui = eSys.IsPointerOverGameObject();

		//Rotate with rmb
		if(Input.GetKeyDown("e") && !onGui)
		{
			rotationDirection++;
			if(rotationDirection > 3)
				rotationDirection = 0;
		}

		//Spawn object when releasing lmb
		if(Input.GetMouseButtonUp(0) && !Input.GetKey(KeyCode.LeftAlt) && !onGui)
		{
			//Spawn object at mouse release point
			Vector3 pos = GetMouseOnSelectionPlane();
			SpawnPrefab(selectedPrefab, pos, Quaternion.Euler(0f, 0f, rotationDirection * 90f));
		}

		//Remove object with shift + lmb
		if(Input.GetKey(KeyCode.LeftAlt) && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && !onGui)
		{
			RemoveSelectedObject();
		}

		//Quit with esc if in test mode
		if(Input.GetButtonDown("Menu"))
		{
			EndTest();
		}

		//Move spawn plane with scroll wheel
        if (!onGui)
        {
            float scrollCount = Input.GetAxis("Mouse ScrollWheel");
            if (Input.GetKey(KeyCode.LeftShift))
            {
                EditorObjects.OBJ.AddSelectionPlanePosition(new Vector3(0f, scrollCount * 100f * EditorObjects.OBJ.verticalGridScale, 0f));
            }
            else
            {
                EditorObjects.OBJ.AddSelectionPlanePosition(new Vector3(0f, scrollCount * 10f * EditorObjects.OBJ.verticalGridScale, 0f));
            }
        }
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
		if(!selectionPos.Equals(NaV) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftAlt) && snapToGrid)
		{
			Vector3 roundedSelectionPos = EditorObjects.RoundVectorToGrid(selectionPos, EditorObjects.OBJ.gridScale, EditorObjects.OBJ.verticalGridScale);
			Quaternion selectionRot = Quaternion.Euler(0f, 0f, rotationDirection * 90f);

			if(selectionBox == null)
			{
				//Create a new box/object display
				GameObject boxObj = selectionBoxPrefab;
				if(selectedPrefab != null) { boxObj = selectedPrefab; }
				selectionBox = CreateSelectionBoxObject(boxObj, roundedSelectionPos, selectionRot);
			}
			else
			{
				string selectionName = "SelectionBox";
				if(selectedPrefab != null)
					selectionName = selectedPrefab.name;

				//Check if selected object changed
				if(selectionBox.name.Equals(selectionName))
				{
					//Object didn't change, just move the preview
					selectionBox.transform.position = roundedSelectionPos + EditorObjects.OBJ.GetObjectOffsetByName(selectionBox.name);
					selectionBox.transform.localRotation = Quaternion.Euler(EditorObjects.OBJ.GetObjectRotationByName(selectionBox.name)) * selectionRot;
				}
				else
				{
					//Object changed, spawn new object
					GameObject.Destroy(selectionBox);

					GameObject boxObj = selectionBoxPrefab;
					if(selectedPrefab != null) { boxObj = selectedPrefab; }
					selectionBox = CreateSelectionBoxObject(boxObj, roundedSelectionPos, selectionRot);
				}
			}
		}
		else
		{
			GameObject.Destroy(selectionBox);
			selectionBox = null;
		}
	}

	private GameObject CreateSelectionBoxObject(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		Quaternion newRot = Quaternion.Euler(EditorObjects.OBJ.GetObjectRotationByName(prefab.name)) * rot;
		
		GameObject instance = (GameObject)GameObject.Instantiate(prefab, pos, newRot);
		instance.transform.localScale = EditorObjects.OBJ.GetObjectScaleByName(prefab.name);
		instance.transform.localPosition += EditorObjects.OBJ.GetObjectOffsetByName(prefab.name);
		instance.name = prefab.name;

		return instance;
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

	//Script-intern way to toggle snap (updates button too)
	private void SetSnap(bool doSnap)
	{
		snapToGrid = doSnap;
		snapToggle.isOn = doSnap;
	}

	public void ClearLevel()
	{
		EditorObjects.OBJ.DestroyAllObjects();
	}

	//Start playing the level
	public void TestLevel()
	{
		GameObject startSpawn = EditorObjects.OBJ.GetStartSpawn();
		if(startSpawn != null)
		{
			GameInfo.info.spawnNewPlayer(startSpawn.GetComponent<Respawn>(), true, true);
			GameInfo.info.getPlayerInfo().prepareRace(1f);
		}
		SetInterfaceActive(false);
		GameInfo.info.unlockMenu();
		GameInfo.info.SetMenuState(GameInfo.MenuState.editorplay);
		GameInfo.info.lockMenu();
	}

	public void SaveLevel()
	{
		EditorObjects.OBJ.SaveLevelToFile(Application.dataPath + "/" + levelNameText.text + ".vlvl");
	}

	public void LoadLevel()
	{
		ClearLevel();
		LevelData data = EditorObjects.OBJ.LoadLevelFromFile(Application.dataPath + "/" + levelNameText.text + ".vlvl");

		foreach(ObjectData oData in data.levelObjects)
		{
			SpawnPreparedPrefab(EditorObjects.OBJ.GetPrefabByName(oData.name), oData.position, Quaternion.Euler(oData.eulerRotation), oData.scale);
		}
	}

	//Stop playing the level
	public void EndTest()
	{
		GameInfo.info.setPlayerInfo(null);
		GameInfo.info.cleanUpPlayer();
		SetInterfaceActive(true);
		GameInfo.info.unlockMenu();
		GameInfo.info.SetMenuState(GameInfo.MenuState.editor);
		GameInfo.info.lockMenu();
	}

	//Selects the prefab by name
	public void SelectPrefab(string name)
	{
		selectedPrefab = EditorObjects.OBJ.GetPrefabByName(name);
		prefabText.text = name;
	}

	//Instantiates a prefab with already corrected values
	private void SpawnPreparedPrefab(GameObject prefab, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		GameObject instance = (GameObject)GameObject.Instantiate(prefab, pos, rot);
		instance.transform.localScale = scale;
		instance.name = prefab.name;
		EditorObjects.OBJ.AddNonGridObject(instance);
	}

	//Instantiates a prefab
	private void SpawnPrefab(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		//Check if we have a valid position and object
		if(!pos.Equals(NaV) && prefab != null)
		{
			Vector3 newPos = pos;

			//Rotate the object to it's saved rotation
			Quaternion newRot = Quaternion.Euler(EditorObjects.OBJ.GetObjectRotationByName(prefab.name)) * rot;

			//Round position to grid
			if(snapToGrid)
			{
				newPos = EditorObjects.RoundVectorToGrid(newPos, EditorObjects.OBJ.gridScale, EditorObjects.OBJ.verticalGridScale);
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
			GameObject instance = (GameObject)GameObject.Instantiate(prefab, newPos, newRot);
			instance.transform.localScale = EditorObjects.OBJ.GetObjectScaleByName(prefab.name);
			
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

			//Offset object
			instance.transform.localPosition += EditorObjects.OBJ.GetObjectOffsetByName(prefab.name);
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

	public Texture2D GetObjectPreview(string name)
	{
		foreach(previewImageElement el in previewImages)
		{
			if(el.name.Equals(name))
			{
				return el.image;
			}
		}
		return null;
	}

	//Casts a ray from the camera into the direction of the mouse cursor
	private bool MouseRaycast(LayerMask layers, out RaycastHit hit, float length = Mathf.Infinity)
	{
		Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, GetComponent<Camera>().nearClipPlane);
		Vector3 worldPos = GetComponent<Camera>().ScreenToWorldPoint(mousePos);
		Vector3 camPos = GetComponent<Camera>().transform.position;
		Vector3 rayDirection = worldPos - camPos;
		Ray clickRay = new Ray(camPos, rayDirection);
		return Physics.Raycast(clickRay, out hit, length, layers);
	}

	public void ExitEditor()
	{
        GameInfo.info.loadLevel("MainMenu");
	}

	private Vector3 SimpleVectorMultiply(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

    public FileSelection GetFileSelection()
    {
        return fileSel;
    }
}
