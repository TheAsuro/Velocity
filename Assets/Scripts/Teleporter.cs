using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
	public Vector3 target;
	public bool cancelVelocity = false;
	public bool applyRotation = false;
	public Quaternion targetRotation;
}
