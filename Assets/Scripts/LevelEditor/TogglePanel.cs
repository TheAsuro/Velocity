using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TogglePanel : MonoBehaviour
{
	public GameObject objectToToggle;

	void Awake()
	{
		Toggle t = GetComponent<Toggle>();
		if(t != null)
			t.onValueChanged.AddListener(SetValue);
	}

	private void SetValue(bool value)
	{
		objectToToggle.SetActive(value);
	}
}
