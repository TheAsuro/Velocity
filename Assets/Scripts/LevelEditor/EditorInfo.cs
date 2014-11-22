using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorInfo : MonoBehaviour
{
	public LayerMask clickLayers;
	public GameObject selectionBoxPrefab;

	private GameObject selectedPrefab;

	//GUI
	private Transform canvasT;
	private Transform topT;
	private InputField prefabText;
	private Toggle snapToggle;
	private InputField snapInput;
	private GameObject selectionPlane;

	//Selection
	private GameObject selectionBox = null;
	private Vector3 selectionStartPos = NaV;

	private static Vector3 NaV = new Vector3(float.NaN, float.NaN, float.NaN);

	private bool snapToGrid = true;
	private float snapValue = 1f;

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
		prefabText.onSubmit.AddListener(PrefabSubmit);
		snapToggle = topT.Find("SnapToGrid").GetComponent<Toggle>();
		snapInput = topT.Find("SnapInput").GetComponent<InputField>();
	}

	void Update()
	{
		//Draw box on mouse selection
		Vector3 selectionPos = GetMouseOnSelectionPlane();

		if(!selectionPos.Equals(NaV))
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

		//Start selection (or just click)
		if(Input.GetMouseButtonDown(0))
		{
			selectionStartPos = RoundToGrid(GetMouseOnSelectionPlane());
		}

		//Spawn with left click
		if(Input.GetMouseButtonUp(0) && selectedPrefab != null)
		{
			//Spawn in a rectangle if snap to grid is on
			if(snapToGrid)
			{
				Vector3 selectionEndPos = RoundToGrid(GetMouseOnSelectionPlane());

				if(!selectionStartPos.Equals(NaV) && !selectionEndPos.Equals(NaV))
				{
					Vector3 difference = selectionEndPos - selectionStartPos;

					for(int i = 0; i <= Mathf.Abs(difference.x); i++)
					{
						for(int j = 0; j <= Mathf.Abs(difference.z); j++)
						{
							Vector3 pos = selectionStartPos + new Vector3(i * Mathf.Sign(difference.x), 0f, j * Mathf.Sign(difference.z));
							SpawnPrefab(selectedPrefab, pos, Quaternion.identity, true);
						}
					}
				}
			}
			else //Spawn only a single entity
			{
				Vector3 pos = GetMouseOnSelectionPlane();
				if(!pos.Equals(NaV))
				{
					SpawnPrefab(selectedPrefab, pos, Quaternion.identity, true);
				}
			}
		}

		//Move spawn plane with scroll wheel
		float scrollCount = Input.GetAxis("Mouse ScrollWheel");
		EditorObjects.OBJ.AddSelectionPlanePosition(new Vector3(0f, scrollCount * 10f * snapValue, 0f));
	}

	//Called when player submits the selection textbox
	private void PrefabSubmit(string sub)
	{
		SelectPrefab(sub);
	}

	//Toggles snap on/off depending on gui button
	public void UpdateSnap()
	{
		snapToGrid = snapToggle.isOn;
	}

	//Public function that also scales the selection plane material
	public void UpdateSnapValue()
	{
		SetSnapValue(float.Parse(snapInput.text.text));
		EditorObjects.OBJ.SetSelectionPlaneRelativeMaterialScale(1f / snapValue);
	}

	//Selects the prefab by name
	private void SelectPrefab(string name)
	{
		selectedPrefab = EditorObjects.OBJ.GetPrefabByName(name);
	}

	//Instantiates a new prefab
	private void SpawnPrefab(GameObject prefab, Vector3 pos, Quaternion rot, bool alignToGround = false, bool overrideSnap = false)
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
		EditorObjects.OBJ.AddObject(instance);
	}

	//Get the position the mouse is over
	private Vector3 GetMouseOnSelectionPlane()
	{
		Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.nearClipPlane);
		Vector3 worldPos = camera.ScreenToWorldPoint(mousePos);
		Vector3 camPos = camera.transform.position;
		Vector3 rayDirection = worldPos - camPos;
		Ray clickRay = new Ray(camPos, rayDirection);
		RaycastHit hitInfo;
		bool hit = Physics.Raycast(clickRay, out hitInfo, Mathf.Infinity, clickLayers);
		if(!hit)
			return NaV;

		return hitInfo.point;
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
