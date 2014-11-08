using UnityEngine;
using System.Collections;

public class EditorCam : MonoBehaviour
{
	void Update()
	{
		Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * Time.deltaTime;
		Quaternion viewRot = transform.rotation;
		transform.position = transform.position + viewRot * input;
	}
}
