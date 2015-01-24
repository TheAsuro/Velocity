using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EditorObjectDisplay : MonoBehaviour
{
	public void SetText(string text)
	{
		transform.Find("Text").gameObject.GetComponent<Text>().text = text;
	}

	public void SetImage(Texture texture)
	{
		transform.Find("RawImage").gameObject.GetComponent<RawImage>().texture = texture;
	}
}
