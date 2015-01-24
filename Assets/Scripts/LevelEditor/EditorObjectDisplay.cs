using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class EditorObjectDisplay : MonoBehaviour
{
	private GameObject displayedObject;

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
}