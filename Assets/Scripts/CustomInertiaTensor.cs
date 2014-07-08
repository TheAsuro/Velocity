using UnityEngine;
using System.Collections;

public class CustomInertiaTensor : MonoBehaviour
{
	public Vector3 inertiaTensor = Vector3.one;

	void Start()
	{
		rigidbody.inertiaTensor = inertiaTensor;
	}
}
