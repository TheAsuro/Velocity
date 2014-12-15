using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorInfo : MonoBehaviour
{
	public LayerMask spawnLayers;
	public LayerMask groundLayers;
	public LayerMask removeLayers;
	public GameObject selectionBoxPrefab;
	public GameObject groundPrefab;

	private GameObject selectedPrefab;

	//GUI
	private Transform canvasT;
	private Transform topT;
	private InputField prefabText;
	private Toggle snapToggle;
	private Toggle placeModeToggle;
	private InputField snapInput;

	//Selection
	private GameObject selectionBox = null;
	private Vector3 selectionStartPos = NaV;

	private static Vector3 NaV = new Vector3(float.NaN, float.NaN, float.NaN);

	private bool snapToGrid = true;
	private float snapValue = 1f;
	private PlaceMode currentPlaceMode = PlaceMode.objects;

	public enum PlaceMode
	{
		ground,
		objects
	}

	void Awake()
	{
		canvasT = transform.parent.Find("Canvas");
		topT = canvasT.Find("Top");
		prefabText = topT.Find("PrefabInput").GetComponent<InputField>();
		prefabText.onEndEdit.AddListener(PrefabSubmit);
		placeModeToggle = topT.Find("PlaceMode").GetComponent<Toggle>();
		snapToggle = topT.Find("SnapToGrid").GetComponent<Toggle>();
		snapInput = topT.Find("SnapInput").GetComponent<InputField>();
	}

	void Update()
	{
		DrawSelectionBox();

		//Start selection with lmb
		if(Input.GetMouseButtonDown(0))
		{
			switch(currentPlaceMode)
			{
				case PlaceMode.ground:
					selectionStartPos = GetMouseOnSelectionPlane(groundLayers);
					break;
				default:
					selectionStartPos = GetMouseOnSelectionPlane();
					break;
			}			
		}

		//Spawn ground/object when releasing lmb
		if(Input.GetMouseButtonUp(0))
		{
			if(currentPlaceMode == PlaceMode.ground)
			{
				//Spawn ground area from mouse down point to mouse up point
				Vector3 selectionEndPos = GetMouseOnSelectionPlane(groundLayers);
				SpawnGround(selectionStartPos, selectionEndPos);
			}
			else if(currentPlaceMode == PlaceMode.objects && selectedPrefab != null)
			{
				//Spawn object at mouse release point
				Vector3 pos = GetMouseOnSelectionPlane();
				SpawnPrefab(selectedPrefab, pos, Quaternion.identity, true);
			}
		}

		//Remove object with shift + lmb
		if(Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonUp(1))
		{
			RemoveSelectedObject();
		}

		//Move spawn plane with scroll wheel
		float scrollCount = Input.GetAxis("Mouse ScrollWheel");
		EditorObjects.OBJ.AddSelectionPlanePosition(new Vector3(0f, scrollCount * 10f * snapValue, 0f));
	}

	//Draws a box where the cursor is/a rectangle of the selected area (TODO)
	private void DrawSelectionBox()
	{
		Vector3 selectionPos = GetMouseOnSelectionPlane() + new Vector3(0f, selectionBoxPrefab.transform.localScale.y / 2f, 0f);

		//Draw box only when cursor is on a valid square, rmb and shift are not pressed
		// and snap to grid is active or we are placing ground
		if(!selectionPos.Equals(NaV) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftShift) && (snapToGrid || currentPlaceMode == PlaceMode.ground))
		{
			Vector3 roundedSelectionPos = RoundToGrid(selectionPos);

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

	//Toggles ground mode on/off (gui button)
	public void UpdateGroundMode()
	{
		if(placeModeToggle != null)
		{
			if(placeModeToggle.isOn)
			{
				currentPlaceMode = PlaceMode.ground;
			}
			else
			{
				currentPlaceMode = PlaceMode.objects;
			}
		}
	}

	//Public function that also scales the selection plane material
	public void UpdateSnapValue()
	{
		SetSnapValue(float.Parse(snapInput.text));
		EditorObjects.OBJ.SetSelectionPlaneRelativeMaterialScale(1f / snapValue);
	}

	//Selects the prefab by name
	private void SelectPrefab(string name)
	{
		selectedPrefab = EditorObjects.OBJ.GetPrefabByName(name);
	}

	//Creates ground in a specified rectangle (auto snap to grid)
	private void SpawnGround(Vector3 start, Vector3 end)
	{
		if(!start.Equals(NaV) && !end.Equals(NaV))
		{
			//Round start and end point
			Vector3 snapStart = RoundToGrid(start);
			Vector3 snapEnd = RoundToGrid(end);

			//Calculate difference and add 1 to the size
			Vector3 diff = snapEnd - snapStart;
			Vector3 addVector = new Vector3(Mathf.Sign(diff.x), Mathf.Sign(diff.y), Mathf.Sign(diff.z));

			//Spawn the ground object in the center of the rectangle and add it's saved location
			Vector3 newPos = snapStart + (diff) / 2f + groundPrefab.transform.position;

			//Scale the ground object to the size of the rectangle but keep it's saved size
			Vector3 newScale = SimpleVectorMultiply(snapEnd - snapStart + addVector, groundPrefab.transform.localScale);

			//Don't rotate the ground
			Quaternion newRot = Quaternion.identity;

			//Instantiate it
			GameObject groundInstance = (GameObject)GameObject.Instantiate(groundPrefab, newPos, newRot);
			groundInstance.transform.localScale = newScale;
		}
	}

	//Instantiates a new prefab
	private void SpawnPrefab(GameObject prefab, Vector3 pos, Quaternion rot, bool alignToGround = false, bool overrideSnap = false)
	{
		if(!pos.Equals(NaV))
		{
			Vector3 newPos = pos;

			if(alignToGround && prefab.collider != null)
			{
				float additionalHeight = prefab.collider.bounds.extents.y;
				newPos = new Vector3(pos.x, pos.y + additionalHeight, pos.z);
			}

			if(snapToGrid && !overrideSnap)
			{
				newPos = RoundToGrid(newPos);
			}
			
			GameObject instance = (GameObject)GameObject.Instantiate(prefab, newPos, rot);
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

	//Set the value that RoundToGrid will snap to
	public void SetSnapValue(float value)
	{
		snapValue = value;
	}

	//Round a vector
	private Vector3 RoundToGrid(Vector3 input)
	{
		return new Vector3(RoundToGrid(input.x), input.y, RoundToGrid(input.z));
	}

	private Vector3 SimpleVectorMultiply(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	//Round a float so it snaps to the grid
	private float RoundToGrid(float input)
	{
		//rip
		if(input.Equals(float.NaN))
			return float.NaN;

		//Find out the next lowest mutliple of the grid
		int sign = (int)Mathf.Sign(input);
		int counter = 0;
		float lowResult = float.NaN;
		float highResult = float.NaN;
		bool hit = false;

		while(!hit)
		{
			float i = counter * sign * snapValue;

			if(i == input)
			{
				hit = true;
				lowResult = counter * snapValue * sign;
			}
			else if(i > input && sign == 1)
			{
				hit = true;
				lowResult = (counter - 1) * snapValue * sign;
			}
			else if(i < input && sign == -1)
			{
				hit = true;
				lowResult = counter * snapValue * sign;
			}

			counter++;
		}

		//Decide in what direction to round
		highResult = lowResult + snapValue;

		if(input >= lowResult + snapValue / 2f)
		{
			return highResult;
		}
		return lowResult;
	}
}
