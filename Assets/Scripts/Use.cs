using UnityEngine;
using System.Collections;

public class Use : MonoBehaviour
{
	public LayerMask detectLayers;

	void Update()
	{
		if(Input.GetButtonDown("Use"))
		{
			Vector3 startPos = transform.position;
			Vector3 direction = transform.forward;
			Debug.DrawRay(startPos, direction);
			RaycastHit info;
			if(Physics.Raycast(startPos, direction, out info, 3f, detectLayers))
			{
				switch(info.collider.gameObject.tag)
				{
					case "PressButton":
						fireEvent(info.collider.gameObject, null);
						break;
				}
			}
		}
	}

	private void fireEvent(GameObject obj, params object[] parameters)
	{
		obj.GetComponent<Event>().fire(parameters);
	}
}
