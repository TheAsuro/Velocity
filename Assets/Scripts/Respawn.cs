using System;
using UnityEngine;

//Player resets to one of these points if r is pressed
public class Respawn : MonoBehaviour
{
	public bool respawnPositionIsRelative = true;
	public bool respawnRotationIsRelative = true;
	public Vector3 respawnPosition;
	public float respawnYRotation = 0f;

    public event EventHandler OnPlayerTrigger;

    private void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag.Equals("Player") && OnPlayerTrigger != null)
		{
			OnPlayerTrigger(this, new EventArgs());
		}
	}

	public Vector3 GetSpawnPos()
	{
		if(respawnPositionIsRelative)
		{
			return transform.position + respawnPosition;
		}
		else
		{
			return respawnPosition;
		}
	}

	public Quaternion GetSpawnRot()
	{
		Vector3 respawnRotation = new Vector3(0f, respawnYRotation, 0f);
		if(respawnRotationIsRelative)
		{
			return transform.rotation * Quaternion.Euler(respawnPosition);
		}
		else
		{
			return Quaternion.Euler(respawnRotation);
		}
	}
}
