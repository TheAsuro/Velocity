using UnityEngine;
using System.Collections;

public class Turn : MonoBehaviour
{
	public float degrees = 360;
	public float time = 1f;
	public axis rotationAxis = axis.Y;

	public enum axis
	{
		X,
		Y,
		Z
	}

	void Update()
	{
		float rotationAmount = (Time.time / time) * degrees;
		Vector3 rotationVector;

		if(rotationAxis == axis.X)
		{
			rotationVector = new Vector3(rotationAmount, transform.rotation.y, transform.rotation.z);
		}
		else if(rotationAxis == axis.Y)
		{
			rotationVector = new Vector3(transform.rotation.x, rotationAmount, transform.rotation.z);
		}
		else
		{
			rotationVector = new Vector3(transform.rotation.x, transform.rotation.y, rotationAmount);
		}

		transform.rotation = Quaternion.Euler(rotationVector);
	}
}
