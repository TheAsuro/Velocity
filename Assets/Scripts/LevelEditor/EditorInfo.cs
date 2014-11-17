using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorInfo : MonoBehaviour
{
	public LayerMask clickLayers;

	private GameObject selectedPrefab;

	//GUI
	private Transform canvasT;
	private Transform topT;
	private InputField prefabText;
	private Toggle snapToggle;
	private InputField snapInput;
	private GameObject selectionPlane;

	private static Vector3 NaV = new Vector3(float.NaN, float.NaN, float.NaN);

	private bool snapToGrid = true;
	private float snapValue = 1f;

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
		//Spawn with left click
		if(Input.GetMouseButtonDown(0))
		{
			if(selectedPrefab != null)
				SpawnPrefabAtMouse(selectedPrefab);
		}

		//Move spawn plane with scroll wheel
		float scrollCount = Input.GetAxis("Mouse ScrollWheel");
		EditorObjects.OBJ.AddSelectionPlanePosition(new Vector3(0f, scrollCount * 10f * snapValue, 0f));
	}

	private void PrefabSubmit(string sub)
	{
		SelectPrefab(sub);
	}

	public void UpdateSnap()
	{
		snapToGrid = snapToggle.isOn;
	}

	public void UpdateSnapValue()
	{
		SetSnapValue(float.Parse(snapInput.text.text));
		EditorObjects.OBJ.SetSelectionPlaneRelativeMaterialScale(1f / snapValue);
	}

	private void SelectPrefab(string name)
	{
		selectedPrefab = EditorObjects.OBJ.GetPrefabByName(name);
	}

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
			newPos = new Vector3(RoundToGrid(newPos.x), newPos.y, RoundToGrid(newPos.z));
		}
		
		GameObject instance = (GameObject)GameObject.Instantiate(prefab, newPos, rot);
		EditorObjects.OBJ.AddObject(instance);
	}

	private void SpawnPrefabAtMouse(GameObject prefab)
	{
		Vector3 pos = GetMouseOnSelectionPlane();
		if(!pos.Equals(NaV))
		{
			SpawnPrefab(prefab, pos, Quaternion.identity, true);
		}
	}

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

	public void SetSnapValue(float value)
	{
		snapValue = value;
	}

	private float RoundToGrid(float input)
	{
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
