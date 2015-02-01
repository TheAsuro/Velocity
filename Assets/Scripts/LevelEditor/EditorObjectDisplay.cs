using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorObjectDisplay : MonoBehaviour
{
	private GameObject displayedObject;
	private EditorInfo eInfo;

	void Awake()
	{
		eInfo = GameObject.Find("Camera").GetComponent<EditorInfo>();
		GetComponent<Button>().onClick.AddListener(OnClick);
	}

	public void SetText(string text)
	{
		transform.Find("Text").gameObject.GetComponent<Text>().text = text;
	}

	public void SetImage(Texture texture)
	{
		transform.Find("RawImage").gameObject.GetComponent<RawImage>().texture = texture;
	}

	public void SetObject(GameObject obj)
	{
		displayedObject = obj;
		SetText(obj.name);
	}

	private string GetText()
	{
		return transform.Find("Text").gameObject.GetComponent<Text>().text;
	}

	private void OnClick()
	{
		if(GetText() != "")
		{
			eInfo.SelectPrefab(GetText());
		}
	}
}