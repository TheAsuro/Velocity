using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
	private InputField snapInput;

	//Selection
	private GameObject selectionBox = null;
	private Vector3 selectionStartPos = NaV; //unused now, may be useful for area spawning

	//Thing that tells you something went wrong
	public static Vector3 NaV = new Vector3(float.NaN, float.NaN, float.NaN);

	//Values set by GUI
	private bool snapToGrid = true;
	private float snapValue = 1f;

	void Awake()
	{
		canvasT = transform.parent.Find("Canvas");
		topT = canvasT.Find("Top");
		prefabText = topT.Find("PrefabInput").GetComponent<InputField>();
		prefabText.onEndEdit.AddListener(PrefabSubmit);
		snapToggle = topT.Find("SnapToGrid").GetComponent<Toggle>();
		snapInput = topT.Find("SnapInput").GetComponent<InputField>();
	}

	void Update()
	{
		DrawSelectionBox();

		//Start selection with lmb
		if(Input.GetMouseButtonDown(0))
		{
			selectionStartPos = GetMouseOnSelectionPlane();
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

		//Move spawn plane with scroll wheel
		float scrollCount = Input.GetAxis("Mouse ScrollWheel");
		EditorObjects.OBJ.AddSelectionPlanePosition(new Vector3(0f, scrollCount * 10f * snapValue, 0f));
	}

	//Draws a box where the cursor is/a rectangle of the selected area (TODO)
	private void DrawSelectionBox()
	{
		Vector3 selectionPos = GetMouseOnSelectionPlane() + new Vector3(0f, selectionBoxPrefab.transform.localScale.y / 2f, 0f);

		//Draw box only when cursor is on a valid square, rmb and shift are not pressed and snap to grid
		if(!selectionPos.Equals(NaV) && !Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftShift) && snapToGrid)
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
			
			EditorObjects.OBJ.RemoveObjectFromGrid(transformToDelete.position);
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

	//Instantiates a new prefab
	private void SpawnPrefab(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		if(!pos.Equals(NaV))
		{
			Vector3 newPos = pos;

			if(prefab.collider != null)
			{
				float additionalHeight = prefab.collider.bounds.extents.y;
				newPos = new Vector3(pos.x, pos.y + additionalHeight, pos.z);
			}

			if(snapToGrid)
			{
				newPos = RoundToGrid(newPos);
			}

			Vector3[] prefabExtents = EditorObjects.OBJ.GetObjectExtentsByName(prefab.name);
			foreach(Vector3 ex in prefabExtents)
			{
				Vector3 actualPos = newPos + ex;
				if(!EditorObjects.OBJ.IsGridPositionFree(actualPos))
					return;
			}
			
			GameObject instance = (GameObject)GameObject.Instantiate(prefab, newPos, rot);

			EditorObjects.OBJ.AddObjectToGrid(instance);
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
