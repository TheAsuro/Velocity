using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DemoRecord : MonoBehaviour
{

	private bool recording = false;
	private Dictionary<float,Vector3> posList = new Dictionary<float,Vector3>();

	void Update()
	{
		if(recording)
		{
			posList.Add(Time.time, transform.position);
		}
	}
}