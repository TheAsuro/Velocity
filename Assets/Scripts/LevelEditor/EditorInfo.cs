using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorInfo : MonoBehaviour
{
	public LayerMask clickLayers;

	private GameObject loadedPrefab;

	//GUI
	private Transform canvasT;
	private Transform topT;
	private Transform objectButtonT;
	private Text objectButtonText;
	private Transform objectSelectionT;
	private InputField prefabText;
	private Toggle snapToggle;

	private static Vector3 NaV = new Vector3(float.NaN, float.NaN, float.NaN);

	private bool snapToGrid;

	void Awake()
	{
		canvasT = transform.parent.Find("Canvas");
		topT = canvasT.Find("Top");
		objectButtonT = topT.Find("ObjectButton");
		objectButtonText = objectButtonT.Find("Text").GetComponent<Text>();
		objectSelectionT = canvasT.Find("ObjectSelection");
		prefabText = topT.Find("PrefabInput").GetComponent<InputField>();
		prefabText.onSubmit.AddListener(PrefabSubmit);
		snapToggle = topT.Find("SnapToGrid").GetComponent<Toggle>();
	}

	void Update()
	{
		//Spawn with left click
		if(Input.GetMouseButtonDown(0))
		{
			if(loadedPrefab != null)
				SpawnPrefabAtMouse(loadedPrefab);
		}

		//Move spawn plane with scroll wheel
		float scrollCount = Input.GetAxis("Mouse ScrollWheel");
		EditorObjects.OBJ.AddSelectionPlanePosition(new Vector3(0f, scrollCount * 10f, 0f));
	}

	private void PrefabSubmit(string sub)
	{
		LoadPrefab(sub);
	}

	public void UpdateSnap()
	{
		snapToGrid = snapToggle.isOn;
	}

	public void ToggleObjectSelection()
	{
		if(objectSelectionT.gameObject.activeSelf)
		{
			objectSelectionT.gameObject.SetActive(false);
			objectButtonText.text = "˅";
		}
		else
		{
			objectSelectionT.gameObject.SetActive(true);
			objectButtonText.text = "˄";
		}
	}

	private void LoadPrefab(string name)
	{
		loadedPrefab = (GameObject)Resources.Load(name, typeof(GameObject));
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

	private float RoundToGrid(float input)
	{
		float floor = Mathf.Floor(input);
		float rest = input - floor;
		if(rest < 0.5f)
			return floor;

		return Mathf.Ceil(input);
	}
}
