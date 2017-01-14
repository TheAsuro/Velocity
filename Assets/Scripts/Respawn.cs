﻿using UnityEngine;

//Player resets to one of these points if r is pressed, handeled by WorldInfo
public class Respawn : MonoBehaviour
{
	public bool isFistSpawn = false;
	public bool respawnPositionIsRelative = true;
	public bool respawnRotationIsRelative = true;
	public Vector3 respawnPosition;
	public float respawnYRotation = 0f;

    private void Start()
	{
		if(isFistSpawn)
		{
            if (WorldInfo.info == null)
                throw new System.InvalidOperationException("This scene does not have a WoldInfo object! Game can not initialize.");

			WorldInfo.info.SetSpawn(this);
		}
	}

    private void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag.Equals("Player"))
		{
			WorldInfo.info.SetSpawn(this);
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
