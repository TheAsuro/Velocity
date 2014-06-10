using UnityEngine;
using System.Collections;

public class Grab : MonoBehaviour
{
	public float grabDistance = 3f;
	public float moveForce = 3f;
	public LayerMask layers;
	public bool glitched = false;

	private GameObject camObj;
	private GameObject grabbedObject;
	private float objectDistance;

	void Awake()
	{
		camObj = transform.FindChild("Camera").gameObject;
	}

	void Update()
	{
		//Grab or let go an object
		if(Input.GetButtonDown("Use"))
		{
			if(grabbedObject == null)
			{
				Ray ray = new Ray(transform.position, camObj.transform.forward);
				RaycastHit hitInfo;
				Physics.Raycast(ray, out hitInfo, grabDistance, layers);
				Debug.DrawRay(ray.origin,ray.direction * grabDistance,Color.blue, 0.2f);
				
				if(hitInfo.rigidbody != null)
				{
					grab(hitInfo.rigidbody.gameObject);
				}
			}
			else
			{
				release();
			}
		}
		
		//Move the object with the view
		if(grabbedObject != null)
		{
			Vector3 newPos = camObj.transform.position + camObj.transform.forward * objectDistance;
			Vector3 moveVelocity = (newPos - grabbedObject.transform.position) * moveForce;
			grabbedObject.rigidbody.velocity = moveVelocity;
		}
	}
	
	private void grab(GameObject obj)
	{
		grabbedObject = obj;
		objectDistance = Vector3.Magnitude(grabbedObject.transform.position - camObj.transform.position);
		grabbedObject.rigidbody.useGravity = false;
	}
	
	private void release()
	{
		grabbedObject.rigidbody.useGravity = true;
		grabbedObject = null;
	}
	
	void OnCollisionEnter(Collision other)
	{
		if(other.gameObject == grabbedObject && !glitched)
		{
			release();
		}
	}
	
	void OnCollisionStay(Collision other)
	{
		if(other.gameObject == grabbedObject && !glitched)
		{
			release();
		}
	}
}
