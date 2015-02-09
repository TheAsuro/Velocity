using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorObjectDisplay : MonoBehaviour
{
	void Awake()
	{
		GetComponent<Button>().onClick.AddListener(OnClick);
	}

	public void SetText(string text)
	{
		transform.Find("Text").gameObject.GetComponent<Text>().text = text;
	}

	public void SetImage(Texture texture)
	{
		if(texture != null)
			transform.Find("RawImage").gameObject.GetComponent<RawImage>().texture = texture;
	}

	public void SetObject(GameObject obj)
	{
		SetText(obj.name);
		SetImage(EditorInfo.info.GetObjectPreview(obj.name));
	}

	private string GetText()
	{
		return transform.Find("Text").gameObject.GetComponent<Text>().text;
	}

	private void OnClick()
	{
		if(GetText() != "")
		{
			EditorInfo.info.SelectPrefab(GetText());
		}
	}
}